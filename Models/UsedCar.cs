using System.Collections.Generic;

namespace UsedAndReliableCars.Models
{
    public class UsedCar
    {
        public int Id { get; set; } = -1;
        public string Type { get; set; } = "UsedCar";
        public int PriceCategory { get; set; } = 0; // 0 = Unknown, 1 = Budget, 2 = Mid-range, 3 = Premium
        public string Year { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;

        public List<UsedCar> UsedCars { get; set; } = new List<UsedCar>();
    }
}