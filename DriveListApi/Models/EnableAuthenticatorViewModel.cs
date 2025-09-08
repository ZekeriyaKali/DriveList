namespace DriveListApi.Models
{
    public class EnableAuthenticatorViewModel
    {
        // Shared secret key (base32 format, kullanıcıya gösterilecek)
        public string SharedKey { get; set; } = string.Empty;

        // QR Code URI (otpauth:// URI formatında)
        public string AuthenticatorUri { get; set; } = string.Empty;

        // QR Code base64 resmi (View’de <img src="..." /> için)
        public string QrCodeImageBase64 { get; set; } = string.Empty;

        // Kullanıcının Authenticator uygulamasına girip oluşturduğu kod
        public string Code { get; set; } = string.Empty;
    }
}
