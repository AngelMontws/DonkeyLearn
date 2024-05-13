using DonkeyLearn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DonkeyLearn.Controllers
{
    public class AdminController : Controller
    {
        string cadenaCon = "DATA SOURCE=.; INITIAL CATALOG=DONKEYLEARN; integrated security=true;" ;
        public IActionResult MenuAdmin(DatosModel datos)
        {
            string query = "SELECT * FROM GRUPO WHERE adm = " + datos.IdUsuario;
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return View();
                        }
                        else
                        {
                            return RedirectToAction("Grupo");
                        }
                    }
                }
            }
           
        }
        [HttpGet]
        public IActionResult Grupo() {
            return View();
        }
        [HttpPost]
        public IActionResult Validar()
        {
            return View();
        }
    }
}
