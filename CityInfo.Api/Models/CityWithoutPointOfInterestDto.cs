using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.Api.Entities;

namespace CityInfo.Api.Models
{
    public class CityWithoutPointOfInterestDto
    {
        public CityWithoutPointOfInterestDto()
        {

        }

        public CityWithoutPointOfInterestDto(City city)
        {
            Id = city.Id;
            Name = city.Name;
            Description = city.Description;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
