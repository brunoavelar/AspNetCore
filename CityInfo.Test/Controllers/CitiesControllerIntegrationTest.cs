using System.Collections.Generic;
using System.Linq;
using System.Net;
using CityInfo.Api;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using CityInfo.Api.Models;
using CityInfo.Api.Entities;
using CityInfo.Api.Services;

namespace CityInfo.Test.Controllers
{
    [TestFixture]
    public class CitiesControllerIntegrationTest : IntegrationTestBase
    {
        [Test]
        public async Task GetCities_ShoulReturn_AllCities()
        {
            var repository = (Repository)Server.Host.Services.GetService(typeof(IRepository));
            var cities = await repository.GetCitiesAsync();
            var citiesDto = cities.Select(x => new CityWithoutPointOfInterestDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            });
            var jsonFromDatabase = SerializeObject(citiesDto);

            var result = await Client.GetAsync("/api/cities");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.IsSuccessStatusCode.Should().BeTrue();

            var content = await result.Content.ReadAsStringAsync();
            content.Should().Be(jsonFromDatabase);
        }

        [Test]
        public async Task GetCity_ShoulReturn_SpecifiedCity()
        {
            var repository = (Repository)Server.Host.Services.GetService(typeof(IRepository));
            var city = repository.GetCity(1, true);
            var cityDto = new CityDto
            {
                Id = city.Id,
                Name = city.Name,
                Description = city.Description
            };

            var poitsOfInterest = city.PointsOfInterest.Select(x => new PointOfInterestDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            });

            cityDto.PointsOfInterest.AddRange(poitsOfInterest);

            var jsonFromDatabase = SerializeObject(cityDto);

            var result = await Client.GetAsync("/api/cities/1?includePointOfInterest=true");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.IsSuccessStatusCode.Should().BeTrue();

            var content = await result.Content.ReadAsStringAsync();
            content.Should().Be(jsonFromDatabase);
        }
    }
}
