using System.Collections.Generic;
using System.Linq;
using System.Net;
using CityInfo.Api;
using CityInfo.Api.Controllers;
using CityInfo.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;


namespace CityInfo.Test.Controllers
{
    [TestFixture]
    public class CitiesControllerTest
    {
        [SetUp]
        public void Setup()
        {
            CitiesDataStore.Current.Cities.Clear();
            CitiesDataStore.Current.Cities.Add(new CityDto { Id = 1, Name = "City 1", Description = "Description 1" });
            CitiesDataStore.Current.Cities.Add(new CityDto { Id = 2, Name = "City 2" });
        }

        [Test]
        public void GetCities_ShouldReturn_200()
        {
            var controller = new CitiesController();
            var result = controller.GetCities();
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public void GetCities_ShouldReturn_AllCities()
        {
            var controller = new CitiesController();
            var result = controller.GetCities();
            var okResult = (OkObjectResult) result;
            var data = (IEnumerable<CityDto>) okResult.Value;

            data.Should().HaveCount(2);
        }

        [Test]
        public void GetCity_ShouldReturn_200()
        {
            var controller = new CitiesController();
            var result = controller.GetCity(1);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public void GetCity_ShouldReturn_404_When_NoCitiesWereFound()
        {
            var controller = new CitiesController();
            var result = controller.GetCity(3);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int) HttpStatusCode.NotFound);
        }

        [Test]
        public void GetCity_ShouldReturn_TheCity()
        {
            var controller = new CitiesController();
            var result = controller.GetCity(1);
            var okResult = (OkObjectResult)result;
            var data = (CityDto)okResult.Value;

            var cityFromDb = CitiesDataStore.Current.Cities.Single(x => x.Id == 1);

            data.Id.Should().Be(cityFromDb.Id);
            data.Name.Should().Be(cityFromDb.Name);
            data.Description.Should().Be(cityFromDb.Description);
        }
    }
}
