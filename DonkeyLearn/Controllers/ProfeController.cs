using DonkeyLearn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DonkeyLearn.Controllers
{
    public class ProfeController : Controller
    {
        string cadenaCon = "DATA SOURCE=.; INITIAL CATALOG=DONKEYLEARN; integrated security=true;";
        public IActionResult MenuProfe(DatosModel datos)
        {
            string query = "SELECT * FROM ENCARGADOS WHERE Profesor = " + datos.IdUsuario;
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
                            return RedirectToAction("Unirse");
                        }
                    }
                }
            }
        }
        [HttpGet]
        public IActionResult Unirse()
        {
            return View();
        }
    }
}
