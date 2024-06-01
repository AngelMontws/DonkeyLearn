using DonkeyLearn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DonkeyLearn.Controllers
{
    public class AdminController : Controller
    {
        //------------------------------------Para Iniciar--------------------------------------------
        public string grupo;
        string cadenaCon = "DATA SOURCE=.; INITIAL CATALOG=DONKEYLEARN; integrated security=true;";
        public IActionResult MenuAdmin(DatosModel datos)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_ObtenerGrupoPorAdm", conn)) // Change the stored procedure name here
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@adm", HttpContext.Session.GetInt32("IdUsuario")); // Change the parameter name here
                        conn.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                HttpContext.Session.SetString("Grupo", dr["ID_Grupo"].ToString());
                                List<string> labels = new List<string>();
                                List<decimal> data = new List<decimal>();

                                using (SqlConnection con = new SqlConnection(cadenaCon))
                                {
                                    using (SqlCommand cmd2 = new SqlCommand("PromedioGrupal", con))
                                    {
                                        cmd2.CommandType = CommandType.StoredProcedure;
                                        cmd2.Parameters.AddWithValue("@materia", HttpContext.Session.GetString("Grupo"));
                                        con.Open();
                                        using (SqlDataReader dr2 = cmd2.ExecuteReader())
                                        {
                                            while (dr2.Read())
                                            {
                                                labels.Add(dr2["Materia"].ToString());
                                                data.Add(Convert.ToDecimal(dr2["promedio_puntaje"]));
                                            }
                                        }
                                        con.Close();
                                    }                                    
                                }

                                ViewBag.Labels = labels;
                                ViewBag.Data = data;

                                return View();
                            }
                            else
                            {
                                TempData["ID_usuario"] = datos.IdUsuario;
                                return RedirectToAction("Grupo", datos);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("Inicio", "Inicio");
            }
        }


        public JsonResult GetChartData()
        {
            List<string> labels = new List<string>();
            List<decimal> data = new List<decimal>();

            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT Materia, avg(cast(puntaje AS DECIMAL(10,2))) AS promedio_puntaje FROM Progres_Estu inner join UNI_APRE on uni_ap=ID_materia where uni_ap like @materia + '%' GROUP BY Materia;", conn))
                {
                    cmd.Parameters.AddWithValue("@materia", HttpContext.Session.GetString("Grupo"));
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            labels.Add(dr["Materia"].ToString());
                            data.Add(Convert.ToDecimal(dr["promedio_puntaje"]));
                        }
                    }
                    conn.Close();
                }
            }

            return Json(new { Labels = labels, Data = data });
        }

        //------------------------------------Para Grupo--------------------------------------------
        [HttpGet]
        public IActionResult Grupo(DatosModel datos)
        {
            try
            {
                TempData["ID_usuario"] = datos.IdUsuario;
                return View(datos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("Grupo");
            }
        }
      
        [HttpPost]
        public IActionResult Validar(DatosModel datos)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_ObtenerAdmPorGrupo", conn)) // Change the stored procedure name here
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ID", datos.grupo);
                        conn.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                if (dr.IsDBNull(0))
                                {
                                    conn.Close(); // Cerrar la conexión aquí
                                    using (SqlCommand cmd2 = new SqlCommand("sp_ActualizarAdmEnGrupo", conn))
                                    {
                                        cmd2.CommandType = CommandType.StoredProcedure;
                                        cmd2.Parameters.AddWithValue("@Grupo", datos.grupo);
                                        cmd2.Parameters.AddWithValue("@Adm", HttpContext.Session.GetInt32("IdUsuario"));
                                        conn.Open(); // Abrir la conexión aquí
                                        cmd2.ExecuteNonQuery();
                                        conn.Close(); // Cerrar la conexión aquí
                                    }
                                    datos.IdUsuario = (int)HttpContext.Session.GetInt32("IdUsuario");
                                    HttpContext.Session.SetInt32("IdUsuario", datos.IdUsuario);
                                    HttpContext.Session.SetString("Grupo", datos.grupo);  // Establecer el valor de la sesión "Grupo" aquí
                                    return RedirectToAction("MenuAdmin", datos);
                                }
                                else
                                {
                                    conn.Close();
                                    TempData["Error"] = "Parece que hubo un error, reingresa tu clave de acceso";
                                    return RedirectToAction("Grupo", datos);
                                }
                            }
                            else
                            {
                                conn.Close();
                                TempData["Error"] = "Parece que hubo un error, reingresa tu clave de acceso";
                                return RedirectToAction("Grupo", datos);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("Grupo");
            }
        }


        //------------------------------------Para Profes--------------------------------------------
        [HttpGet]
        public IActionResult Profes(DatosModel datos)
        {
            try
            {
                var profesores = GetProfesores();
                return View(profesores);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("MenuAdmin", datos);
            }
        }
        public List<ProfesorModel> GetProfesores()
        {
            
            List<ProfesorModel> profesores = new List<ProfesorModel>();
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand("CargarProfes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Grupo", HttpContext.Session.GetString("Grupo"));
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            profesores.Add(new ProfesorModel
                            {
                                Materia = dr["Mat"].ToString(),
                                Profesor = dr["Profesor"].ToString(),
                                Nom_usuario = dr["Nom_usuario"].ToString(),
                                AP_PAT = dr["AP_PAT"].ToString(),
                                AP_MAT = dr["AP_MAT"].ToString(),
                                Correo = dr["correo"].ToString()
                            });
                        }
                    }
                    conn.Close();
                }
            }
            return profesores;
        }

        public IActionResult BuscarProfesor(string idUsuario)
        {
            try
            {
                List<ProfesorModel> profesores = GetProfesorById(idUsuario);
                ViewData["Profes"] = profesores;
                return View("Profes", profesores);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("Profes");
            }
        }
        public List<ProfesorModel> GetProfesorById(string idUsuario)
        {
            List<ProfesorModel> profesores = new List<ProfesorModel>();
            //Falta filtro para solo profesores del grupo
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand("BuscarProfes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Profe", idUsuario);
                    cmd.Parameters.AddWithValue("@Grupo", HttpContext.Session.GetString("Grupo"));
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            profesores.Add(new ProfesorModel
                            {
                                Materia = dr["Mat"].ToString(),
                                Profesor = dr["Profesor"].ToString(),
                                Nom_usuario = dr["Nom_usuario"].ToString(),
                                AP_PAT = dr["AP_PAT"].ToString(),
                                AP_MAT = dr["AP_MAT"].ToString(),
                                Correo = dr["correo"].ToString()
                            });
                        }
                    }
                    conn.Close();
                }
            }
            return profesores;
        }

        //------------------------------------Para Materia--------------------------------------------
        [HttpGet]
        public IActionResult Materias(DatosModel datos)
        {
            try
            {
                List<MateriaModel> materias = ObtenerMaterias(grupo);
                ViewData["Materias"] = materias;
                ViewData["Datos"] = datos;
                return View(materias);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("MenuAdmin");
            }
        }
        public List<MateriaModel> ObtenerMaterias(string grupo)
        {
            grupo = HttpContext.Session.GetString("Grupo");  // Obtener el valor de la sesión "Grupo" aquí
            List<MateriaModel> materias = new List<MateriaModel>();
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand("BuscarMaterias", conn)) // Change the stored procedure name here
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Grupo", grupo);
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            materias.Add(new MateriaModel { ID = dr["ID_materia"].ToString(), Nombre = dr["Materia"].ToString(), llave = dr["llave_al"].ToString() });
                        }
                    }
                    conn.Close();
                }
            }
            return materias;
        }
        [HttpPost]
        public IActionResult EliminarMateria(string id, string llave, DatosModel datos)
        {
            try
            {
                string query = "DELETE FROM Encargados WHERE Mat = @ID_materia";
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID_materia", id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                query = "DELETE FROM UNI_APRE WHERE ID_materia = @ID_materia";
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID_materia", id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                query = "SELECT * FROM INSCRITOS WHERE Llave = @Llave";
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Llave", llave);
                        conn.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                query = "DELETE FROM Progres_Estu WHERE uni_ap = @uni";
                                using (SqlConnection conn2 = new SqlConnection(cadenaCon))
                                {
                                    using (SqlCommand cmd2 = new SqlCommand(query, conn2))
                                    {
                                        cmd2.Parameters.AddWithValue("@uni", id);
                                        conn2.Open();
                                        cmd2.ExecuteNonQuery();
                                        conn2.Close();
                                    }
                                }
                                query = "DELETE FROM INSCRITOS WHERE Llave = @Llave";
                                using (SqlConnection conn2 = new SqlConnection(cadenaCon))
                                {
                                    using (SqlCommand cmd2 = new SqlCommand(query, conn2))
                                    {
                                        cmd2.Parameters.AddWithValue("@Llave", llave);
                                        conn2.Open();
                                        cmd2.ExecuteNonQuery();
                                        conn2.Close();
                                    }
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                TempData["Mensaje"] = "Materia eliminada";
                datos.IdUsuario = HttpContext.Session.GetInt32("IdUsuario").GetValueOrDefault();
                return RedirectToAction("Materias", datos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                datos.IdUsuario = HttpContext.Session.GetInt32("IdUsuario").GetValueOrDefault();
                return RedirectToAction("Materias", datos);
            }
        }
        [HttpPost]
        public IActionResult CrearClase(string nombreClase, DatosModel datos)
        {
            try
            {
                string grupo = HttpContext.Session.GetString("Grupo");
                // Generate a random number between 1 and 99
                int count = new Random().Next(1, 100);

                // Format the count as a 2-digit number
                string countFormatted = count.ToString("D2");

                // Concatenate the formatted count with the grupo
                string id = grupo + countFormatted;
                string queryInsert = "INSERT INTO UNI_APRE VALUES (@ID, @Materia, @Grupo, @Codigo_Al)";
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand(queryInsert, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.Parameters.AddWithValue("@Materia", nombreClase);
                        cmd.Parameters.AddWithValue("@Grupo", grupo);
                        cmd.Parameters.AddWithValue("@Codigo_Al", CodigoAlumno());
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                queryInsert = "INSERT INTO ENCARGADOS (Mat) VALUES (@ID)";
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand(queryInsert, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                TempData["Mensaje"] = "Clase creada";
                datos.IdUsuario = HttpContext.Session.GetInt32("IdUsuario").GetValueOrDefault();
                return RedirectToAction("Materias", datos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                datos.IdUsuario = HttpContext.Session.GetInt32("IdUsuario").GetValueOrDefault();
                return RedirectToAction("Materias");
            }
        }
        public string CodigoAlumno()
        {
            string Captcha = "";
            for (int i = 0; i <= 2; i++)
            {
                var guid = Guid.NewGuid();
                var justNumbers = new String(guid.ToString().Where(Char.IsDigit).ToArray());
                var seed = int.Parse(justNumbers.Substring(0, 4));
                var random = new Random(seed);
                var value = random.Next(0, 9);
                Captcha = Captcha + value.ToString();
                int numero = random.Next(26);
                char letra = (char)(((int)'a') + numero);
                Captcha += letra;
            }
            return Captcha;
        }
        public string GenerarCodigo()
        {
            string Captcha = "";
            for (int i = 0; i < 6; i++)
            {
                var guid = Guid.NewGuid();
                var justNumbers = new String(guid.ToString().Where(Char.IsDigit).ToArray());
                var seed = int.Parse(justNumbers.Substring(0, 4));
                var random = new Random(seed);
                var value = random.Next(0, 9);
                Captcha = Captcha + value.ToString();
                int numero = random.Next(26);
                char letra = (char)(((int)'a') + numero);
                Captcha += letra;
            }
            return Captcha;
        }
        //------------------------------------Para Salir--------------------------------------------
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Inicio", "Inicio");
        }


    }
}
