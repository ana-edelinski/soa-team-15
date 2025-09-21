using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToursService.Database;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;

namespace ToursService.Repositories
{
    public class TourTransportTimeRepository : ITourTransportTimeRepository
    {
        private readonly ToursContext _db;

        public TourTransportTimeRepository(ToursContext db)
        {
            _db = db;
        }

        // CREATE
        public TourTransportTime Create(TourTransportTime entity)
        {
            _db.Set<TourTransportTime>().Add(entity);
            _db.SaveChanges();
            return entity;
        }

        // READ
        public TourTransportTime? GetById(long id)
        {
            return _db.Set<TourTransportTime>()
                      .AsNoTracking()
                      .FirstOrDefault(x => x.Id == id);
        }

        public List<TourTransportTime> GetByTour(long tourId)
        {
            return _db.Set<TourTransportTime>()
                      .AsNoTracking()
                      .Where(x => x.TourId == tourId)
                      .OrderBy(x => x.Type)
                      .ToList();
        }

        public TourTransportTime? GetByTourAndType(long tourId, TransportType type)
        {
            return _db.Set<TourTransportTime>()
                      .FirstOrDefault(x => x.TourId == tourId && x.Type == type);
        }

        public bool Exists(long tourId, TransportType type)
        {
            return _db.Set<TourTransportTime>()
                      .Any(x => x.TourId == tourId && x.Type == type);
        }

        // UPDATE
        public void Update(TourTransportTime entity)
        {
            _db.Set<TourTransportTime>().Update(entity);
            _db.SaveChanges();
        }

        // DELETE
        public void Remove(TourTransportTime entity)
        {
            _db.Set<TourTransportTime>().Remove(entity);
            _db.SaveChanges();
        }

        public void DeleteById(long id)
        {
            var entity = _db.Set<TourTransportTime>().FirstOrDefault(x => x.Id == id);
            if (entity != null)
            {
                _db.Set<TourTransportTime>().Remove(entity);
                _db.SaveChanges();
            }
        }
    }
}
