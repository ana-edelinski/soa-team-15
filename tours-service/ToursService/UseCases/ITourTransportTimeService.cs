using FluentResults;
using System.Collections.Generic;
using ToursService.Domain;
using ToursService.Dtos;

namespace ToursService.UseCases
{
    public interface ITourTransportTimeService
    {
        // READ
        Result<List<TourTransportTimeDto>> GetAll(long tourId);
        Result<TourTransportTimeDto> GetOne(long tourId, TransportType type);

        // WRITE
        Result Create(long tourId, long authorId, TransportType type, int minutes);
        Result Update(long tourId, long authorId, TransportType type, int minutes);
        Result Delete(long tourId, long authorId, TransportType type);
    }
}
