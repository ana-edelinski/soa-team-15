using System.Collections.Generic;
using ToursService.Domain;

namespace ToursService.Domain.RepositoryInterfaces
{
    public interface ITourTransportTimeRepository
    {
        // CREATE
        TourTransportTime Create(TourTransportTime entity);

        // READ
        TourTransportTime? GetById(long id);
        List<TourTransportTime> GetByTour(long tourId);
        TourTransportTime? GetByTourAndType(long tourId, TransportType type);
        bool Exists(long tourId, TransportType type);

        // UPDATE
        void Update(TourTransportTime entity);

        // DELETE
        void Remove(TourTransportTime entity);
        void DeleteById(long id);
    }
}
