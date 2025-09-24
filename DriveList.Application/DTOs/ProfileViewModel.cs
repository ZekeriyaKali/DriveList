using DriveList.Domain.Entities;

namespace DriveList.Application.DTOs
{
    public class ProfileViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int TotalPredictions { get; set; }
        public int TotalDiagnoses { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public List<LoginAudit> RecentLoginAttempts { get; set; } = new();
        public int Credits { get; set; }

    }
}
