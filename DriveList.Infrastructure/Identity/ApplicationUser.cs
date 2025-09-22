using Microsoft.AspNetCore.Identity;

namespace DriveList.Infrastructure.Identity { 

public class ApplicationUser : IdentityUser
{
    public DateTime? LastLoginTime { get; set; } // Kullanıcının son giriş zamanı
    public int Credits { get; set; } = 10; // Başlangıç kredisi
}
}