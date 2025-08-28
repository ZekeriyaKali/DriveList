using System.ComponentModel.DataAnnotations;

namespace DriveListApi.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
        public string Email { get; set; }
    }
}
