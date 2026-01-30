namespace UsedAndReliableCars.Models
{
    public class UsedCar
    {
        public string Type { get; set; }
        public string PriceCategory { get; set; }
        public string Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public List<UsedCar> UsedCars { get; set; }

        
    }
}
