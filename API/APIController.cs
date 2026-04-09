using Microsoft.AspNetCore.Mvc;
using UsedAndReliableCars.Models;

namespace UsedAndReliableCars.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly UsedCar usedCars;

        public APIController( UsedCar usedCars )
        {
            this.usedCars = usedCars;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCars()
        {
            var usedCarList = usedCars.UsedCars;
            return Ok(usedCarList);
        }
    }
}
