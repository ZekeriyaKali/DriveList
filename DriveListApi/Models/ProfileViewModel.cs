namespace DriveListApi.Models
{
    public class ProfileViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int TotalPredictions { get; set; }
        public int TotalDiagnoses { get; set; }
        public DateTime? LastLoginTime { get; set; }

    }
}
