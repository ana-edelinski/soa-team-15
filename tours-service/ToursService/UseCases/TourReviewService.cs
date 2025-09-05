// UseCases/TourReviewService.cs
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using ToursService.Database;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;
using ToursService.Dtos;

namespace ToursService.UseCases
{
    public class TourReviewService : ITourReviewService
    {
        private readonly ITourReviewRepository _repo;
        private readonly ToursContext _db;
        private readonly IMapper _mapper;

        public TourReviewService(ITourReviewRepository repo, ToursContext db, IMapper mapper)
        {
            _repo = repo;
            _db = db;
            _mapper = mapper;
        }

        public Result<TourReviewDto> Create(TourReviewCreateDto dto, long? requesterIdFromClaims = null)
        {
            if (dto is null) return Result.Fail<TourReviewDto>("Review DTO is null.");
            if (dto.IdTour <= 0) return Result.Fail<TourReviewDto>("IdTour must be positive.");
            if (dto.IdTourist <= 0) return Result.Fail<TourReviewDto>("IdTourist must be positive.");
            if (dto.Rating < 1 || dto.Rating > 5) return Result.Fail<TourReviewDto>("Rating must be between 1 and 5.");
            if (dto.DateTour is null) return Result.Fail<TourReviewDto>("DateTour is required.");

            // (opciono) enforce da autor recenzije == authenticated korisnik (ako šalješ claim)
            if (requesterIdFromClaims.HasValue && requesterIdFromClaims.Value != dto.IdTourist)
                return Result.Fail<TourReviewDto>("You can only post a review for yourself.");

            // (opciono) provera da tura postoji:
            var tourExists = _db.Tours.AsNoTracking().Any(t => t.Id == dto.IdTour);
            if (!tourExists) return Result.Fail<TourReviewDto>("Tour not found.");

            try
            {
                var entity = _mapper.Map<TourReview>(dto);
                entity.DateComment = DateTime.UtcNow; // server-side timestamp

                var created = _repo.Create(entity);
                var createdDto = _mapper.Map<TourReviewDto>(created);
                return Result.Ok(createdDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<TourReviewDto>($"EXCEPTION: {ex.Message}");
            }
        }

        public Result<List<TourReviewDto>> GetByTour(long tourId)
        {
            var list = _repo.GetByTour(tourId);
            var dtos = list.Select(_mapper.Map<TourReviewDto>).ToList();
            return Result.Ok(dtos);
        }

        public Result<List<TourReviewDto>> GetByTourist(long touristId)
        {
            var list = _repo.GetByTourist(touristId);
            var dtos = list.Select(_mapper.Map<TourReviewDto>).ToList();
            return Result.Ok(dtos);
        }

        public Result<PagedResult<TourReviewDto>> GetPaged(int page, int pageSize)
        {
            var pr = _repo.GetPaged(page, pageSize);
            var mapped = new PagedResult<TourReviewDto>(
                pr.Results.Select(_mapper.Map<TourReviewDto>).ToList(),
                pr.TotalCount
            );
            return Result.Ok(mapped);
        }

        public Result Delete(int id, long requesterId)
        {
            // (opciono) dozvola: admin ili autor
            var r = _repo.GetById(id);
            if (r is null) return Result.Fail("Review not found.");

            // Ako imaš role/claims, ovde proveri admin ulogu; za sada: autor može da briše
            if (r.IdTourist != requesterId)
                return Result.Fail("Forbidden.");

            try
            {
                _repo.Delete(id);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"EXCEPTION: {ex.Message}");
            }
        }

        public Result<TourReviewSummaryDto> SummaryByTour(long tourId)
        {
            var list = _repo.GetByTour(tourId);
            if (list.Count == 0)
                return Result.Ok(new TourReviewSummaryDto { Count = 0, AverageRating = 0 });

            var count = list.LongCount();
            var avg = list.Average(x => x.Rating);
            return Result.Ok(new TourReviewSummaryDto { Count = count, AverageRating = Math.Round(avg, 2) });
        }
    }
}
