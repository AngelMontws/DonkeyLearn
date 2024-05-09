using Microsoft.AspNetCore.Mvc;

namespace DonkeyLearn.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult MenuAdmin()
        {
            return View();
        }
    }
}
