
namespace DriveList.Shared.Models
{
    public class PredictionHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // Identity UserId

        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Km { get; set; }
        public string GearType { get; set; }
        public string FuelType { get; set; }
        public string City { get; set; }
        public decimal PredictedPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
