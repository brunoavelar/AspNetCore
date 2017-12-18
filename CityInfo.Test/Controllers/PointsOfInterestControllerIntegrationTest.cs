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

namespace CityInfo.Test.Controllers
{
    [TestFixture]
    public class PointsOfInterestControllerIntegrationTest : IntegrationTestBase
    {
        [Test]
        public async Task GetPointsOfInterest_Integrate_ShouldReturnEntities()
        {
            var repository = (Repository)Server.Host.Services.GetService(typeof(IRepository));
            var pois = await repository.GetPointsOfInterestForCity(1);
            var poisDto = pois.Select(x => new PointOfInterestDto()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            });
            var jsonFromDatabase = SerializeObject(poisDto);

            var result = await Client.GetAsync("/api/cities/1/pointsOfInterest");
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await result.Content.ReadAsStringAsync();
            content.Should().Be(jsonFromDatabase);
        }

        [Test]
        public async Task GetPointOfInterest_Integrate_ShouldReturnEntity()
        {
            var repository = (Repository)Server.Host.Services.GetService(typeof(IRepository));
            var poi = (await repository.GetPointsOfInterestForCity(1)).First();
            var poiDto = new PointOfInterestDto()
            {
                Id = poi.Id,
                Name = poi.Name,
                Description = poi.Description
            };
            var jsonFromDatabase = SerializeObject(poiDto);

            var result = await Client.GetAsync("/api/cities/1/pointOfInterest/1");
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await result.Content.ReadAsStringAsync();
            content.Should().Be(jsonFromDatabase);
        }

        [Test]
        public async Task CreatePointOfInterest_Integrate_ShouldValidateEntity()
        {
            var model = new PointOfInterestForCreationDto
            {
                Name = string.Empty
            };

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var result = await Client.PostAsync("/api/cities/1/pointOfInterest", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 51);
            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await Client.PostAsync("/api/cities/1/pointOfInterest", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 50);
            model.Description = new string('a', 201);
            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await Client.PostAsync("/api/cities/1/pointOfInterest", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 50);
            model.Description = new string('a', 200);

            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await Client.PostAsync("/api/cities/1/pointOfInterest", content);
            result.StatusCode.Should().Be(HttpStatusCode.Created);
            result.IsSuccessStatusCode.Should().BeTrue();
        }

        [Test]
        public async Task UpdatePointOfInterest_Integrate_ShouldValidateEntity()
        {
            var model = new PointOfInterestForCreationDto
            {
                Name = string.Empty
            };

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var result = await Client.PutAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 51);
            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await Client.PutAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 50);
            model.Description = new string('a', 201);
            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await Client.PutAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            model.Name = new string('a', 50);
            model.Description = new string('a', 200);

            content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            result = await Client.PutAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            result.IsSuccessStatusCode.Should().BeTrue();

            var poiFromDb = CitiesDataStore.Current.Cities.Single(x => x.Id == 1).PointsOfInterest.Single(x => x.Id == 1);
            poiFromDb.Name.Should().Be(model.Name);
            poiFromDb.Description.Should().Be(model.Description);
        }

        [Test]
        public async Task PartiallyUpdatePointOfInterest_Integrate_ShouldValidateEntity()
        {
            var patchDoc = new JsonPatchDocument<PointOfInterestForUpdateDto>();
            patchDoc.Replace(x => x.Name, string.Empty); // Invalid
            patchDoc.Remove(x => x.Description); // Invalid

            var content = new StringContent(JsonConvert.SerializeObject(patchDoc), Encoding.UTF8, "application/json");
            var result = await Client.PatchAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            patchDoc.Replace(x => x.Name, new string('a', 51)); // Invalid - Too many characters
            patchDoc.Replace(x => x.Description, new string('a', 51)); // Valid - Should validate just Vame
            content = new StringContent(JsonConvert.SerializeObject(patchDoc), Encoding.UTF8, "application/json");
            result = await Client.PatchAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            patchDoc.Replace(x => x.Name, new string('a', 50)); // Valid - Should validate Description
            patchDoc.Replace(x => x.Description, new string('a', 201)); // Invalid - Too many characters
            content = new StringContent(JsonConvert.SerializeObject(patchDoc), Encoding.UTF8, "application/json");
            result = await Client.PatchAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var validName = new string('a', 50);
            var validDescription = new string('a', 200);

            patchDoc.Replace(x => x.Name, validName); // Valid
            patchDoc.Replace(x => x.Description, validDescription); // Valid
            content = new StringContent(JsonConvert.SerializeObject(patchDoc), Encoding.UTF8, "application/json");
            result = await Client.PatchAsync("/api/cities/1/pointOfInterest/1", content);
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            result.IsSuccessStatusCode.Should().BeTrue();

            var poiFromDb = CitiesDataStore.Current.Cities.Single(x => x.Id == 1).PointsOfInterest.Single(x => x.Id == 1);
            poiFromDb.Name.Should().Be(validName);
            poiFromDb.Description.Should().Be(validDescription);
        }
    }
}
