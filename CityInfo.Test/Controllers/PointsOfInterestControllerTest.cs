using System.Collections.Generic;
using System.Linq;
using System.Net;
using CityInfo.Api;
using CityInfo.Api.Controllers;
using CityInfo.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.JsonPatch;
using CityInfo.Test.Extentions;
using Moq;
using CityInfo.Api.Services;
using Microsoft.AspNetCore;
using CityInfo.Api.Entities;

namespace CityInfo.Test.Controllers
{
    [TestFixture]
    public class PointsOfInterestControllerTest
    {
        private Mock<ILoggerService<PointsOfInterestController>> _mockLogger;
        private Mock<IMailService> _mockMailService;
        private Mock<IRepository> _mockRepository;

        private PointsOfInterestController _controller;

        City CityWithTwoPointsOfInterest = new City()
        {
            Id = 1,
            Name = "City 1",
            Description = "Description of city 1",
            PointsOfInterest = new List<PointOfInterest>
                {
                    new PointOfInterest
                    {
                        Id = 1,
                        Name = "PoI 1",
                        Description = "Description of PoI 1"
                    },
                    new PointOfInterest
                    {
                        Id = 2,
                        Name = "PoI 2",
                        Description = "Description of PoI 2"
                    }
                }
        };

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILoggerService<PointsOfInterestController>>();
            _mockMailService = new Mock<IMailService>();
            _mockRepository = new Mock<IRepository>();

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

            _controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object, _mockRepository.Object);
        }

        [TearDown]
        public void Cleanup()
        {
            if(_controller != null)
            {
                _controller.Dispose();
            }
        }

        #region Get Points of Interest

        [Test]
        public async Task GetPointsOfInterest_ShouldReturn_200()
        {
            _mockRepository.Setup(x => x.CityExists(CityWithTwoPointsOfInterest.Id)).Returns(Task.FromResult(true));

            var result = await _controller.GetPointsOfInterest(CityWithTwoPointsOfInterest.Id);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public async Task GetPointsOfInterest_ShouldReturn_AllPointsOfInterestsForTheCity()
        {
            _mockRepository.Setup(x => x.CityExists(CityWithTwoPointsOfInterest.Id)).Returns(Task.FromResult(true));
            _mockRepository.Setup(x => x.GetPointsOfInterestForCity(CityWithTwoPointsOfInterest.Id)).Returns(Task.FromResult(CityWithTwoPointsOfInterest.PointsOfInterest.AsEnumerable()));

            var result = await _controller.GetPointsOfInterest(CityWithTwoPointsOfInterest.Id);
            var okResult = (OkObjectResult)result;
            var data = (IEnumerable<PointOfInterestDto>)okResult.Value;

            data.Should().HaveCount(2);
        }

        [Test]
        public async Task GetPointsOfInterest_ShouldReturn_404_When_TheCityIsNotFound()
        {
            var result = await _controller.GetPointsOfInterest(2);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetPointsOfInterest_ShouldLog_Message_When_TheCityIsNotFound()
        {
            await _controller.GetPointsOfInterest(2);
            _mockLogger.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once());
            var expectedLog = string.Format(LogMessages.PointOfInterestNotFoundMsg, 2);
        }

        #endregion

        #region Get Point of Interest

        [Test]
        public void GetPointOfInterest_ShouldReturn_200()
        {
            _mockRepository.Setup(x => x.GetCity(1, true)).Returns(CityWithTwoPointsOfInterest);

            var result = _controller.GetPointOfInterest(1, 1);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public void GetPointOfInterest_ShouldReturn_ThePointOfInterestWithSpecifiedIdFromTheSpecifiedTheCity()
        {
            _mockRepository.Setup(x => x.GetCity(1, true)).Returns(CityWithTwoPointsOfInterest);

            var result = _controller.GetPointOfInterest(1, 1);
            var okResult = (OkObjectResult)result;
            var data = (PointOfInterestDto)okResult.Value;

            var poiFromDb = CityWithTwoPointsOfInterest
                .PointsOfInterest
                .Single(x => x.Id == 1);

            data.Id.Should().Be(poiFromDb.Id);
            data.Name.Should().Be(poiFromDb.Name);
            data.Description.Should().Be(poiFromDb.Description);
        }

        [Test]
        public void GetPointOfInterest_ShouldLog_Message_When_TheCityIsNotFound()
        {
            var result = _controller.GetPointOfInterest(2, 1);
            var expectedLog = string.Format(LogMessages.CityNotFoundMsg, 2);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }

        [Test]
        public void GetPointOfInterest_ShouldReturn_404_When_TheCityIsNotFound()
        {
            var result = _controller.GetPointOfInterest(2, 1);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public void GetPointOfInterest_ShouldLogMessage_When_ThePoiIsNotFound()
        {
            _mockRepository.Setup(x => x.GetCity(1, true)).Returns(CityWithTwoPointsOfInterest);

            var result = _controller.GetPointOfInterest(1, 3);
            var expectedLog = string.Format(LogMessages.PointOfInterestNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }

        [Test]
        public void GetPointOfInterest_ShouldReturn_404_When_ThePoiIsNotFound()
        {
            var result = _controller.GetPointOfInterest(1, 3);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        #endregion

        #region Create Point of Interest

        [Test]
        public async Task CreatePointOfInterest_ShouldReturn_400_When_InvalidDataIsSent()
        {
            var result = await _controller.CreatePointOfInterest(1, null);
            result.Should().BeOfType<BadRequestResult>();
            var badRequestResult = (BadRequestResult)result;
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreatePointOfInterest_ShouldReturn_404_When_TheIdOfCityIsInvalid()
        {
            var result = await _controller.CreatePointOfInterest(3, new PointOfInterestForCreationDto());
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public async Task CreatePointOfInterest_ShouldLogMessage_When_TheCityIsNotFound()
        {
            var result = await _controller.CreatePointOfInterest(3, new PointOfInterestForCreationDto());
            var expectedLog = string.Format(LogMessages.CityNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }

        [Test]
        public async Task CreatePointOfInterest_ShouldReturn_201()
        {
            var result = await _controller.CreatePointOfInterest(1, new PointOfInterestForCreationDto());
            result.Should().BeOfType<CreatedAtRouteResult>();
            var createdResult = (CreatedAtRouteResult)result;
            createdResult.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult.RouteName.Should().Be(PointsOfInterestController.GetPointOfInterestRouteName);
            createdResult.RouteValues["cityId"].Should().Be(1);
            createdResult.RouteValues["id"].Should().Be(3);
        }

        [Test]
        public void CreatePointOfInterest_Should_CalculateNewId()
        {
            var currentMaxId = CitiesDataStore.Current.Cities.SelectMany(x => x.PointsOfInterest).Max(x => x.Id);

            _controller.CreatePointOfInterest(1, new PointOfInterestForCreationDto());

            var newPoi = CitiesDataStore.Current.Cities
                .Single(x => x.Id == 1)
                .PointsOfInterest
                .SingleOrDefault(x => x.Id == currentMaxId + 1);

            newPoi.Should().NotBeNull();
        }

        #endregion

        #region Total Update (PUT) of Point of Interest

        [Test]
        public async Task UpdatePointOfInterest_ShouldReturn_400_When_InvalidDataIsSent()
        {
            var result = await _controller.UpdatePointOfInterest(1, 1, null);
            result.Should().BeOfType<BadRequestResult>();
            var badRequestResult = (BadRequestResult)result;
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdatePointOfInterest_ShouldReturn_404_When_IdsAreInvalid()
        {
            var result = await _controller.UpdatePointOfInterest(1, 3, new PointOfInterestForUpdateDto());
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            result = await _controller.UpdatePointOfInterest(3, 1, new PointOfInterestForUpdateDto());
            result.Should().BeOfType<NotFoundResult>();
            notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public async Task UpdatePointOfInterest_ShouldLogMessage_When_IdsAreInvalid()
        {
            var result = await _controller.UpdatePointOfInterest(3, 1, new PointOfInterestForUpdateDto());
            var expectedLog = string.Format(LogMessages.CityNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());

            _mockLogger.ResetCalls();

            result = await _controller.UpdatePointOfInterest(1, 3, new PointOfInterestForUpdateDto());
            result.Should().BeOfType<NotFoundResult>();
            expectedLog = string.Format(LogMessages.PointOfInterestNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }


        [Test]
        public async Task UpdatePointOfInterest_ShouldReturn_204()
        {
            var result = await _controller.UpdatePointOfInterest(1, 1, new PointOfInterestForUpdateDto());
            result.Should().BeOfType<NoContentResult>();
            var noContentResult = (NoContentResult)result;
            noContentResult.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        #endregion

        #region Total Update (PUT) of Point of Interest

        [Test]
        public async Task PartiallyUpdatePointOfInterest_ShouldReturn_400_When_InvalidDataIsSent()
        {
            var result = await _controller.PartiallyUpdatePointOfInterest(1, 1, null);
            result.Should().BeOfType<BadRequestResult>();
            var badRequestResult = (BadRequestResult)result;
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task PartiallyUpdatePointOfInterest_ShouldReturn_404_When_IdsAreInvalid()
        {
            var patchDoc = new JsonPatchDocument<PointOfInterestForUpdateDto>();
            patchDoc.Replace(x => x.Name, "Updated - PoI 1");

            var result = await _controller.PartiallyUpdatePointOfInterest(1, 3, patchDoc);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            result = await _controller.PartiallyUpdatePointOfInterest(3, 1, patchDoc);
            result.Should().BeOfType<NotFoundResult>();
            notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public async Task PartiallyUpdatePointOfInterest_ShouldLogMessage_When_IdsAreInvalid()
        {
            var patchDoc = new JsonPatchDocument<PointOfInterestForUpdateDto>();
            patchDoc.Replace(x => x.Name, "Updated - PoI 1");

            var result = await _controller.PartiallyUpdatePointOfInterest(3, 1, patchDoc);
            var expectedLog = string.Format(LogMessages.CityNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());

            _mockLogger.ResetCalls();

            result = await _controller.PartiallyUpdatePointOfInterest(1, 3, patchDoc);
            expectedLog = string.Format(LogMessages.PointOfInterestNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }
        
        #endregion

        #region Delete Point of Interest

        [Test]
        public async Task DeletePointOfInterest_ShouldReturn_204()
        {
            var result = await _controller.DeletePointOfInterest(1, 1);
            result.Should().BeOfType<NoContentResult>();
            var okResult = (NoContentResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        [Test]
        public async Task DeletePointOfInterest_ShouldReturn_404_When_IdsAreInvalid()
        {
            var result = await _controller.DeletePointOfInterest(2, 1);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            result = await _controller.DeletePointOfInterest(1, 3);
            result.Should().BeOfType<NotFoundResult>();
            notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeletePointOfInterest_ShouldLogMessage_When_IdsAreInvalid()
        {
            var result = await _controller.DeletePointOfInterest(2, 1);
            var expectedLog = string.Format(LogMessages.CityNotFoundMsg, 2);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());

            _mockLogger.ResetCalls();

            result = await _controller.DeletePointOfInterest(1, 3);
            expectedLog = string.Format(LogMessages.PointOfInterestNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }

        [Test]
        public async Task DeletePointOfInterest_ShouldMailAdmin_When_Deleted()
        {
            var result = await _controller.DeletePointOfInterest(1, 1);
            _mockMailService.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>()));
        }

        #endregion
    }
}
