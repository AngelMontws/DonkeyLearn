using DonkeyLearn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;

namespace DonkeyLearn.Controllers
{
    public class AdminController : Controller
    {
        public string grupo;
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
                            TempData["ID_usuario"] = datos.IdUsuario; // Almacena el valor de ID_usuario en TempData
                            return RedirectToAction("Grupo", datos);

                        }
                    }
                }
            }
           
        }
        [HttpGet]
        public IActionResult Grupo(DatosModel datos) {
            TempData["ID_usuario"] = datos.IdUsuario;
            return View(datos);
        }
        [HttpPost]
        public IActionResult Validar(DatosModel datos)
        {
            try
            {
                
                int id = (int)TempData["ID_usuario"];
                string query = "UPDATE GRUPO SET adm = " + id + " WHERE ID_Grupo = @Grupo";
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Grupo", datos.grupo);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                datos.IdUsuario = id;
                HttpContext.Session.SetInt32("IdUsuario", datos.IdUsuario);
                return RedirectToAction("MenuAdmin", datos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("Grupo");
            }
        }

    }
}
