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
            var existingCity = new City { Id = 1, Name = "City 1" };
            _mockRepository.Setup(x => x.GetCity(It.IsAny<int>(), It.IsAny<bool>())).Returns(existingCity);

            var result = _controller.GetCity(1);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public void GetCity_ShouldReturn_404_When_NoCitiesWereFound()
        {
            var existingCity = new City { Id = 1, Name = "City 1" };
            _mockRepository.Setup(x => x.GetCity(1, It.IsAny<bool>())).Returns(existingCity);

            var result = _controller.GetCity(3);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int) HttpStatusCode.NotFound);
        }

        [Test]
        public void GetCity_ShouldReturn_TheCity()
        {
            var expectedCity = new City { Id = 1, Name = "City 1", Description = "Description 1" };
            expectedCity.PointsOfInterest.Add(new PointOfInterest() { Id = 1, Name = "Point of Interest 1", Description = "Description of Point of Interest 1", CityId = 1 });
            _mockRepository.Setup(x => x.GetCity(1, It.IsAny<bool>())).Returns(expectedCity);

            var result = _controller.GetCity(1);
            var okResult = (OkObjectResult)result;
            var data = (CityWithoutPointOfInterestDto)okResult.Value;

            data.Id.Should().Be(expectedCity.Id);
            data.Name.Should().Be(expectedCity.Name);
            data.Description.Should().Be(expectedCity.Description);
        }

        [Test]
        public void GetCityIncludePointOfInterest_ShouldReturn_TheCityWithPointOfInterest()
        {
            var expectedCity = new City { Id = 1, Name = "City 1", Description = "Description 1" };
            expectedCity.PointsOfInterest.Add(new PointOfInterest() { Id = 1, Name = "Point of Interest 1", Description = "Description of Point of Interest 1", CityId = 1 });
            _mockRepository.Setup(x => x.GetCity(1, It.IsAny<bool>())).Returns(expectedCity);

            var result = _controller.GetCity(1, true);
            var okResult = (OkObjectResult)result;
            var data = (CityDto)okResult.Value;

            data.Id.Should().Be(expectedCity.Id);
            data.Name.Should().Be(expectedCity.Name);
            data.Description.Should().Be(expectedCity.Description);
            data.NumberOfPointsOfInterest.Should().Be(1);
            data.PointsOfInterest.Should().HaveCount(1);

            var poi = data.PointsOfInterest.Single();
            poi.Id.Should().Be(expectedCity.PointsOfInterest.Single().Id);
            poi.Name.Should().Be(expectedCity.PointsOfInterest.Single().Name);
            poi.Description.Should().Be(expectedCity.PointsOfInterest.Single().Description);
        }
    }
}
