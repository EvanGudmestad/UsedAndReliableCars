using System;
using System.ComponentModel.DataAnnotations;
using UsedAndReliableCars.Enums;

namespace UsedAndReliableCars.Models
{
    public class Car
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public CarMake Make { get; set; }  // Enum

        [Required]
        public string Model { get; set; }  // Populated dynamically

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        public string Generation { get; set; }
    }
}
