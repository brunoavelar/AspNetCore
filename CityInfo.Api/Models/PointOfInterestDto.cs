using CityInfo.Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.Api.Models
{
    public class PointOfInterestDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public PointOfInterestDto()
        {

        }

        public PointOfInterestDto(PointOfInterest poi)
        {
            Id = poi.Id;
            Name = poi.Name;
            Description = poi.Description;
        }
    }
}
