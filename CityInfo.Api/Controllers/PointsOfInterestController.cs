using System.Linq;
using CityInfo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using CityInfo.Api.Utils;

namespace CityInfo.Api.Controllers
{
    [Route("api/cities")]
    public class PointsOfInterestController : Controller
    {
        public const string GetPointOfInterestRouteName = "GetPointOfInterest";

        private ILoggerAdapter<PointsOfInterestController> _logger;

        public PointsOfInterestController(ILoggerAdapter<PointsOfInterestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{cityId}/pointsOfInterest")]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            var city = GetCity(cityId);
            if (city == null)
            {
                return NotFound();
            }

            return Ok(city.PointsOfInterest);
        }

        [HttpGet("{cityId}/pointOfInterest/{id}", Name = GetPointOfInterestRouteName)]
        public IActionResult GetPointOfInterest(int cityId, int id)
        {
            var city = GetCity(cityId);
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterest = GetPointOfInterest(city, id);
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

            var city = GetCity(cityId);
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

            var city = GetCity(cityId);
            if (city == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            var pointOfInterestFromDb = GetPointOfInterest(city, id);
            if (pointOfInterestFromDb == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            pointOfInterestFromDb.Name = pointOfInterest.Name;
            pointOfInterestFromDb.Description = pointOfInterest.Description;

            return Task.FromResult<IActionResult>(NoContent());
        }

        [HttpPatch("{cityId}/pointOfInterest/{id}")]
        public Task<IActionResult> PartiallyUpdatePointOfInterest(int cityId, int id, [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return Task.FromResult<IActionResult>(BadRequest());
            }

            var city = GetCity(cityId);
            if (city == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            var pointOfInterestFromDb = GetPointOfInterest(city, id);
            if (pointOfInterestFromDb == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            var pointOfInterestToPatch = new PointOfInterestForUpdateDto
            {
                Name = pointOfInterestFromDb.Name,
                Description = pointOfInterestFromDb.Description
            };

            patchDoc.ApplyTo(pointOfInterestToPatch);

            TryValidateModel(pointOfInterestToPatch);

            if (!ModelState.IsValid)
            {
                return Task.FromResult<IActionResult>(BadRequest(ModelState));
            }

            pointOfInterestFromDb.Name = pointOfInterestToPatch.Name;
            pointOfInterestFromDb.Description = pointOfInterestToPatch.Description;

            return Task.FromResult<IActionResult>(NoContent());
        }

        [HttpDelete("{cityId}/pointOfInterest/{id}")]
        public Task<IActionResult> DeletePointOfInterest(int cityId, int id)
        {
            var city = GetCity(cityId);
            if (city == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            var pointOfInterestFromDb = GetPointOfInterest(city, id);
            if (pointOfInterestFromDb == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            city.PointsOfInterest.Remove(pointOfInterestFromDb);

            return Task.FromResult<IActionResult>(NoContent());
        }

        private CityDto GetCity(int cityId)
        {
            var city = CitiesDataStore.Current.Cities.SingleOrDefault(x => x.Id == cityId);

            if (city == null)
            {
                var cityNotFoundMsg = string.Format(LogMessages.CityNotFoundMsg, cityId);
                _logger.LogInformation(cityNotFoundMsg);
            }

            return city;
        }

        private PointOfInterestDto GetPointOfInterest(CityDto city, int pointOfInterestId)
        {
            var pointOfInterest = city.PointsOfInterest.SingleOrDefault(x => x.Id == pointOfInterestId);

            if (pointOfInterest == null)
            {
                var pointOfInterestNotFoundMsg = string.Format(LogMessages.PointOfInterestNotFoundMsg, pointOfInterestId);
                _logger.LogInformation(pointOfInterestNotFoundMsg);
            }

            return pointOfInterest;
        }
    }
}
