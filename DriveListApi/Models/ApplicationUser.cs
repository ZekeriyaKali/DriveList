using Microsoft.AspNetCore.Identity;

namespace DriveListApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime? LastLoginTime { get; set; } // Kullanıcının son giriş zamanı
    }
}
