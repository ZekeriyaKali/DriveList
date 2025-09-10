using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DriveListApi.Controllers
{
    public class AiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Predict([FromBody] CarInputModel modelInput)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.Credits <= 0)
                return BadRequest("Yeterli krediniz yok.");

            // Yapay zeka tahmini (AI servisini çağır)
            var prediction = await _aiService.GetPrediction(modelInput);

            // Krediyi düş
            user.Credits -= 1;
            await _userManager.UpdateAsync(user);

            return Ok(new { prediction, remainingCredits = user.Credits });
        }
    }
}
