using System.Linq;
using CityInfo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CityInfo.Api.Controllers
{
    [Route("api/cities")]
    public class PointsOfInterestController : Controller
    {
        public const string GetPointOfInterestRouteName = "GetPointOfInterest";

        [HttpGet("{cityId}/pointsOfInterest")]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            return Ok(city.PointsOfInterest);
        }

        [HttpGet("{cityId}/pointOfInterest/{id}", Name = GetPointOfInterestRouteName)]
        public IActionResult GetPointOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterest = city.PointsOfInterest.FirstOrDefault(x => x.Id == id);
            if (pointOfInterest == null)
            {
                return NotFound();
            }

            return Ok(pointOfInterest);
        }

        [HttpPost("{cityId}/pointOfInterest")]
        public Task<IActionResult> CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            if (pointOfInterest == null)
            {
                return Task.FromResult<IActionResult>(BadRequest());
            }

            if (!ModelState.IsValid)
            {
                return Task.FromResult<IActionResult>(BadRequest(ModelState));
            }

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            // For demo. Will change when adding EF support
            var maxId = CitiesDataStore.Current.Cities
                .SelectMany(x => x.PointsOfInterest)
                .Max(x => x.Id);

            var pointOfInterestToSave = new PointOfInterestDto
            {
                Id = ++maxId,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };
            city.PointsOfInterest.Add(pointOfInterestToSave);

            var createdAtRoute = CreatedAtRoute(GetPointOfInterestRouteName, new
            {
                cityId = cityId,
                id = pointOfInterestToSave.Id
            }, pointOfInterestToSave);

            return Task.FromResult<IActionResult>(createdAtRoute);
        }

        [HttpPut("{cityId}/pointOfInterest/{id}")]
        public Task<IActionResult> UpdatePointOfInterest(int cityId, int id, [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
            if (pointOfInterest == null)
            {
                return Task.FromResult<IActionResult>(BadRequest());
            }

            if (!ModelState.IsValid)
            {
                return Task.FromResult<IActionResult>(BadRequest(ModelState));
            }

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            var pointOfInterestFromDb = city.PointsOfInterest.FirstOrDefault(x => x.Id == id);
            if (pointOfInterestFromDb == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            pointOfInterestFromDb.Name = pointOfInterest.Name;
            pointOfInterestFromDb.Description = pointOfInterest.Description;

            return Task.FromResult<IActionResult>(NoContent());
        }
    }
}
