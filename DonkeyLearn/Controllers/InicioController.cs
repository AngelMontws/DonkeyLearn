using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net.Mail;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System;
using Microsoft.Data.SqlClient;
using DonkeyLearn.Models;

namespace DonkeyLearn.Controllers
{
    public class InicioController : Controller
    {
        private MailMessage envio;
        string cadenaCon = "DATA SOURCE=.; INITIAL CATALOG=DONKEYLEARN; integrated security=true;";
        [HttpGet]
        public IActionResult Inicio()
        {
            return View();
        }
        public void Registrar(DatosModel datos)
        {
            if (string.IsNullOrEmpty(datos.Nombre) ||
                string.IsNullOrEmpty(datos.ApPaterno) ||
                string.IsNullOrEmpty(datos.ApMaterno) ||
                string.IsNullOrEmpty(datos.Contrasena) ||
                string.IsNullOrEmpty(datos.TipoUsuario) ||
                string.IsNullOrEmpty(datos.CorreoElectronico))
            {
                TempData["Error"] = "Todos los campos deben estar llenos";
                return;
            }
            else
            {
                try
                {
                    bool registrado;
                    using (SqlConnection con = new SqlConnection(cadenaCon))
                    {
                        SqlCommand cmd = new SqlCommand("sp_Registro", con);
                        cmd.Parameters.AddWithValue("@ID", datos.IdUsuario);
                        cmd.Parameters.AddWithValue("@Nombre", datos.Nombre);
                        cmd.Parameters.AddWithValue("@aP", datos.ApPaterno);
                        cmd.Parameters.AddWithValue("@aM", datos.ApMaterno);
                        cmd.Parameters.AddWithValue("@Contra", datos.Contrasena);
                        cmd.Parameters.AddWithValue("@Tipo", datos.TipoUsuario);
                        cmd.Parameters.AddWithValue("@correo", datos.CorreoElectronico);
                        cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                        cmd.Parameters.Add("Registrado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                        cmd.CommandType = CommandType.StoredProcedure;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        registrado = Convert.ToBoolean(cmd.Parameters["Registrado"].Value);
                    }
                    if (registrado)
                    {
                        TempData["Mensaje"] = "Usuario Registrado";
                    }
                    else
                    {
                        TempData["Error"] = "Estás intentando registrar a un usuario ya existente";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.ToString();
                }
            }
        }
        [HttpPost]
        public IActionResult Alumno(DatosModel datos)
        {
            
            try
            {
                datos.TipoUsuario = "Alumno";
                Registrar(datos);
                return RedirectToAction("Inicio");

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("Inicio");
            }
        }
        [HttpPost]
        public IActionResult Login(DatosModel datos)
        {
            try
            {
                if (datos.Contrasena == null || datos.IdUsuario == 0)
                {
                    TempData["Error"] = "Introduce tu usuario y contraseña";
                    return RedirectToAction("Inicio", datos);
                }
                else
                {
                    using (SqlConnection con = new SqlConnection(cadenaCon))
                    {
                        SqlCommand cmd = new SqlCommand("sp_Logueo", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IdUsuario", datos.IdUsuario);
                        cmd.Parameters.AddWithValue("@Contra", datos.Contrasena);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            datos.IdUsuario = reader.GetInt32(0);
                            datos.Nombre = reader.GetString(1);
                            datos.ApPaterno = reader.GetString(2);
                            datos.ApMaterno = reader.GetString(3);
                            datos.Contrasena = reader.GetString(4);
                            datos.TipoUsuario = reader.GetString(5);
                            datos.CorreoElectronico = reader.GetString(6);
                            HttpContext.Session.SetInt32("IdUsuario", datos.IdUsuario);
                            HttpContext.Session.SetString("Nombre", datos.Nombre);
                            HttpContext.Session.SetString("ApPaterno", datos.ApPaterno);
                            HttpContext.Session.SetString("ApMaterno", datos.ApMaterno);
                            HttpContext.Session.SetString("Contrasena", datos.Contrasena);
                            HttpContext.Session.SetString("TipoUsuario", datos.TipoUsuario);
                            HttpContext.Session.SetString("CorreoElectronico", datos.CorreoElectronico);
                            switch (datos.TipoUsuario)
                            {
                                case "Alumno":
                                return RedirectToAction("Menu", "Alumno", datos);
                                case "Administrador":
                                return RedirectToAction("MenuAdmin", "Admin", datos);
                                case "Profesor":
                                return RedirectToAction("MenuProfe", "Profe", datos);
                                default:
                                return RedirectToAction("Inicio");
                            }
                        }
                        else
                        {
                            TempData["Error"] = "Usuario o contraseña inválidos";
                            return RedirectToAction("Inicio");

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("Inicio");
            }
        }
        [HttpGet]
        public IActionResult RecuperarContra()
        {
            return View();
        }
        [HttpPost]
        public IActionResult RecuperarContra(DatosModel datos)
        {
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                SqlCommand cmd = new SqlCommand("ValidarCorreo", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@correo", datos.CorreoElectronico);
                try
                {
                    conn.Open();
                    SqlDataReader lector = cmd.ExecuteReader();
                    if (lector.Read())
                    {
                        datos.Nombre = lector["Nom_usuario"].ToString();
                        datos.Contrasena = lector["Contra_usuario"].ToString();
                        envio = new MailMessage();
                        envio.From = new MailAddress(Correo.direccion, Correo.alias, System.Text.Encoding.UTF8);
                        envio.To.Add(datos.CorreoElectronico.Trim());
                        envio.Subject = "Recuperé tu contraseña";
                        envio.Body = $"Hola {datos.Nombre}!!! Aquí está la contraseña, ahora déjame dormir: {datos.Contrasena}";
                        envio.IsBodyHtml = true;
                        envio.Priority = MailPriority.High;
                        conn.Close();
                        SmtpClient smtp = new SmtpClient();
                        smtp.UseDefaultCredentials = false;
                        smtp.Port = 25;
                        smtp.Host = "smtp.gmail.com";
                        smtp.Credentials = new System.Net.NetworkCredential(Correo.direccion, Correo.contra);
                        ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
                        smtp.EnableSsl = true;
                        smtp.Send(envio);
                        conn.Close();
                    }
                    else
                        conn.Close();
                    TempData["Mensaje"] = "Enviaremos un correo si lo detectamos";
                    return RedirectToAction("Inicio");
                }
                catch (System.Exception ex)
                {
                    TempData["Error"] = ex.ToString();
                    return RedirectToAction("Inicio");
                }
            }
        }
        [HttpGet]
        public IActionResult Inicio2()
        {
            return View();
        }

        public IActionResult Acercade()
        {
            return View();
        }
    }
}
