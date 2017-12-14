using CityInfo.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.Api.Services
{
    public interface IRepository
    {
        Task<IEnumerable<City>> GetCitiesAsync();
        Task<bool> CityExists(int id);
        City GetCity(int cityId, bool includePointsOfInterest);
        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCity(int cityId);
        PointOfInterest GetPointOfInterest(int cityId, int pointOfInterestId);
    }

    public class Repository : IRepository
    {
        private CityInfoContext _context;

        public Repository(CityInfoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await _context.Cities.OrderBy(x => x.Name).ToListAsync();
        }

        public City GetCity(int cityId, bool includePointsOfInterest)
        {
            var city = _context.Cities.SingleOrDefault(x => x.Id == cityId);

            if (includePointsOfInterest)
            {
                _context.Entry(city)
                    .Collection(x => x.PointsOfInterest)
                    .Load();
            }

            return city;
        }

        public PointOfInterest GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCity(int cityId)
        {
            return await _context
                .PointsOfInterest
                .Where(x => x.CityId == cityId)
                .ToListAsync();
        }

        public async Task<bool> CityExists(int id)
        {
            return await _context.Cities.AnyAsync(x => x.Id == id);
        }
    }
}
