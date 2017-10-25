using CityInfo.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.Api.Controllers
{
    public class DummyController : Controller
    {
        private CityInfoContext _context;

        public DummyController(CityInfoContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("api/testdatabase")]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
