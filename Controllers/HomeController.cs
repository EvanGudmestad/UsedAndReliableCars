using Microsoft.AspNetCore.Mvc;
using UsedAndReliableCars.Models;

namespace UsedAndReliableCars.Controllers
{
    public class HomeController : Controller
    {
        public List<UsedCar> usedCars = new List<UsedCar>
        {
            new UsedCar
            {
                Type = "Sedan",
                PriceCategory = "Under $10,000",
                Year = "2014-2021",
                Make = "Mazda",
                Model = "Mazda6"
            },
            new UsedCar
            {
                Type = "Sedan",
                PriceCategory = "Under $15,000",
                Year = "2014-2019",
                Make = "Toyota",
                Model = "Corolla"
            },
            new UsedCar
            {
                Type = "SUV",
                PriceCategory = "Under $15,000",
                Year = "2018-2024",
                Make = "Chevrolet",
                Model = "Equinox"
            },
            new UsedCar
            {
                Type = "Hybrid Sedan",
                PriceCategory = "Under $20,000",
                Year = "2020 - present",
                Make = "Toyota",
                Model = "Corolla Hybrid"
            },
            new UsedCar
            {
                Type = "SUV",
                PriceCategory = "Under $20,000",
                Year = "2018-2023 ",
                Make = "Subaru",
                Model = "Crosstrek"
            },
            new UsedCar
            {
                Type = "SUV",
                PriceCategory = "Under $20,000",
                Year = "2016-2018",
                Make = "Toyota",
                Model = "RAV4 Hybrid"
            },
            new UsedCar
            {
                Type = "SUV",
                PriceCategory = "Under $20,000",
                Year = "2014-2019",
                Make = "Toyota",
                Model = "Highlander"
            },
            new UsedCar
            {
                Type = "SUV",
                PriceCategory = "Under $20,000",
                Year = "2015-2021",
                Make = "Lexus",
                Model = "NX"
            },
            new UsedCar
            {
                Type = "Truck",
                PriceCategory = "Under $25,000",
                Year = "2017-present",
                Make = "Honda",
                Model = "Ridgeline"
            },
            new UsedCar
            {
                Type = "Sports Car",
                PriceCategory = "Under $20,000",
                Year = "2016-2024",
                Make = "Mazda",
                Model = "MX-5 Miata"
            }
        };
        public IActionResult Index()
        {
            var model = new UsedCar
            {
                UsedCars = usedCars // your list
            };

            return View(model);
        }
    }
}
