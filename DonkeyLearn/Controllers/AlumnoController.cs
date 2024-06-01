using DonkeyLearn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DonkeyLearn.Controllers
{
	public class AlumnoController : Controller
	{
		string cadenaCon = "DATA SOURCE=.; INITIAL CATALOG=DONKEYLEARN; integrated security=true;";
		//-----------------------------------INICIO-----------------------------------
		[HttpGet]
		public IActionResult Menu(DatosModel datos)
		{
			try
			{
				List<MateriaModel> materias = Materias();
				ViewData["Materias"] = materias;
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = ex.Message;
				return RedirectToAction("Inicio", "Inicio");
			}
		}
        public List<MateriaModel> Materias()
        {
            List<MateriaModel> inscripcion = new List<MateriaModel>();
            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("sp_GetMateriasByAlumno", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Alumno", HttpContext.Session.GetInt32("IdUsuario"));
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            MateriaModel materia = new MateriaModel
                            {
                                Nombre = dr["Materia"].ToString(),
                                ID = dr["ID_materia"].ToString()
                            };
                            inscripcion.Add(materia);
                        }
                    }
                }
                con.Close();
            }
            return inscripcion;
        }
		[HttpPost]
		public IActionResult AñadirCLase(DatosModel datos, string llave)
		{
			try
            {
                int id = (int)HttpContext.Session.GetInt32("IdUsuario");
                using (SqlConnection con = new SqlConnection(cadenaCon))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("BuscaClase", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@llave", llave);
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                con.Close();
                                using (SqlCommand cmd2 = new SqlCommand("sp_InsertarInscritos", con))
                                {
                                    cmd2.CommandType = CommandType.StoredProcedure;
                                    cmd2.Parameters.AddWithValue("@Alumno", id);
                                    cmd2.Parameters.AddWithValue("@Llave", llave);
                                    con.Open();
                                    cmd2.ExecuteNonQuery();
                                }
                                TempData["Mensaje"] = "Materia agregada";
                                datos.IdUsuario = id;
                                HttpContext.Session.SetInt32("IdUsuario", datos.IdUsuario);
                                return RedirectToAction("Menu", datos);
                            }
                            else
                            {
                                TempData["Error"] = "No se encontró la materia";
                                datos.IdUsuario = id;
                                HttpContext.Session.SetInt32("IdUsuario", datos.IdUsuario);
                                return RedirectToAction("Menu", datos);
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Menu", datos);
            }
		}
		//-----------------------------------CUESTIONARIOS-----------------------------------
		[HttpGet]
		public IActionResult EscogerCues(MateriaModel materia)
		{
			try
			{
				HttpContext.Session.SetString("Materia", materia.ID);
				List<CuestionarioModel> cuestionarios = CargarCuestionarios(materia.ID);
				ViewData["Cuestionarios"] = cuestionarios;
				return View();
			}
			catch (Exception ex)
			{
				TempData["Error"] = ex.Message;
				return RedirectToAction("Menu");
			}
		}
        public List<CuestionarioModel> CargarCuestionarios(string id)
        {
            List<CuestionarioModel> Cuestionarios = new List<CuestionarioModel>();
            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("sp_CargarCuestionarios", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Materia", id);
                    cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            CuestionarioModel cuestionario = new CuestionarioModel
                            {
                                ID = dr["ID_cues"].ToString(),
                                NombreCuestionario = dr["Nom_cues"].ToString()
                            };
                            Cuestionarios.Add(cuestionario);
                        }
                    }
                }
                con.Close();
            }
            return Cuestionarios;
        }
        [HttpGet]
        public IActionResult VerCuestionario(CuestionarioModel cuestionarios)
        {
            try
            {
                HttpContext.Session.SetString("Cuestionario", cuestionarios.ID);
                CuestionarioModel cuestionario = CargarCuestionario(cuestionarios.ID);
                ViewData["Title"] = "VerCuestionario";
                ViewData["Cuestionario"] = cuestionario;
                ViewData["CuestionarioID"] = cuestionarios.ID; // Almacena el ID del cuestionario en ViewData
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Menu");
            }
        }
        private CuestionarioModel CargarCuestionario(string idCuestionario)
        {
            CuestionarioModel cuestionario = new CuestionarioModel();
            cuestionario.Preguntas = new List<PreguntaModel>();

            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("sp_CargarPreguntas", con)) // Replace the query with the stored procedure name
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID_cues", idCuestionario);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            PreguntaModel pregunta = new PreguntaModel
                            {
                                Pregunta = dr["pregunta"].ToString(),
                                Respuestas = CargarRespuestas(dr["ID_pregunta"].ToString())
                            };
                            cuestionario.Preguntas.Add(pregunta);
                        }
                    }
                }
                con.Close();
            }
            return cuestionario;
        }
        private List<RespuestaModel> CargarRespuestas(string idPregunta)
        {
            List<RespuestaModel> respuestas = new List<RespuestaModel>();

            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("sp_CargarRespuestas", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID_pregunta", idPregunta);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            RespuestaModel respuesta = new RespuestaModel
                            {
                                Respuesta = dr["respuesta"].ToString(),
                                EsCorrecta = (bool)dr["correc_inco"]
                            };
                            respuestas.Add(respuesta);
                        }
                    }
                }
                con.Close();
            }
            return respuestas;
        }
        [HttpPost]
        public IActionResult SubirPuntaje(CuestionarioModel cuestionario)
        {
            try
            {
                int puntaje = cuestionario.Puntaje; // Recupera el puntaje directamente del modelo
                string idUsuario = HttpContext.Session.GetInt32("IdUsuario").ToString();
                string idProgreso = idUsuario + cuestionario.ID;

                using (SqlConnection con = new SqlConnection(cadenaCon))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("subir_puntaje", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ID_usuario", idUsuario);
                        cmd.Parameters.AddWithValue("@Puntaje", puntaje);
                        cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ID_progreso", idProgreso);
                        cmd.Parameters.AddWithValue("@Cuestionario", cuestionario.ID);
                        cmd.Parameters.AddWithValue("@Materia", HttpContext.Session.GetString("Materia"));
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                TempData["Mensaje"] = "Puntaje registrado";
                return RedirectToAction("Menu");
            } catch (Exception ex)
            {
                TempData["Error"] = "Al parecer ya has contestado este cuestionario";
                return RedirectToAction("Menu");
            }
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Inicio", "Inicio");
        }
        //-----------------------------------Reportes-----------------------------------
        [HttpGet]
        public IActionResult Reportes()
        {
			string query = "Select u.ID_materia, u.Materia from UNI_APRE u join INSCRITOS i on u.llave_al = i.Llave Where i.Alumno =  @id";
			List<SelectListItem> items = new List<SelectListItem>();

			using (SqlConnection conn = new SqlConnection(cadenaCon))
			{
				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("IdUsuario"));
					conn.Open();
					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							items.Add(new SelectListItem { Value = dr["ID_materia"].ToString(), Text = dr["Materia"].ToString() });
						}
					}
				}
			}

			ViewBag.Materias = items;

			return View();
		}
        [HttpPost]
        public IActionResult Reportes(string Materia)
        {
            List<string> Cues = new List<string>();
            List<decimal> Puntajes = new List<decimal>();
            List<string> NomCues = new List<string>();
			List<SelectListItem> items = new List<SelectListItem>();
			string query = "select c.ID_cues from CUESTIONARIO c join UNI_APRE u on c.Materia = ID_materia where c.Materia = @mat";
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@mat", Materia);
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Cues.Add(dr["ID_cues"].ToString());
                        }
                    }
                    conn.Close();
                }
                query = "select (cast(p.Puntaje AS DECIMAL(10,2))/(SELECT COUNT(p.ID_pregunta) FROM PREGUNTA p WHERE p.Cuestionario = @cues) * 100) as Puntaje, c.Nom_Cues from Progres_Estu p join CUESTIONARIO c on p.Cuestionario = c.ID_cues where Cuestionario = @cues and ID_usuario = @id";
                foreach (string cue in Cues)
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cues", cue);
                        cmd.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("IdUsuario"));
                        conn.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Puntajes.Add((decimal)dr["Puntaje"]);
                                NomCues.Add(dr["Nom_Cues"].ToString());
                            }
                        }
                        conn.Close();
                    }
                }
				query = "Select u.ID_materia, u.Materia from UNI_APRE u join INSCRITOS i on u.llave_al = i.Llave Where i.Alumno =  @id";
				
				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("IdUsuario"));
					conn.Open();
					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							items.Add(new SelectListItem { Value = dr["ID_materia"].ToString(), Text = dr["Materia"].ToString() });
						}
					}
				}				
			}
			ViewBag.Materias = items;
			ViewBag.Puntajes = Puntajes;
			ViewBag.NomCues = NomCues;
			return View();
		}
    }
}
    
