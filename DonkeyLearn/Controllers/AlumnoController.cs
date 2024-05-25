using DonkeyLearn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;

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
				string query = "SELECT ID_materia, Materia, Llave, Alumno FROM INSCRITOS join UNI_APRE on Llave = llave_al WHERE Alumno = @Alumno";
				using (SqlCommand cmd = new SqlCommand(query, con))
				{
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
			int id = (int)HttpContext.Session.GetInt32("IdUsuario");
			string query = "SELECT llave_al FROM UNI_APRE WHERE llave_al = @llave";
			using (SqlConnection con = new SqlConnection(cadenaCon))
			{
				con.Open();
				using (SqlCommand cmd = new SqlCommand(query, con))
				{
					cmd.Parameters.AddWithValue("@llave", llave);
					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						if (dr.Read())
						{
							con.Close();
							query = "INSERT INTO INSCRITOS VALUES (@Alumno, @Llave)";
							using (SqlCommand cmd2 = new SqlCommand(query, con))
							{
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
				string query = "SELECT ID_cues, Nom_cues FROM CUESTIONARIO Where Materia = @Materia";
				using (SqlCommand cmd = new SqlCommand(query, con))
				{
					cmd.Parameters.AddWithValue("@Materia", id);
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
                string query = "SELECT * FROM PREGUNTA WHERE Cuestionario = @ID_cues";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
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
                string query = "SELECT * FROM RESPUESTA WHERE Pregunta = @ID_pregunta";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
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
            int puntaje = CalcularPuntaje(cuestionario);
            string idUsuario = HttpContext.Session.GetInt32("IdUsuario").ToString();
            string idProgreso = idUsuario + cuestionario.ID + DateTime.Now.ToString("dd/MM");

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
                    cmd.Parameters.AddWithValue("@Cuestionario", cuestionario.ID); // Asegúrate de que estás proporcionando este parámetro
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }

            TempData["Mensaje"] = "Puntaje registrado";
            return RedirectToAction("Menu");
        }


        private int CalcularPuntaje(CuestionarioModel cuestionario)
        {
            int puntaje = 0;
            foreach (var pregunta in cuestionario.Preguntas)
            {
                if (pregunta.RespuestaSeleccionada != null && pregunta.RespuestaSeleccionada.EsCorrecta)
                {
                    puntaje++;
                }
            }
            return puntaje;
        }

    }
}
