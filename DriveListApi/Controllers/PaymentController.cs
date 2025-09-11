using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DriveListApi.Data;
using DriveListApi.Models;

namespace DriveListApi.Controllers
{
    public class PaymentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public PaymentController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Payment/BuyCredits
        public IActionResult BuyCredits()
        {
            return View();
        }

        // POST: Payment/Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(int packageId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Örnek paketler (ileride DB'den gelebilir)
            var packages = new Dictionary<int, int>
            {
                { 1, 10 },   // 10 kredi
                { 2, 25 },   // 25 kredi
                { 3, 50 }    // 50 kredi
            };

            if (!packages.ContainsKey(packageId))
                return BadRequest("Geçersiz paket.");

            int creditsToAdd = packages[packageId];

            // 🔹 Fake ödeme işlemi (gerçek sistemde Stripe/Iyzico API çağrısı yapılır)
            bool paymentSuccess = true; // Test için direkt true
            if (!paymentSuccess)
                return BadRequest("Ödeme başarısız.");

            // Kullanıcıya kredi ekle
            user.Credits += creditsToAdd;
            await _userManager.UpdateAsync(user);

            // DB’ye kayıt
            var history = new PaymentHistory
            {
                UserId = user.Id,
                CreditsAdded = creditsToAdd,
                Amount = creditsToAdd * 5, // örnek: her kredi 5₺
                PaymentDate = DateTime.UtcNow
            };

            _context.PaymentHistories.Add(history);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{creditsToAdd} kredi hesabınıza eklendi!";

            return RedirectToAction("BuyCredits");
        }
    }
}
