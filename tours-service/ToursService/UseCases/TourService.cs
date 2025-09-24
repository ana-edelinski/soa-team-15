using AutoMapper;
using FluentResults;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;
using ToursService.Dtos;
using ToursService.Integrations;
using Microsoft.EntityFrameworkCore;


namespace ToursService.UseCases
{
    public class TourService : ITourService
    {
        private readonly ITourRepository _tourRepository;
        private readonly IKeyPointRepository _keyPointRepository;
        private readonly IMapper _mapper;
        private readonly IPaymentClient _payments;
        private readonly ILogger<TourService> _log;

       
        public TourService(ITourRepository tourRepository, IMapper mapper, IKeyPointRepository keyPointRepository,ILogger<TourService> log, IPaymentClient payment)

        {
            _tourRepository = tourRepository;
            _keyPointRepository = keyPointRepository;
            _mapper = mapper;
            _log = log;
            _payments = payment;

        }

        public Result<TourDto> GetById(long tourId)
        {
            var t = _tourRepository.GetById(tourId);
            if (t == null) return Result.Fail($"Tour {tourId} not found");

            // Ako su ti domain enumi drugačiji, zamijeni nazive ispod po potrebi
            // Pretpostavka: Domain: TourStatus, TourTag; DTO: ToursService.Dtos.TourStatus, ToursService.Dtos.TourTags
            var dtoStatus = Enum.IsDefined(typeof(ToursService.Dtos.TourStatus), (int)t.Status)
                ? (ToursService.Dtos.TourStatus)(int)t.Status
                : default; // ili neki podrazumijevani status

            var safeTags = new List<ToursService.Dtos.TourTags>();
            if (t.Tags != null)
            {
                foreach (var tag in t.Tags)
                {
                    var intVal = Convert.ToInt32(tag); // pokriva slučaj da EF vrati int
                    if (Enum.IsDefined(typeof(ToursService.Dtos.TourTags), intVal))
                        safeTags.Add((ToursService.Dtos.TourTags)intVal);
                    // inače preskoči nepoznate tagove
                }
            }

            var dto = new TourDto
            {
                Id          = t.Id,
                Name        = t.Name,
                Description = t.Description,
                Difficulty  = t.Difficulty,
                Tags        = safeTags,
                UserId      = t.UserId,
                Status      = dtoStatus,
                Price       = t.Price,
                LengthInKm  = t.LengthInKm,
                PublishedTime = t.PublishedTime,
                ArchiveTime   = t.ArchiveTime
            };

            return Result.Ok(dto);
        }

       public Result<double> UpdateTourKM(long tourId, List<KeyPointDto> _)
        {
            
            try
            {
                var tour = _tourRepository.GetById(tourId);
                if (tour == null) return Result.Fail<double>("Tour not found.");

                var domainKeyPoints = _keyPointRepository.GetByTour(tourId) ?? new List<KeyPoint>();

                var km = tour.RecalculateLength(domainKeyPoints.ToList());
                tour.UpdateLength(km);

                _tourRepository.Update(tour);
                return Result.Ok(tour.LengthInKm);
            }
            catch (Exception ex)
            {
                return Result.Fail<double>($"Error updating tour length: {ex.Message}");
            }
        }

        public Result<TourDto> Create(TourDto dto)
        {
            if (dto is null)
                return Result.Fail<TourDto>("Tour DTO is null.");
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Result.Fail<TourDto>("Name is required.");
            if (dto.UserId <= 0)
                return Result.Fail<TourDto>("UserId must be a positive number.");

            try
            {
                var tour = _mapper.Map<Tour>(dto);
                var created = _tourRepository.Create(tour);
                var createdDto = _mapper.Map<TourDto>(created);

                return Result.Ok(createdDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<TourDto>($"EXCEPTION: {ex.Message}");
            }
        }

        public Result<List<KeyPointDto>> GetKeyPointsByTour(long tourId)
        {

             _log.LogInformation("GetKeyPointsByTour called {@Payload}", new { tourId });

             
            try
            {

                List<KeyPoint> keyPoints = _keyPointRepository.GetByTour(tourId);
                List<KeyPointDto> dtos = keyPoints.Select(kp => new KeyPointDto
                {
                    Id = kp.Id,
                    Name = kp.Name,
                    Longitude = kp.Longitude,
                    Latitude = kp.Latitude,
                    Description = kp.Description,
                    UserId = kp.UserId,
                    TourId = kp.TourId
                }).ToList();


                return Result.Ok(dtos);
            }
            catch (Exception ex)
            {
                return Result.Fail<List<KeyPointDto>>($"Error adding key point: {ex.Message}");
            }
        }



        public Result<KeyPointDto> AddKeyPoint(long tourId, KeyPointDto keyPointDto)
        {

             _log.LogInformation("AddKeyPoint called {@Payload}", new
            {
                tourId,
                keyPointDto?.Name,
                keyPointDto?.Latitude,
                keyPointDto?.Longitude
            });


            try
            {
                KeyPoint keyPoint = _mapper.Map<KeyPoint>(keyPointDto);
                keyPoint.TourId = tourId;

                // Handle file upload
                if (keyPointDto.PictureFile != null && keyPointDto.PictureFile.Length > 0)
                {
                    // Validate file type (optional but recommended)
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(keyPointDto.PictureFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return Result.Fail<KeyPointDto>("Invalid file type. Only image files are allowed.");
                    }

                    // Validate file size (optional - e.g., max 5MB)
                    if (keyPointDto.PictureFile.Length > 5 * 1024 * 1024)
                    {
                        return Result.Fail<KeyPointDto>("File size too large. Maximum size is 5MB.");
                    }

                    // Create directory if it doesn't exist
                    var uploadDir = Path.Combine("wwwroot", "images", "keypoints");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    // Generate unique filename
                    var fileName = $"keypoint_{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        keyPointDto.PictureFile.CopyToAsync(stream);
                    }

                    keyPoint.Image = fileName;
                }


                var created = _keyPointRepository.Create(keyPoint);
                var dto = _mapper.Map<KeyPointDto>(created);
                return Result.Ok(dto);
            }
            catch (Exception ex)
            {
                return Result.Fail<KeyPointDto>($"Error adding key point: {ex.Message}");
            }
        }


        

        public Result<List<TourDto>> GetByUserId(long userId)
        {
            {
                var tours = _tourRepository.GetByAuthor(userId);

                if (tours == null || tours.Count == 0)
                {
                    return Result.Fail<List<TourDto>>("No tours found for the specified user.");
                }

                var tourDtos = tours.Select(t => new TourDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Difficulty = t.Difficulty,
                    Tags = t.Tags.Select(tag => (ToursService.Dtos.TourTags)tag).ToList(),
                    UserId = t.UserId,
                    Status = (ToursService.Dtos.TourStatus)t.Status,
                    Price = t.Price,
                    LengthInKm = t.LengthInKm,
                    PublishedTime = t.PublishedTime,
                    ArchiveTime   = t.ArchiveTime
                    //KeyPoints = t.KeyPoints.Select(kp => new KeyPointDto
                    //{
                    //    Id = kp.Id,
                    //    Name = kp.Name,
                    //    Longitude = kp.Longitude,
                    //    Latitude = kp.Latitude,
                    //    Description = kp.Description,
                    //    Image = kp.Image,
                    //    TourId = kp.TourId


                    //}).ToList()
                }).ToList();

                return Result.Ok(tourDtos);

            }
        }




        







        public Result<List<TourPublicDto>> GetPublishedPublic()
            {
                var tours = _tourRepository.GetPublished();
                var list = new List<TourPublicDto>();

                foreach (var t in tours)
                {
                    var first = _keyPointRepository.GetByTour(t.Id)?
                        .OrderBy(kp => kp.Id)
                        .Select(kp => new KeyPointDto
                        {
                            Id = kp.Id,
                            Name = kp.Name,
                            Latitude = kp.Latitude,
                            Longitude = kp.Longitude,
                            Description = kp.Description,
                            Image = kp.Image,
                            UserId = kp.UserId, // možeš izbaciti ako ne želiš
                            TourId = kp.TourId
                        })
                        .FirstOrDefault();

                    list.Add(new TourPublicDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        Difficulty = t.Difficulty,
                        Tags = t.Tags.Select(tag => (ToursService.Dtos.TourTags)tag).ToList(),
                        Price = t.Price,
                        LengthInKm = t.LengthInKm,
                        PublishedAt = t.PublishedTime,   // mapiraš na DTO PublishedAt
                        FirstKeyPoint = first
                    });
                }

                return Result.Ok(list);
            }











        public Result<List<TourDto>> GetPublished()
        {
            try
            {
                var tours = _tourRepository.GetPublished();

                if (tours == null || tours.Count == 0)
                    return Result.Ok(new List<TourDto>());

                var dtos = new List<TourDto>();

                foreach (var t in tours)
                {
                    var keyPoints = _keyPointRepository.GetByTour(t.Id) ?? new List<KeyPoint>();
                    var first = keyPoints.OrderBy(kp => kp.Id).FirstOrDefault();

                    var previewImages = keyPoints
                        .Select(kp => kp.Image)
                        .Where(img => !string.IsNullOrWhiteSpace(img))
                        .Distinct()
                        .Take(4)
                        .ToList();

                    int? durationMinutes = null;
                    if (t.LengthInKm > 0)
                        durationMinutes = (int)Math.Round((t.LengthInKm / 4.0) * 60);

                    dtos.Add(new TourDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        Difficulty = t.Difficulty,
                        Tags = t.Tags.Select(tag => (ToursService.Dtos.TourTags)tag).ToList(),
                        UserId = t.UserId,
                        Status = (ToursService.Dtos.TourStatus)t.Status,
                        Price = t.Price,
                        LengthInKm = t.LengthInKm,
                        DurationMinutes = durationMinutes,
                        StartPointName = first?.Name,
                        PreviewImages = previewImages,
                        PublishedTime = t.PublishedTime,
                        ArchiveTime   = t.ArchiveTime
                    });
                }

                return Result.Ok(dtos);
            }
            catch (Exception ex)
            {
                return Result.Fail<List<TourDto>>($"EXCEPTION: {ex.Message}");
            }
        }



        public Result Publish(long tourId, long authorId)
        {
            var tour = _tourRepository.GetById(tourId);
            if (tour is null) return Result.Fail("Tour not found.");

            try
            {
                tour.Publish(authorId);   // domen validacije i promjena statusa
                _tourRepository.Update(tour);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result Archive(long tourId, long authorId)
        {
            var tour = _tourRepository.GetById(tourId);
            if (tour is null) return Result.Fail("Tour not found.");

            try
            {
                tour.Archive(authorId);
                _tourRepository.Update(tour);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result Reactivate(long tourId, long authorId)
        {
            var tour = _tourRepository.GetById(tourId);
            if (tour is null) return Result.Fail("Tour not found.");

            try
            {
                tour.Reactivate(authorId);
                _tourRepository.Update(tour);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result<TourForTouristDto> GetTourWithKeyPoints(long tourId)
        {
            var tour = _tourRepository.GetById(tourId);
            if (tour is null)
                return Result.Fail($"Tour with id {tourId} not found.");

            var keyPoints = _keyPointRepository.GetByTour(tourId) ?? Enumerable.Empty<KeyPoint>();

            // Ako imaš polje redosleda, zameni .OrderBy(k => k.Id) sa .OrderBy(k => k.OrderNo)
            var dto = new TourForTouristDto(tour, keyPoints.OrderBy(k => k.Id));

            return Result.Ok(dto);
        }





        public Result<List<TourDto>> GetAll()
        {
            try
            {
                var tours = _tourRepository.GetAll(); // već postoji u TourRepository
                var dtos = tours.Select(t => _mapper.Map<TourDto>(t)).ToList();
                return Result.Ok(dtos);
            }
            catch (Exception ex)
            {
                return Result.Fail(new Error(ex.Message));
            }
        }



        public Result<List<TourDto>> GetAllIncludingUnpublished()
        {
            try
            {
                var tours = _tourRepository.GetAllIncludingUnpublished();
                var dtos = tours.Select(t => _mapper.Map<TourDto>(t)).ToList();
                return Result.Ok(dtos);
            }
            catch (Exception ex)
            {
                return Result.Fail(new Error(ex.Message));
            }
        }




        public Result<TourPublicDto> GetPublicTour(long tourId)
        {

              _log.LogInformation("Tour is published {@Payload}", new { tourId });

            var t = _tourRepository.GetById(tourId);
            if (t is null || t.Status != Domain.TourStatus.Published)
                return Result.Fail<TourPublicDto>("Tour not found.");

            var first = _keyPointRepository.GetByTour(t.Id)?
                .OrderBy(k => k.Id)
                .Select(kp => new KeyPointDto
                {
                    Id = kp.Id,
                    Name = kp.Name,
                    Latitude = kp.Latitude,
                    Longitude = kp.Longitude,
                    Description = kp.Description,
                    Image = kp.Image,
                    UserId = kp.UserId,
                    TourId = kp.TourId
                })
                .FirstOrDefault();

            var dto = new TourPublicDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Difficulty = t.Difficulty,
                Tags = t.Tags.Select(tag => (ToursService.Dtos.TourTags)tag).ToList(),
                Price = t.Price,
                LengthInKm = t.LengthInKm,
                PublishedAt = t.PublishedTime,
                FirstKeyPoint = first
            };
            return Result.Ok(dto);
        }



        public async Task<Result<List<TourDto>>> GetPurchasedForUserAsync(long userId)
        {
            // 1) IDs iz payments-a
            var ids = await _payments.GetPurchasedIdsAsync(userId);
            if (ids == null || ids.Count == 0)
                return Result.Ok(new List<TourDto>());

            // 2) Ture iz tours-db
            var tours = await _tourRepository.GetByIdsAsync(ids);

           

            // 3) Mapiranje na DTO
            var dtos = _mapper.Map<List<TourDto>>(tours);
            return Result.Ok(dtos);
        }


    }

}
