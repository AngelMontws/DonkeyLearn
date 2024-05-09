using Microsoft.AspNetCore.Mvc;

namespace DonkeyLearn.Controllers
{
    public class AlumnoController : Controller
    {
        public IActionResult Menu()
        {
            return View();
        }
    }
}
