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
                            HttpContext.Session.SetString("Grupo", dr["ID_Grupo"].ToString()); 

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
        //------------------------------------Para Grupo--------------------------------------------
        [HttpGet]
        public IActionResult Grupo(DatosModel datos) {
            TempData["ID_usuario"] = datos.IdUsuario;
            return View(datos);
        }
		[HttpPost]
		[HttpPost]
		public IActionResult Validar(DatosModel datos)
		{
			try
			{
				string query = "select adm from GRUPO where ID_Grupo = @ID";
				using (SqlConnection conn = new SqlConnection(cadenaCon))
				{
					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@ID", datos.grupo);
						conn.Open();
						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							if (dr.Read())
							{
								// Verificar si el valor recuperado es nulo
								if (dr.IsDBNull(0))
								{
									conn.Close(); // Cerrar la conexión aquí
									query = "UPDATE GRUPO SET adm = @Adm WHERE ID_Grupo = @Grupo";
									using (SqlCommand cmd2 = new SqlCommand(query, conn))
									{
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
            var profesores = GetProfesores();
            return View(profesores);
        }
        public List<ProfesorModel> GetProfesores()
        {
            List<ProfesorModel> profesores = new List<ProfesorModel>();
            string query = "SELECT Mat, Profesor, Nom_usuario, AP_PAT, AP_MAT, correo FROM ENCARGADOS RIGHT JOIN usuario ON Profesor = ID_usuario WHERE Tipo_usuario = 'Profesor' and Mat LIKE @Grupo";
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Grupo", HttpContext.Session.GetString("Grupo") + "%");
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
            List<ProfesorModel> profesores = GetProfesorById(idUsuario);
            ViewData["Profes"] = profesores;
            return View("Profes", profesores);
        }

        public List<ProfesorModel> GetProfesorById(string idUsuario)
        {
            List<ProfesorModel> profesores = new List<ProfesorModel>();
            //Falta filtro para solo profesores del grupo
            string query = "SELECT Mat, Profesor, Nom_usuario, AP_PAT, AP_MAT, correo FROM ENCARGADOS RIGHT JOIN usuario ON Profesor = ID_usuario WHERE ID_usuario = @Profe and Mat LIKE @Grupo";
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Profe", idUsuario);
                    cmd.Parameters.AddWithValue("@Grupo", HttpContext.Session.GetString("Grupo") + "%");
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
            string query = "SELECT ID_materia, Materia, llave_al as Codigo_Estudiante from UNI_APRE WHERE Grupo = @Grupo";
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Grupo", grupo);
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            materias.Add(new MateriaModel { ID = dr["ID_materia"].ToString(), Nombre = dr["Materia"].ToString() });
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
				query = "DELETE FROM INSCRITOS WHERE Llave = @Llave";
				using (SqlConnection conn = new SqlConnection(cadenaCon))
				{
					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@Llave", llave);
						conn.Open();
						cmd.ExecuteNonQuery();
						conn.Close();
					}
				}
				TempData["Mensaje"] = "Materia eliminada";
                datos.IdUsuario = HttpContext.Session.GetInt32("IdUsuario").GetValueOrDefault();
                return RedirectToAction("Materias", datos);
            } catch (Exception ex)
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
                string query = "SELECT COUNT(*) FROM UNI_APRE WHERE Grupo = @Grupo";
                int count;
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Grupo", grupo);
                        conn.Open();
                        count = (int)cmd.ExecuteScalar();
                        conn.Close();
                    }
                }
                string id = grupo + (count + 1).ToString("D2");
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
            } catch (Exception ex)
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
