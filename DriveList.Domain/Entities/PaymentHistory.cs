namespace DriveList.Domain.Entities
{
    public class PaymentHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CreditsAdded { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
