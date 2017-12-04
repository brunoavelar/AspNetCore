using CityInfo.Api.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace CityInfo.Test
{
    public class BaseTest
    {
        private CityInfoContext _context;

        protected CityInfoContext Context
        {
            get
            {
                if (_context == null)
                {
                    _context = BuildDiskContext();
                }

                return _context;
            }
        }

        [SetUp]
        public void Setup()
        {
            var cities = new List<City>()
            {
                new City()
                {
                    Id = 1,
                    Name = "New York City",
                    Description = "The one with that big park.",
                    PointsOfInterest = new List<PointOfInterest>()
                    {
                        new PointOfInterest()
                        {
                            Id = 1,
                            Name = "Central Park",
                            Description = "The most visited urban park in the United States."
                        },
                        new PointOfInterest()
                        {
                            Id = 2,
                            Name = "Empire State Building",
                            Description = "A 102-story skyscraper located in Midtown Manhattan."
                        },
                    }
                },
                new City()
                {
                    Id = 2,
                    Name = "Antwerp",
                    Description = "The one with the cathedral that was never really finished.",
                    PointsOfInterest = new List<PointOfInterest>()
                    {
                        new PointOfInterest()
                        {
                            Id = 3,
                            Name = "Cathedral of Our Lady",
                            Description = "A Gothic style cathedral, conceived by architects Jan and Pieter Appelmans."
                        },
                        new PointOfInterest()
                        {
                            Id = 4,
                            Name = "Antwerp Central Station",
                            Description = "The the finest example of railway architecture in Belgium."
                        },
                    }
                },
                new City()
                {
                    Id = 3,
                    Name = "Paris",
                    Description = "The one with that big tower.",
                    PointsOfInterest = new List<PointOfInterest>()
                    {
                    new PointOfInterest()
                    {
                        Id = 5,
                        Name = "Eiffel Tower",
                        Description = "A wrought iron lattice tower on the Champ de Mars, named after engineer Gustave Eiffel." },
                    new PointOfInterest()
                    {
                        Id = 6,
                        Name = "The Louvre",
                        Description = "The world's largest museum." },
                    }
                }
            };

            using (var context = BuildDiskContext())
            {
                context.Cities.AddRange(cities);
                context.SaveChanges();
            }
        }

        private CityInfoContext BuildDiskContext()
        {
            var connection = new SqliteConnection("Data Source=testDB.db;");

            var options = new DbContextOptionsBuilder<CityInfoContext>()
                .UseSqlite(connection)
                .Options;
            var context = new CityInfoContext(options);

            return context;
        }

        [TearDown]
        public void Cleanup()
        {
            if(_context != null)
            {
                _context.Dispose();
                _context = null;
            }

            File.Delete("testDB.db");
        }
    }
}