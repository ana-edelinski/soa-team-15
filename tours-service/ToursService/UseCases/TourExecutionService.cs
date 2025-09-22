using AutoMapper;
using FluentResults;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;
using ToursService.Dtos;
using ToursService.Repositories;

namespace ToursService.UseCases
{
    public class TourExecutionService : ITourExecutionService
    {
        private readonly ITourExecutionRepository _tourExecutionRepository;
        private readonly IMapper _mapper;

        public TourExecutionService(ITourExecutionRepository tourExecutionRepository, IMapper mapper)
        {
            _tourExecutionRepository = tourExecutionRepository;
            _mapper = mapper;
        }

        public Result<TourExecutionDto> Create(TourExecutionDto execution)
        {
            if (execution == null)
                return Result.Fail<TourExecutionDto>("Execution DTO is null.");

            try
            {
                var entity = _mapper.Map<TourExecution>(execution);

                var created = _tourExecutionRepository.Create(entity);
                if (created == null)
                    return Result.Fail<TourExecutionDto>("Failed to create tour execution.");

                created.StartTourExecution();
                var updated = _tourExecutionRepository.Update(created);
                if (updated == null)
                    return Result.Fail<TourExecutionDto>("Failed to update tour execution.");

                return Result.Ok(_mapper.Map<TourExecutionDto>(updated));
            }
            catch (Exception ex)
            {
                return Result.Fail<TourExecutionDto>($"EXCEPTION: {ex.Message}");
            }
        }

        public Result<TourExecutionDto> CompleteTourExecution(long id)
        {
            if (id <= 0)
                return Result.Fail<TourExecutionDto>("Invalid execution id.");

            try
            {
                var entity = _tourExecutionRepository.Get(id);
                if (entity == null)
                    return Result.Fail<TourExecutionDto>($"TourExecution with id {id} not found.");

                entity.CompleteTourExecution();
                var updated = _tourExecutionRepository.Update(entity);

                return Result.Ok(_mapper.Map<TourExecutionDto>(updated));
            }
            catch (Exception ex)
            {
                return Result.Fail<TourExecutionDto>($"EXCEPTION: {ex.Message}");
            }
        }

        public Result<TourExecutionDto> AbandonTourExecution(long id)
        {
            if (id <= 0)
                return Result.Fail<TourExecutionDto>("Invalid execution id.");

            try
            {
                var entity = _tourExecutionRepository.Get(id);
                if (entity == null)
                    return Result.Fail<TourExecutionDto>($"TourExecution with id {id} not found.");

                entity.AbandonTourExecution();
                var updated = _tourExecutionRepository.Update(entity);

                return Result.Ok(_mapper.Map<TourExecutionDto>(updated));
            }
            catch (Exception ex)
            {
                return Result.Fail<TourExecutionDto>($"EXCEPTION: {ex.Message}");
            }
        }

        public Result<TourExecutionDto> GetByTourAndTouristId(long touristId, long tourId)
        {
            var ex = _tourExecutionRepository.GetByTourAndTourist(touristId, tourId);
            if (ex != null)
            {
                return Result.Ok(_mapper.Map<TourExecutionDto>(ex));
            }

            return Result.Fail<TourExecutionDto>("TourExecution not found.");
        }

        public Result<TourExecutionDto> GetActiveTourByTouristId(long touristId)
        {
            var execution = _tourExecutionRepository.GetActiveTourByTourist(touristId);
            if (execution != null)
            {
                return Result.Ok(_mapper.Map<TourExecutionDto>(execution));
            }

            return Result.Fail<TourExecutionDto>($"No active tour found for tourist with ID {touristId}.");
        }

        public Result<TourExecutionDto> CompleteKeyPoint(long executionId, long keyPointId)
        {
            var execution = _tourExecutionRepository.Get(executionId);
            if (execution == null)
            {
                return Result.Fail<TourExecutionDto>($"Tour execution with ID {executionId} not found.");
            }

            if (!_tourExecutionRepository.KeyPointExists(keyPointId))
            {
                return Result.Fail<TourExecutionDto>($"Key point with ID {keyPointId} does not exist.");
            }

            try
            {
                execution.CompleteKeyPoint(keyPointId);
                _tourExecutionRepository.Update(execution);
                return Result.Ok(_mapper.Map<TourExecutionDto>(execution));
            }
            catch (ArgumentException ex)
            {
                return Result.Fail<TourExecutionDto>(ex.Message);
            }
        }

        public void UpdateLastActivity(long executionId)
        {
            var execution = _tourExecutionRepository.Get(executionId);
            if (execution != null)
            {
                execution.UpdateLastActivity();
                _tourExecutionRepository.Update(execution);
            }
            else
            {
                throw new Exception("Execution not found.");
            }
        }

        public ICollection<KeyPointDto> GetKeyPointsForTour(long tourId)
        {
            var keyPoints = _tourExecutionRepository.GetKeyPointsByTourId(tourId);

            var keyPointDtos = keyPoints.Select(kp => new KeyPointDto
            {
                Id = kp.Id,
                Name = kp.Name,
                Latitude = kp.Latitude,
                Longitude = kp.Longitude,
                Description = kp.Description
            }).ToList();

            return keyPointDtos;
        }
    }
}
