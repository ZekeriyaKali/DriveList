using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;

namespace DriveList.Domain.Entities { 

public class ApplicationUser : IdentityUser
{
    public DateTime? LastLoginTime { get; set; } // Kullanıcının son giriş zamanı
    public int Credits { get; set; } = 10; // Başlangıç kredisi
}
}