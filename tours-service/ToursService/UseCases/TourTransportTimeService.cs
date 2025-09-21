using System.Linq;
using FluentResults;
using System.Collections.Generic;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;
using ToursService.Dtos;

namespace ToursService.UseCases
{
    public class TourTransportTimeService : ITourTransportTimeService
    {
        private readonly ITourRepository _tourRepository;
        private readonly ITourTransportTimeRepository _ttRepository;

        public TourTransportTimeService(
            ITourRepository tourRepository,
            ITourTransportTimeRepository ttRepository)
        {
            _tourRepository = tourRepository;
            _ttRepository = ttRepository;
        }

        // ---------- READ ----------

public Result<List<TourTransportTimeDto>> GetAll(long tourId)
{
    var tour = _tourRepository.GetById(tourId);
    if (tour is null) return Result.Fail("Tour not found.");

    var list = tour.TransportTimes
        .OrderBy(t => t.Type)
        .Select(Map)           // sada poziva Func<T, TResult>
        .ToList();

    return Result.Ok(list);
}

public Result<TourTransportTimeDto> GetOne(long tourId, TransportType type)
{
    var tour = _tourRepository.GetById(tourId);
    if (tour is null) return Result.Fail<TourTransportTimeDto>("Tour not found.");

    var tt = tour.TransportTimes.FirstOrDefault(x => x.Type == type);
    if (tt is null) return Result.Fail<TourTransportTimeDto>("Transport time not found.");

    return Result.Ok(Map(tt)); // jednostavni overload
}

// ---------- Helpers ----------

private static TourTransportTimeDto Map(TourTransportTime t)
    => new TourTransportTimeDto
    {
        Id = t.Id,
        TourId = t.TourId,
        Type = t.Type,
        Minutes = t.Minutes
    };

        // ---------- WRITE ----------

        public Result Create(long tourId, long authorId, TransportType type, int minutes)
        {
            try
            {
                var tour = _tourRepository.GetById(tourId);
                if (tour is null) return Result.Fail("Tour not found.");

                // Jedan zapis po tipu – ako postoji → 409 (iz perspektive kontrolera)
                if (tour.TransportTimes.Any(t => t.Type == type))
                    return Result.Fail("Transport time for this type already exists. Use update.");

                // Domen brine o validaciji autora i minuta
                tour.AddOrUpdateTransportTime(authorId, type, minutes);

                _tourRepository.Update(tour);
                return Result.Ok();
            }
            catch (System.Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result Update(long tourId, long authorId, TransportType type, int minutes)
        {
            try
            {
                var tour = _tourRepository.GetById(tourId);
                if (tour is null) return Result.Fail("Tour not found.");

                if (!tour.TransportTimes.Any(t => t.Type == type))
                    return Result.Fail("Transport time for this type not found.");

                minutes = calcucateTime(tour.LengthInKm,type);    

                tour.AddOrUpdateTransportTime(authorId, type, minutes);

                _tourRepository.Update(tour);
                return Result.Ok();
            }
            catch (System.Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result Delete(long tourId, long authorId, TransportType type)
        {
            try
            {
                var tour = _tourRepository.GetById(tourId);
                if (tour is null) return Result.Fail("Tour not found.");

                var existing = tour.TransportTimes.FirstOrDefault(t => t.Type == type);
                if (existing is null) return Result.Fail("Transport time for this type not found.");

                // Domen provjerava autora
                var removed = tour.RemoveTransportTime(authorId, type);
                if (!removed) return Result.Fail("Transport time for this type not found."); // safety

                // Eksplicitno obriši zavisni entitet (sigurno za EF)
                _ttRepository.Remove(existing);

                // (opciono) update agregata
                _tourRepository.Update(tour);

                return Result.Ok();
            }
            catch (System.Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public int calcucateTime(double distanceKm, TransportType type){

        double speed = type switch
        {
            TransportType.Walk => 5.0,
            TransportType.Bike => 16.0,
            TransportType.Car  => 50.0,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

         if (distanceKm <= 0 || speed <= 0) return 0;

        double minutes = distanceKm / speed * 60.0;
        return (int)Math.Round(minutes);

        }
    }
}
