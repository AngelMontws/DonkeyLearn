using Microsoft.AspNetCore.Mvc;

namespace DonkeyLearn.Controllers
{
    public class AlumnoController : Controller
    {
        string cadenaCon = "DATA SOURCE=.; INITIAL CATALOG=DONKEYLEARN; integrated security=true;";
        public IActionResult Menu()
        {
            return View();
        }
    }
}
