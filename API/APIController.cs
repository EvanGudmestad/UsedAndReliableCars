using Microsoft.AspNetCore.Mvc;
using UsedAndReliableCars.Models;

namespace UsedAndReliableCars.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllCars()
        {
            var usedCarList = new UsedCar().UsedCars;
            return Ok(usedCarList);
        }
    }
}