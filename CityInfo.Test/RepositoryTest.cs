using CityInfo.Api.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CityInfo.Api.Services;
using NUnit.Framework;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using System.IO;

namespace CityInfo.Test
{
    [TestFixture]
    public class RepositoryTest : BaseTest
    {
        [Test]
        public async Task GetCities_ShouldReturn_OrderedCities()
        {
            var repo = new Repository(Context);

            var cities = await repo.GetCitiesAsync();

            cities.Should().BeInAscendingOrder(x => x.Name);
        }

        [Test]
        public async Task GetCity_ShouldReturn_City()
        {
            var expectedCity = await Context.Cities.SingleAsync(x => x.Id == 1);

            var repo = new Repository(Context);
            var city = repo.GetCity(expectedCity.Id, false);
            city.Should().Be(expectedCity);
            city.PointsOfInterest.Should().BeEmpty();

            city = repo.GetCity(expectedCity.Id, true);
            city.Should().Be(expectedCity);
            city.PointsOfInterest.Should().NotBeEmpty();
        }
    }
}
