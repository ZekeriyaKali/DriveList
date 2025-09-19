using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveList.Shared.Models
{
    public class CarRequest
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Km { get; set; }
        public string GearType { get; set; }
        public string FuelType { get; set; }
        public string City { get; set; }
    }
}
