namespace DriveListApi.Models
{
    public class Prediction
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Km { get; set; }
        public string GearType { get; set; }
        public string FuelType { get; set; }
        public string City { get; set; }
        public double PredictedPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
