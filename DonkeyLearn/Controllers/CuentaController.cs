using DonkeyLearn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DonkeyLearn.Controllers
{
    public class CuentaController : Controller
    {
        string cadenaCon = "DATA SOURCE=.; INITIAL CATALOG=DONKEYLEARN; integrated security=true;";
        [HttpGet]
        public IActionResult Cuenta()
        {
            DatosModel datos = new DatosModel
            {
                Nombre = HttpContext.Session.GetString("Nombre"),
                ApPaterno = HttpContext.Session.GetString("ApPaterno"),
                ApMaterno = HttpContext.Session.GetString("ApMaterno"),
                CorreoElectronico = HttpContext.Session.GetString("CorreoElectronico"),
                IdUsuario = HttpContext.Session.GetInt32("IdUsuario").GetValueOrDefault(),
                TipoUsuario = HttpContext.Session.GetString("TipoUsuario"),
                Contrasena = HttpContext.Session.GetString("Contrasena")
            };

            return View(datos);
        }
        [HttpPost]
        public IActionResult Actualizar(DatosModel datos)
        {
            HttpContext.Session.SetInt32("IdUsuario", datos.IdUsuario);
            HttpContext.Session.SetString("Nombre", datos.Nombre);
            HttpContext.Session.SetString("ApPaterno", datos.ApPaterno);
            HttpContext.Session.SetString("ApMaterno", datos.ApMaterno);
            HttpContext.Session.SetString("Contrasena", datos.Contrasena);
            HttpContext.Session.SetString("CorreoElectronico", datos.CorreoElectronico);
            SqlConnection conec = new SqlConnection(cadenaCon);
            try
            {
                conec.Open();
                using (SqlCommand cmd = new SqlCommand("actualizar", conec))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", datos.IdUsuario);
                    cmd.Parameters.AddWithValue("@Nom_usuario", datos.Nombre);
                    cmd.Parameters.AddWithValue("@AP_PAT", datos.ApPaterno);
                    cmd.Parameters.AddWithValue("@AP_Mat", datos.ApMaterno);
                    cmd.Parameters.AddWithValue("@Contra", datos.Contrasena); // Pasar la contraseña sin encriptar
                    cmd.Parameters.AddWithValue("@correo", datos.CorreoElectronico);
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas == 0)
                    {
                        TempData["Error"] = "No se actualizó ninguna fila. Verifique los datos y vuelva a intentarlo.";
                    }
                    else
                    {
                        TempData["Mensaje"] = "Registro Actualizado";
                    }
                }

                return View("Cuenta", datos);
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = ex.ToString();
                conec.Close();
                return View("Cuenta", datos);
            }
        }
    }
}
