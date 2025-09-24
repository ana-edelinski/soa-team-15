using AutoMapper;
using FluentResults;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;
using ToursService.Dtos;
using ToursService.Integrations.Saga;
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

        public async Task CompensateAsync(long executionId, string v, CancellationToken ct)
        {
            var exec = _tourExecutionRepository.Get(executionId);
            if (exec == null) return; 

            // Ako je još Pending → označi kao Rejected (ostaje trag u bazi)
            if (exec.Status == Domain.TourExecutionStatus.Pending)
            {
                exec.RejectPending();
                _tourExecutionRepository.Update(exec);
                await Task.CompletedTask;
                return;
            }

            // Ako je (teoretski) već Active, kompenzacija može biti "Abandon"?
            

            await Task.CompletedTask;
        }

        public async Task<long?> CreatePendingAsync(long touristId, long tourId, long locationId, string correlationId, CancellationToken ct)
        {
            var alreadyActive = _tourExecutionRepository.GetActiveTourByTourist(touristId);
            if (alreadyActive != null)
                return await Task.FromResult<long?>(null);   // ili baci izuzetak / Result.Fail → 409

            

            var now = DateTime.UtcNow;
            var pending = new TourExecution(
                tourId: tourId,
                touristId: touristId,
                locationId: locationId,
                lastActivity: now,
                status: Domain.TourExecutionStatus.Pending,
                completedKeys: new List<CompletedKeyPoint>()
            );

            // Korelacija (ako može da se upiše)
            if (Guid.TryParse(correlationId, out var sagaId))
            {
                try { pending.AttachSaga(sagaId); } catch { /* ako nema polja, ignore */ }
            }

            var created = _tourExecutionRepository.Create(pending);
            if (created == null) return await Task.FromResult<long?>(null);

            return await Task.FromResult<long?>(created.Id);
        }

        public async Task ActivateAsync(long executionId, CancellationToken ct)
        {
            var exec = _tourExecutionRepository.Get(executionId);
            if (exec == null) throw new KeyNotFoundException($"TourExecution {executionId} not found.");

            // ako je već ACTIVE, samo izađi
            if (exec.Status == Domain.TourExecutionStatus.Active) return;

            if (exec.Status != Domain.TourExecutionStatus.Pending)
                throw new InvalidOperationException(
                    $"Cannot activate execution {executionId} from status {exec.Status}.");

            // koristi postojeću domensku metodu (postavlja Active + LastActivity)
            exec.StartTourExecution();

            _tourExecutionRepository.Update(exec);
            await Task.CompletedTask;
        }
    }
}
