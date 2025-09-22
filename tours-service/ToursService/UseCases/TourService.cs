using AutoMapper;
using FluentResults;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;
using ToursService.Dtos;


namespace ToursService.UseCases
{
    public class TourService : ITourService
    {
        private readonly ITourRepository _tourRepository;
        private readonly IKeyPointRepository _keyPointRepository;
        private readonly IMapper _mapper;

        public TourService(ITourRepository tourRepository, IMapper mapper, IKeyPointRepository keyPointRepository)
        {
            _tourRepository = tourRepository;
            _keyPointRepository = keyPointRepository;
            _mapper = mapper;
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
                    //LengthInKm = t.LengthInKm,
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
                    // Učitamo KeyPoint-ove da izvedemo start i slike (ne vraćamo ih sve klijentu)
                    var keyPoints = _keyPointRepository.GetByTour(t.Id) ?? new List<KeyPoint>();
                    var first = keyPoints.OrderBy(kp => kp.Id).FirstOrDefault();

                    // Slike za preview (do 4). Pretpostavka: KeyPoint.Image sadrži naziv fajla/URL.
                    var previewImages = keyPoints
                        .Select(kp => kp.Image)
                        .Where(img => !string.IsNullOrWhiteSpace(img))
                        .Distinct()
                        .Take(4)
                        .ToList();

                    // Trajanje: ako nemaš polje u entitetu, procena po dužini (4 km/h)
                    int? durationMinutes = null;
                    if (t.LengthInKm > 0)
                    {
                        durationMinutes = (int)Math.Round((t.LengthInKm / 4.0) * 60);
                    }

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

                        // 👇 NOVO punjenje
                        LengthInKm = t.LengthInKm,
                        DurationMinutes = durationMinutes,
                        StartPointName = first?.Name,
                        PreviewImages = previewImages
                    });
                }

                return Result.Ok(dtos);
            }
            catch (Exception ex)
            {
                return Result.Fail<List<TourDto>>($"EXCEPTION: {ex.Message}");
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

    }

}
