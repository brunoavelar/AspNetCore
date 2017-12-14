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
using System.Linq;

namespace CityInfo.Test
{
    [TestFixture]
    public class RepositoryTest : BaseTest
    {
        [Test]
        public async Task GetPointsOfInterestForCity_ShouldReturn_PointsOfInterestOfTheCity()
        {
            var city = Context.Cities.First();
            var expectedPois = city.PointsOfInterest;

            var repo = new Repository(Context);

            var pois = await repo.GetPointsOfInterestForCity(city.Id);
            pois.ShouldBeEquivalentTo(expectedPois);
        }

        [Test]
        public async Task GetPointsOfInterestForCity_ShouldNotReturn_PointsOfInterestOfOtherCities()
        {
            var city = Context.Cities.First();
            var notExpectedPois = Context.PointsOfInterest.Where(x => x.CityId != city.Id);

            var repo = new Repository(Context);

            var pois = await repo.GetPointsOfInterestForCity(city.Id);
            pois.Should().NotContain(notExpectedPois);
        }

        [Test]
        public async Task GetCities_ShouldReturn_AllCities()
        {
            var repo = new Repository(Context);

            var cities = await repo.GetCitiesAsync();

            var expectedCities = Context.Cities;

            cities.Should().Contain(expectedCities);
        }

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

        [Test]
        public async Task CityExists_ShouldReturn_True_IfCityExists()
        {
            var expectedCity = await Context.Cities.FirstOrDefaultAsync();

            var repo = new Repository(Context);
            var exists = await repo.CityExists(expectedCity.Id);

            exists.Should().BeTrue();
        }

        [Test]
        public async Task CityExists_ShouldReturn_False_IfCityDoesntExists()
        {
            var repo = new Repository(Context);
            var exists = await repo.CityExists(999);

            exists.Should().BeFalse();
        }
    }
}
