using CityInfo.Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.Api.Models
{
    public class CityDto : CityWithoutPointOfInterestDto
    {
        public List<PointOfInterestDto> PointsOfInterest { get; set; } = new List<PointOfInterestDto>();

        public int NumberOfPointsOfInterest => PointsOfInterest.Count;

        public CityDto()
        {

        }

        public CityDto(City city) 
            : base(city)
        {
            foreach (var item in city.PointsOfInterest)
            {
                PointsOfInterest.Add(new PointOfInterestDto(item));
            }
        }
    }
}
