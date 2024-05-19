using DonkeyLearn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

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
                            HttpContext.Session.SetString("Materia", dr["Materia"].ToString());
                            return View();
                        }
                        else
                        {
                            TempData["ID_usuario"] = datos.IdUsuario;
                            return RedirectToAction("Unirse", datos);
                        }
                    }
                }
            }
        }
        //------------------------------------Para Validar--------------------------------------------
        [HttpGet]
        public IActionResult Unirse(DatosModel datos)
        {
            TempData["ID_usuario"] = datos.IdUsuario;
            return View(datos);
        }
        [HttpPost]
        public IActionResult Validar(DatosModel datos)
        {
            try
            {
                int id = (int)TempData["ID_usuario"];
                string query = "UPDATE ENCARGADOS SET Profesor = " + id + " WHERE Materia = @Grupo";
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
                HttpContext.Session.SetString("Grupo", datos.grupo);  // Establecer el valor de la sesión "Grupo" aquí
                return RedirectToAction("MenuProfe", datos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("Unirse");
            }
        }
        //------------------------------------Para Cuestionario--------------------------------------------
        [HttpGet]
        public IActionResult Cuestionario()
        {
            var cuestionario = new CuestionarioModel
            {
                Preguntas = new List<PreguntaModel>
                {
                    new PreguntaModel
                    {
                        Respuestas = new List<RespuestaModel>
                        {
                            new RespuestaModel(),
                            new RespuestaModel(),
                            new RespuestaModel()
                        }
                    }
                }
            };

            string query = "SELECT Materia, NomMat FROM ENCARGADOS LEFT JOIN UNI_APRE ON Materia = ID_materia WHERE Profesor = @Profesor";
            List<SelectListItem> items = new List<SelectListItem>();

            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Profesor", HttpContext.Session.GetInt32("IdUsuario"));
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            items.Add(new SelectListItem { Value = dr["Materia"].ToString(), Text = dr["NomMat"].ToString() });
                        }
                    }
                }
            }

            ViewBag.Materias = items;

            return View(cuestionario);
        }

        [HttpPost]
        public IActionResult InsertarCuestionario(CuestionarioModel cuestionario, string Materia)
        {
            try
            {
                string materia = Materia;
                string idCues = GenerarIdCuestionario(materia);
                string queryCuestionario = "INSERT INTO CUESTIONARIO VALUES (@ID_cues, @Cuestionario, @Materia)";
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand(queryCuestionario, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID_cues", idCues);
                        cmd.Parameters.AddWithValue("@Cuestionario", cuestionario.NombreCuestionario);
                        cmd.Parameters.AddWithValue("@Materia", materia);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

                int preguntaNum = 1;
                foreach (var pregunta in cuestionario.Preguntas)
                {
                    string idPregunta = idCues + "P" + preguntaNum.ToString("D2");

                    string queryPregunta = "INSERT INTO PREGUNTA VALUES (@ID_pregunta, @pregunta, @ID_cues)";
                    using (SqlConnection conn = new SqlConnection(cadenaCon))
                    {
                        using (SqlCommand cmd = new SqlCommand(queryPregunta, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID_pregunta", idPregunta);
                            cmd.Parameters.AddWithValue("@pregunta", pregunta.Pregunta);
                            cmd.Parameters.AddWithValue("@ID_cues", idCues);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }
                    }

                    int respuestaNum = 1;
                    foreach (var respuesta in pregunta.Respuestas)
                    {
                        string idRespuesta = idPregunta + "R" + respuestaNum;

                        string queryRespuesta = "INSERT INTO Respuesta VALUES (@ID_res, @Respuesta, @Correc_inco, @ID_pregunta)";
                        using (SqlConnection conn = new SqlConnection(cadenaCon))
                        {
                            using (SqlCommand cmd = new SqlCommand(queryRespuesta, conn))
                            {
                                cmd.Parameters.AddWithValue("@ID_res", idRespuesta);
                                cmd.Parameters.AddWithValue("@Respuesta", respuesta.Respuesta);
                                cmd.Parameters.AddWithValue("@Correc_inco", respuesta.EsCorrecta ? 1 : 0);
                                cmd.Parameters.AddWithValue("@ID_pregunta", idPregunta);
                                conn.Open();
                                cmd.ExecuteNonQuery();
                                conn.Close();
                            }
                        }

                        respuestaNum++;
                    }
                    preguntaNum++;
                }
                TempData["Mensaje"] = "Cuestionario creado correctamente";
                return RedirectToAction("Cuestionario");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("Cuestionario");
            }
        }

        private string GenerarIdCuestionario(string materia)
        {
            string idCues;
            string queryGetLastId = "SELECT TOP 1 ID_cues FROM CUESTIONARIO ORDER BY ID_cues DESC";

            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(queryGetLastId, conn))
                {
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    conn.Close();

                    if (result != null)
                    {
                        string lastId = result.ToString();
                        string numericPart = lastId.Substring(materia.Length + 1); // +1 for 'C'
                        int number = int.Parse(numericPart);
                        number++;
                        idCues = materia + "C" + number.ToString("D2");
                    }
                    else
                    {
                        idCues = materia + "C01";
                    }
                }
            }

            return idCues;
        }
    }
}