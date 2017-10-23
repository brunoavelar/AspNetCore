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

namespace CityInfo.Test.Controllers
{
    [TestFixture]
    public class PointsOfInterestControllerTest
    {
        private Mock<ILoggerService<PointsOfInterestController>> _mockLogger;
        private Mock<IMailService> _mockMailService;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILoggerService<PointsOfInterestController>>();
            _mockMailService = new Mock<IMailService>();

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

        #region Get Points of Interest

        [Test]
        public void GetPointsOfInterest_ShouldReturn_200()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = controller.GetPointsOfInterest(1);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public void GetPointsOfInterest_ShouldReturn_AllPointsOfInterestsForTheCity()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = controller.GetPointsOfInterest(1);
            var okResult = (OkObjectResult)result;
            var data = (IEnumerable<PointOfInterestDto>)okResult.Value;

            data.Should().HaveCount(2);
        }

        [Test]
        public void GetPointsOfInterest_ShouldReturn_404_When_TheCityIsNotFound()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = controller.GetPointsOfInterest(2);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public void GetPointsOfInterest_ShouldLog_Message_When_TheCityIsNotFound()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            controller.GetPointsOfInterest(2);
            _mockLogger.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once());
            var expectedLog = string.Format(LogMessages.PointOfInterestNotFoundMsg, 2);
        }

        #endregion

        #region Get Point of Interest

        [Test]
        public void GetPointOfInterest_ShouldReturn_200()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = controller.GetPointOfInterest(1, 1);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Test]
        public void GetPointOfInterest_ShouldReturn_ThePointOfInterestWithSpecifiedIdFromTheSpecifiedTheCity()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
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
        public void GetPointOfInterest_ShouldLog_Message_When_TheCityIsNotFound()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = controller.GetPointOfInterest(2, 1);
            var expectedLog = string.Format(LogMessages.CityNotFoundMsg, 2);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }

        [Test]
        public void GetPointOfInterest_ShouldReturn_404_When_TheCityIsNotFound()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = controller.GetPointOfInterest(2, 1);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public void GetPointOfInterest_ShouldLogMessage_When_ThePoiIsNotFound()
        {

            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = controller.GetPointOfInterest(1, 3);
            var expectedLog = string.Format(LogMessages.PointOfInterestNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }

        [Test]
        public void GetPointOfInterest_ShouldReturn_404_When_ThePoiIsNotFound()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = controller.GetPointOfInterest(1, 3);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        #endregion

        #region Create Point of Interest

        [Test]
        public async Task CreatePointOfInterest_ShouldReturn_400_When_InvalidDataIsSent()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.CreatePointOfInterest(1, null);
            result.Should().BeOfType<BadRequestResult>();
            var badRequestResult = (BadRequestResult)result;
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreatePointOfInterest_ShouldReturn_404_When_TheIdOfCityIsInvalid()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.CreatePointOfInterest(3, new PointOfInterestForCreationDto());
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public async Task CreatePointOfInterest_ShouldLogMessage_When_TheCityIsNotFound()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.CreatePointOfInterest(3, new PointOfInterestForCreationDto());
            var expectedLog = string.Format(LogMessages.CityNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }

        [Test]
        public async Task CreatePointOfInterest_ShouldReturn_201()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.CreatePointOfInterest(1, new PointOfInterestForCreationDto());
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

            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            controller.CreatePointOfInterest(1, new PointOfInterestForCreationDto());

            var newPoi = CitiesDataStore.Current.Cities
                .Single(x => x.Id == 1)
                .PointsOfInterest
                .SingleOrDefault(x => x.Id == currentMaxId + 1);

            newPoi.Should().NotBeNull();
        }

        [Test]
        public async Task CreatePointOfInterest_Integrate_ShouldValidateEntity()
        {
            var currentMaxId = CitiesDataStore.Current.Cities.SelectMany(x => x.PointsOfInterest).Max(x => x.Id);
            var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            var client = server.CreateClient();

            var model = new PointOfInterestForCreationDto
            {
                Name = string.Empty
            };

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var result = await client.PostAsync("/api/cities/1/pointOfInterest", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 51);
            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await client.PostAsync("/api/cities/1/pointOfInterest", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 50);
            model.Description = new string('a', 201);
            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await client.PostAsync("/api/cities/1/pointOfInterest", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 50);
            model.Description = new string('a', 200);

            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await client.PostAsync("/api/cities/1/pointOfInterest", content);
            result.StatusCode.Should().Be(HttpStatusCode.Created);
            result.IsSuccessStatusCode.Should().BeTrue();
        }

        #endregion

        #region Total Update (PUT) of Point of Interest

        [Test]
        public async Task UpdatePointOfInterest_ShouldReturn_400_When_InvalidDataIsSent()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.UpdatePointOfInterest(1, 1, null);
            result.Should().BeOfType<BadRequestResult>();
            var badRequestResult = (BadRequestResult)result;
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdatePointOfInterest_ShouldReturn_404_When_IdsAreInvalid()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.UpdatePointOfInterest(1, 3, new PointOfInterestForUpdateDto());
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            result = await controller.UpdatePointOfInterest(3, 1, new PointOfInterestForUpdateDto());
            result.Should().BeOfType<NotFoundResult>();
            notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public async Task UpdatePointOfInterest_ShouldLogMessage_When_IdsAreInvalid()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.UpdatePointOfInterest(3, 1, new PointOfInterestForUpdateDto());
            var expectedLog = string.Format(LogMessages.CityNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());

            _mockLogger.ResetCalls();

            result = await controller.UpdatePointOfInterest(1, 3, new PointOfInterestForUpdateDto());
            result.Should().BeOfType<NotFoundResult>();
            expectedLog = string.Format(LogMessages.PointOfInterestNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }


        [Test]
        public async Task UpdatePointOfInterest_ShouldReturn_204()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.UpdatePointOfInterest(1, 1, new PointOfInterestForUpdateDto());
            result.Should().BeOfType<NoContentResult>();
            var noContentResult = (NoContentResult)result;
            noContentResult.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        [Test]
        public async Task UpdatePointOfInterest_Integrate_ShouldValidateEntity()
        {
            var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            var client = server.CreateClient();

            var model = new PointOfInterestForCreationDto
            {
                Name = string.Empty
            };

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var result = await client.PutAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 51);
            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await client.PutAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 50);
            model.Description = new string('a', 201);
            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await client.PutAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 50);
            model.Description = new string('a', 200);

            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await client.PutAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            result.IsSuccessStatusCode.Should().BeTrue();

            var poiFromDb = CitiesDataStore.Current.Cities.Single(x => x.Id == 1).PointsOfInterest.Single(x => x.Id == 1);
            poiFromDb.Name.Should().Be(model.Name);
            poiFromDb.Description.Should().Be(model.Description);
        }

        #endregion

        #region Total Update (PUT) of Point of Interest

        [Test]
        public async Task PartiallyUpdatePointOfInterest_ShouldReturn_400_When_InvalidDataIsSent()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.PartiallyUpdatePointOfInterest(1, 1, null);
            result.Should().BeOfType<BadRequestResult>();
            var badRequestResult = (BadRequestResult)result;
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task PartiallyUpdatePointOfInterest_ShouldReturn_404_When_IdsAreInvalid()
        {
            var patchDoc = new JsonPatchDocument<PointOfInterestForUpdateDto>();
            patchDoc.Replace(x => x.Name, "Updated - PoI 1");

            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.PartiallyUpdatePointOfInterest(1, 3, patchDoc);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            result = await controller.PartiallyUpdatePointOfInterest(3, 1, patchDoc);
            result.Should().BeOfType<NotFoundResult>();
            notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public async Task PartiallyUpdatePointOfInterest_ShouldLogMessage_When_IdsAreInvalid()
        {
            var patchDoc = new JsonPatchDocument<PointOfInterestForUpdateDto>();
            patchDoc.Replace(x => x.Name, "Updated - PoI 1");

            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.PartiallyUpdatePointOfInterest(3, 1, patchDoc);
            var expectedLog = string.Format(LogMessages.CityNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());

            _mockLogger.ResetCalls();

            result = await controller.PartiallyUpdatePointOfInterest(1, 3, patchDoc);
            expectedLog = string.Format(LogMessages.PointOfInterestNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }

        [Test]
        public async Task PartiallyUpdatePointOfInterest_Integrate_ShouldValidateEntity()
        {
            var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            HttpClient client = server.CreateClient();

            var patchDoc = new JsonPatchDocument<PointOfInterestForUpdateDto>();
            patchDoc.Replace(x => x.Name, string.Empty); // Invalid
            patchDoc.Remove(x => x.Description); // Invalid

            var content = new StringContent(JsonConvert.SerializeObject(patchDoc), Encoding.UTF8, "application/json");
            var result = await client.PatchAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            patchDoc.Replace(x => x.Name, new string('a', 51)); // Invalid - Too many characters
            patchDoc.Replace(x => x.Description, new string('a', 51)); // Valid - Should validate just Vame
            content = new StringContent(JsonConvert.SerializeObject(patchDoc), Encoding.UTF8, "application/json");
            result = await client.PatchAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            patchDoc.Replace(x => x.Name, new string('a', 50)); // Valid - Should validate Description
            patchDoc.Replace(x => x.Description, new string('a', 201)); // Invalid - Too many characters
            content = new StringContent(JsonConvert.SerializeObject(patchDoc), Encoding.UTF8, "application/json");
            result = await client.PatchAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var validName = new string('a', 50);
            var validDescription = new string('a', 200);

            patchDoc.Replace(x => x.Name, validName); // Valid
            patchDoc.Replace(x => x.Description, validDescription); // Valid
            content = new StringContent(JsonConvert.SerializeObject(patchDoc), Encoding.UTF8, "application/json");
            result = await client.PatchAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            result.IsSuccessStatusCode.Should().BeTrue();

            var poiFromDb = CitiesDataStore.Current.Cities.Single(x => x.Id == 1).PointsOfInterest.Single(x => x.Id == 1);
            poiFromDb.Name.Should().Be(validName);
            poiFromDb.Description.Should().Be(validDescription);
        }

        #endregion

        #region Delete Point of Interest

        [Test]
        public async Task DeletePointOfInterest_ShouldReturn_204()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.DeletePointOfInterest(1, 1);
            result.Should().BeOfType<NoContentResult>();
            var okResult = (NoContentResult)result;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        [Test]
        public async Task DeletePointOfInterest_ShouldReturn_404_When_IdsAreInvalid()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.DeletePointOfInterest(2, 1);
            result.Should().BeOfType<NotFoundResult>();
            var notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            result = await controller.DeletePointOfInterest(1, 3);
            result.Should().BeOfType<NotFoundResult>();
            notFoundResult = (NotFoundResult)result;
            notFoundResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeletePointOfInterest_ShouldLogMessage_When_IdsAreInvalid()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.DeletePointOfInterest(2, 1);
            var expectedLog = string.Format(LogMessages.CityNotFoundMsg, 2);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());

            _mockLogger.ResetCalls();

            result = await controller.DeletePointOfInterest(1, 3);
            expectedLog = string.Format(LogMessages.PointOfInterestNotFoundMsg, 3);
            _mockLogger.Verify(x => x.LogInformation(expectedLog), Times.Once());
        }

        [Test]
        public async Task DeletePointOfInterest_ShouldMailAdmin_When_Deleted()
        {
            var controller = new PointsOfInterestController(_mockLogger.Object, _mockMailService.Object);
            var result = await controller.DeletePointOfInterest(1, 1);
            _mockMailService.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>()));
        }

        #endregion
    }
}
