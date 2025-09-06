namespace DriveListApi.Models
{
    public class LoginAudit
    {
        public int Id { get; set; }
        public string? UserId { get; set; }           // Identity user id (nullable for unknown user)
        public string? Username { get; set; }         // attempted username/email
        public bool Success { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? FailureReason { get; set; }    // optional detail
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
