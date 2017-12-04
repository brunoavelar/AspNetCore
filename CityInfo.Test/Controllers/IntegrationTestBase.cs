using CityInfo.Api;
using CityInfo.Api.Entities;
using CityInfo.Api.Models;
using CityInfo.Api.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;

namespace CityInfo.Test.Controllers
{
    public abstract class IntegrationTestBase
    {
        protected HttpClient Client { get; private set; }
        protected TestServer Server { get; private set; }

        [SetUp]
        public void Setup()
        {
            Server = CreateFakeServer();
            Client = Server.CreateClient();

            var cityDto = new CityDto()
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
            CitiesDataStore.Current.Cities.Add(cityDto);
        }

        protected static TestServer CreateFakeServer()
        {
            var builder = WebHost.CreateDefaultBuilder().UseStartup<Startup>();

            return new TestServer(builder);
        }

        protected static string SerializeObject(object value)
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var jsonFromDatabase = JsonConvert.SerializeObject(value, settings);
            return jsonFromDatabase;
        }
    }
}