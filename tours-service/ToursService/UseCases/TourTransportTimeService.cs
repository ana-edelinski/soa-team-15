using System.Linq;
using FluentResults;
using System.Collections.Generic;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;
using ToursService.Dtos;
using Microsoft.Extensions.Logging;

namespace ToursService.UseCases
{
    // using ... (isti using-i)

public class TourTransportTimeService : ITourTransportTimeService
{
    private readonly ITourRepository _tourRepository;
    private readonly ITourTransportTimeRepository _ttRepository;
    private readonly ILogger<TourTransportTimeService> _log;


    public TourTransportTimeService(
        ITourRepository tourRepository,
        ITourTransportTimeRepository ttRepository,
        ILogger<TourTransportTimeService> log
    )
    {
        _tourRepository = tourRepository;
        _ttRepository = ttRepository;
        _log = log;
    }

    // ---------- READ (ostavi kako želiš ili koristi repo direktno) ----------

    public Result<List<TourTransportTimeDto>> GetAll(long tourId)
    {
        var tour = _tourRepository.GetById(tourId);
        if (tour is null) return Result.Fail("Tour not found.");

        var list = _ttRepository.GetByTour(tourId)
            .OrderBy(t => t.Type)
            .Select(Map)
            .ToList();

        return Result.Ok(list);
    }

    public Result<TourTransportTimeDto> GetOne(long tourId, TransportType type)
    {
        var tour = _tourRepository.GetById(tourId);
        if (tour is null) return Result.Fail<TourTransportTimeDto>("Tour not found.");

        var tt = _ttRepository.GetByTourAndType(tourId, type);
        if (tt is null) return Result.Fail<TourTransportTimeDto>("Transport time not found.");

        return Result.Ok(Map(tt));
    }

    private static TourTransportTimeDto Map(TourTransportTime t)
        => new TourTransportTimeDto
        {
            Id = t.Id,
            TourId = t.TourId,
            Type = t.Type,
            Minutes = t.Minutes
        };

    // ---------- WRITE (ključne male izmjene) ----------

    public Result Create(long tourId, long authorId, TransportType type, int minutes)
    {
      _log.LogInformation("Creating transport time {@Payload}", new { tourId, type, minutes });

        try
        {
            var tour = _tourRepository.GetById(tourId);
            if (tour is null) return Result.Fail("Tour not found.");

            // NE oslanjamo se na tour.TransportTimes; koristimo repo
            if (_ttRepository.Exists(tourId, type))
                return Result.Fail("Transport time for this type already exists. Use update.");

            if (minutes <= 0)
                return Result.Fail("Minutes must be > 0.");

            // KLJUČNO: koristi novi ctor sa tourId da FK bude postavljen
            var entity = new TourTransportTime(tourId, type, minutes);
            _ttRepository.Create(entity);

            return Result.Ok();
        }
        catch (Exception ex)
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

            var existing = _ttRepository.GetByTourAndType(tourId, type);
            if (existing is null) return Result.Fail("Transport time for this type not found.");

            // Ako želiš da server SAM računa iz km:
            minutes = calcucateTime(tour.LengthInKm, type);

            if (minutes <= 0)
                return Result.Fail("Minutes must be > 0.");

            existing.Update(minutes);
            _ttRepository.Update(existing);

            return Result.Ok();
        }
        catch (Exception ex)
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

            var existing = _ttRepository.GetByTourAndType(tourId, type);
            if (existing is null) return Result.Fail("Transport time for this type not found.");

            _ttRepository.Remove(existing);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public int calcucateTime(double distanceKm, TransportType type)
    {
        double speed = type switch
        {
            TransportType.Walk => 5.0,
            TransportType.Bike => 16.0,
            TransportType.Car  => 50.0,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
        if (distanceKm <= 0 || speed <= 0) return 0;
        return (int)Math.Round(distanceKm / speed * 60.0);
    }
}
}