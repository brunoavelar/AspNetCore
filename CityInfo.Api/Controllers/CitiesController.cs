﻿using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CityInfo.Api.Services;
using System.Threading.Tasks;
using CityInfo.Api.Models;

namespace CityInfo.Api.Controllers
{
    [Route("api/cities")]
    public class CitiesController : Controller
    {
        private IRepository _repository;

        public CitiesController(IRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _repository.GetCitiesAsync();

            var citiesDto = cities.Select(x => new CityWithoutPointOfInterestDto(x));

            return Ok(citiesDto);
        }

        [HttpGet("{id}")]
        public IActionResult GetCity(int id, bool includePointOfInterest = false)
        {
            var city = _repository.GetCity(id, includePointOfInterest);

            if (city == null)
            {
                return NotFound();
            }

            var cityToReturn = includePointOfInterest ? new CityDto(city) : new CityWithoutPointOfInterestDto(city);

            return Ok(cityToReturn);
        }
    }
}
