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
    public class PointsOfInterestControllerTest
    {
        [SetUp]
        public void Setup()
        {
            var city1 = new CityDto()
            {
                Id = 1,
                Name = "City 1",
                Description = "Description of city 1",
                PointsOfInterest = new List<PointOfInterestDto>
                {
                    new PointOfInterestDto
                    {
                        Id = 1,
                        Name = "PoI 1",
                        Description = "Description of PoI 1"
                    },
                    new PointOfInterestDto
                    {
                        Id = 2,
                        Name = "PoI 2",
                        Description = "Description of PoI 2"
                    }
                }
            };

            CitiesDataStore.Current.Cities.Clear();
            CitiesDataStore.Current.Cities.Add(city1);
        }

        [Test]
        public void GetPointsOfInterest_ShouldReturn_200()
        {
            var controller = new PointsOfInterestController();
            var result = controller.GetPointsOfInterest(1);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public void GetPointsOfInterest_ShouldReturn_AllPointsOfInterestsForTheCity()
        {
            var controller = new PointsOfInterestController();
            var result = controller.GetPointsOfInterest(1);
            var okResult = (OkObjectResult) result;
            var data = (IEnumerable<PointOfInterestDto>) okResult.Value;

            data.Should().HaveCount(2);
        }

        [Test]
        public void GetPointsOfInterest_ShouldReturn_404_When_NoCitiesWereFound()
        {
            var controller = new PointsOfInterestController();
            var result = controller.GetPointsOfInterest(2);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public void GetPointOfInterest_ShouldReturn_200()
        {
            var controller = new PointsOfInterestController();
            var result = controller.GetPointOfInterest(1, 1);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public void GetPointOfInterest_ShouldReturn_ThePointOfInterestWithSpecifiedIdFromTheSpecifiedTheCity()
        {
            var controller = new PointsOfInterestController();
            var result = controller.GetPointOfInterest(1, 1);
            var okResult = (OkObjectResult)result;
            var data = (PointOfInterestDto)okResult.Value;

            var poiFromDb = CitiesDataStore.Current.Cities
                .Single(x => x.Id == 1)
                .PointsOfInterest
                .Single(x => x.Id == 1);

            data.Id.Should().Be(poiFromDb.Id);
            data.Name.Should().Be(poiFromDb.Name);
            data.Description.Should().Be(poiFromDb.Description);
        }

        [Test]
        public void GetPointOfInterest_ShouldReturn_NotFound_When_TheCityIsNotFound()
        {
            var controller = new PointsOfInterestController();
            var result = controller.GetPointOfInterest(2, 1);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public void GetPointOfInterest_ShouldReturn_404_When_ThePoiIsNotFound()
        {
            var controller = new PointsOfInterestController();
            var result = controller.GetPointOfInterest(1, 3);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public void CreatePointOfInterest_ShouldReturn_400_When_InvalidDataIsSent()
        {
            var controller = new PointsOfInterestController();
            var result = controller.CreatePointOfInterest(1, null);
            result.Should().BeOfType<BadRequestResult>();
            var badRequestResult = (BadRequestResult)result;
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Test]
        public void CreatePointOfInterest_ShouldReturn_404_When_TheIdOfCityIsInvalid()
        {
            var controller = new PointsOfInterestController();
            var result = controller.CreatePointOfInterest(3, new PointOfInterestForCreationDto());
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public void CreatePointOfInterest_ShouldReturn_201()
        {
            var controller = new PointsOfInterestController();
            var result = controller.CreatePointOfInterest(1, new PointOfInterestForCreationDto());
            result.Should().BeOfType<CreatedResult>();
            var createdResult = (CreatedResult)result;
            createdResult.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult.Location.Should().BeEmpty(); //Fix at home
        }

        [Test]
        public void CreatePointOfInterest_Should_CalculateNewId()
        {
            var currentMaxId = CitiesDataStore.Current.Cities.SelectMany(x => x.PointsOfInterest).Max(x => x.Id);

            var controller = new PointsOfInterestController();
            controller.CreatePointOfInterest(1, new PointOfInterestForCreationDto());

            var newPoi = CitiesDataStore.Current.Cities
                .Single(x => x.Id == 1)
                .PointsOfInterest
                .SingleOrDefault(x => x.Id == currentMaxId + 1);

            newPoi.Should().NotBeNull();
        }
    }
}
