using Microsoft.AspNetCore.Mvc;

namespace DriveListApi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
