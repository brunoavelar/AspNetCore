using System.Collections.Generic;
using System.Linq;
using System.Net;
using CityInfo.Api;
using CityInfo.Api.Controllers;
using CityInfo.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using CityInfo.Api.Services;
using Moq;
using System.Threading.Tasks;
using CityInfo.Api.Entities;

namespace CityInfo.Test.Controllers
{
    [TestFixture]
    public class CitiesControllerTest
    {
        private Mock<IRepository> _mockRepository;

        private CitiesController _controller;

        [TearDown]
        public void Cleanup()
        {
            if (_controller != null)
            {
                _controller.Dispose();
            }
        }

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IRepository>();

            _controller = new CitiesController(_mockRepository.Object);

            CitiesDataStore.Current.Cities.Clear();
            CitiesDataStore.Current.Cities.Add(new CityDto { Id = 1, Name = "City 1", Description = "Description 1" });
            CitiesDataStore.Current.Cities.Add(new CityDto { Id = 2, Name = "City 2" });
        }

        [Test]
        public async Task GetCities_ShouldReturn_200()
        {
            var result = await _controller.GetCities();
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public async Task GetCities_ShouldReturn_AllCities()
        {
            IEnumerable<City> expectedList = new List<City>()
            {
                new City { Id = 1, Name = "City 1" },
                new City { Id = 2, Name = "City 2" }
            };
            _mockRepository.Setup(x => x.GetCitiesAsync()).Returns(Task.FromResult(expectedList));

            var result = await _controller.GetCities();
            var okResult = (OkObjectResult) result;
            var data = (IEnumerable<CityWithoutPointOfInterestDto>) okResult.Value;

            data.Should().HaveCount(2);
        }

        [Test]
        public void GetCity_ShouldReturn_200()
        {
            var result = _controller.GetCity(1);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public void GetCity_ShouldReturn_404_When_NoCitiesWereFound()
        {
            var result = _controller.GetCity(3);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int) HttpStatusCode.NotFound);
        }

        [Test]
        public void GetCity_ShouldReturn_TheCity()
        {
            var result = _controller.GetCity(1);
            var okResult = (OkObjectResult)result;
            var data = (CityDto)okResult.Value;

            var cityFromDb = CitiesDataStore.Current.Cities.Single(x => x.Id == 1);

            data.Id.Should().Be(cityFromDb.Id);
            data.Name.Should().Be(cityFromDb.Name);
            data.Description.Should().Be(cityFromDb.Description);
        }
    }
}
