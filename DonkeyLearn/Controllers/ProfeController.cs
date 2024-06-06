using DonkeyLearn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DonkeyLearn.Controllers
{
    public class ProfeController : Controller
    {
        string cadenaCon = "DATA SOURCE=.; INITIAL CATALOG=DONKEYLEARN; integrated security=true;";
        public IActionResult MenuProfe(DatosModel datos)
        {
            try
            {
                string query = "SELECT * FROM ENCARGADOS WHERE Profesor = @Profe";
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@Profe", HttpContext.Session.GetInt32("IdUsuario"));
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                HttpContext.Session.SetString("Materia", dr["Mat"].ToString());
                                List<MateriaModel> lista = Codigos();
                                ViewData["Datos"] = datos;
                                ViewData["Materias"] = lista;
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
            } catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("MenuProfe");
            }
        }
        public List<MateriaModel> Codigos()
        {
            List<MateriaModel> lista = new List<MateriaModel>();
            string qry = "SELECT Materia, llave_al from UNI_APRE inner join ENCARGADOS on ID_materia = Mat where Profesor = @profe";
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(qry, conn))
                {
                    cmd.Parameters.AddWithValue("@profe", HttpContext.Session.GetInt32("IdUsuario"));
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            MateriaModel materia = new MateriaModel
                            {
                                Nombre = dr["Materia"].ToString(),
                                llave = dr["llave_al"].ToString()
                            };
                            lista.Add(materia);
                        }
                    }
                    conn.Close();
                }
                return lista;
            }
            
        }
        [HttpPost]
        public IActionResult Añadir(DatosModel datos, string grupo)
        {
            try
            {
                int id = (int)HttpContext.Session.GetInt32("IdUsuario");
                string query = "SELECT * FROM ENCARGADOS WHERE Profesor = " + id;
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                conn.Close();
                                query = "INSERT INTO ENCARGADOS VALUES (@Profesor,@Materia)";
                                using SqlConnection cn = new SqlConnection(cadenaCon);
                                {
                                    using SqlCommand command = new SqlCommand(query, cn);
                                    {
                                        command.Parameters.AddWithValue("@Profesor", id);
                                        command.Parameters.AddWithValue("@Materia", grupo);
                                        cn.Open();
                                        command.ExecuteNonQuery();
                                        cn.Close();
                                    }
                                    TempData["Mensaje"] = "Clase añadida correctamente";
                                }
                                datos.IdUsuario = id;
                                HttpContext.Session.SetInt32("IdUsuario", datos.IdUsuario);
                                return RedirectToAction("MenuProfe", datos);
                            }
                            else
                            {
                                TempData["Error"] = "No se ha podido añadir la clase";
                                TempData["ID_usuario"] = datos.IdUsuario;
                                return RedirectToAction("MenuProfe", datos);
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("MenuProfe", datos);
            }
        }
        //------------------------------------Para Validar--------------------------------------------
        [HttpGet]
        public IActionResult Unirse(DatosModel datos)
        {
            try
            {
				TempData["ID_usuario"] = datos.IdUsuario;
				return View(datos);
			}
			catch (Exception ex)
			{
				TempData["Error"] = ex.ToString();
				return RedirectToAction("Unirse");
			}
		}
        [HttpPost]
        public IActionResult Validar(DatosModel datos)
        {
            int id = (int)HttpContext.Session.GetInt32("IdUsuario");
            try
            {
                string query = "UPDATE ENCARGADOS SET Profesor = @Profe WHERE Mat = @Grupo and Profesor IS NULL";
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Profe", id);
                        cmd.Parameters.AddWithValue("@Grupo", datos.grupo);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                HttpContext.Session.SetInt32("IdUsuario", id);
                HttpContext.Session.SetString("Grupo", datos.grupo);
                return RedirectToAction("MenuProfe", datos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Parece que ingresaste un código erróneo, vuelve a intentarlo";
                HttpContext.Session.SetInt32("IdUsuario", id);
                return RedirectToAction("MenuProfe", datos);
            }
        }

        //------------------------------------Para Cuestionario--------------------------------------------
        [HttpGet]
        public IActionResult Cuestionario()
        {
           try
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
                            new RespuestaModel(),
                            new RespuestaModel()
                        }
                    }
                }
                };

                string query = "SELECT Materia, Mat FROM UNI_APRE LEFT JOIN ENCARGADOS ON ID_materia = Mat WHERE Profesor = @Profesor";
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
                                items.Add(new SelectListItem { Value = dr["Mat"].ToString(), Text = dr["Materia"].ToString() });
                            }
                        }
                    }
                }

                ViewBag.Materias = items;

                return View(cuestionario);
            } catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("MenuProfe");
            }
        }
        [HttpPost]
        public IActionResult InsertarCuestionario(CuestionarioModel cuestionario, string Materia)
        {
            try
            {
                string materia = Materia;
                string idCues = GenerarIdCuestionario(materia);
                string queryCuestionario = "INSERT INTO CUESTIONARIO VALUES (@ID_cues, @Cuestionario, @Materia, @FecLim)";
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    using (SqlCommand cmd = new SqlCommand(queryCuestionario, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID_cues", idCues);
                        cmd.Parameters.AddWithValue("@Cuestionario", cuestionario.NombreCuestionario);
                        cmd.Parameters.AddWithValue("@Materia", materia);
                        cmd.Parameters.AddWithValue("@FecLim", cuestionario.FechaLim);
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
            } catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("Cuestionario");
            }
        }
        private string GenerarIdCuestionario(string materia)
        {
            string idCues;
            string queryGetLastId = "SELECT TOP 1 ID_cues FROM CUESTIONARIO where ID_cues like @ID ORDER BY ID_cues DESC";

            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(queryGetLastId, conn))
                {
                   cmd.Parameters.AddWithValue("@ID", materia + "C%");
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

        //------------------------------------Para Reportes--------------------------------------------
        public IActionResult ReporteClase()
        {
            int id = (int)HttpContext.Session.GetInt32("IdUsuario");
            List<string> Cues = new List<string>();
            List<string> NomMat = new List<string>();
            List<int> PromCues = new List<int>();
            List<double> PromCuesSum = new List<double>();
            List<int> NPreg = new List<int>();
            List<double> NPregSum = new List<double>();
            List<double> Prm = new List<double>();
            string query = "select c.ID_cues from CUESTIONARIO c join ENCARGADOS e on c.Materia = e.Mat  where e.Profesor = @id";
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
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
                query = "select u.Materia from UNI_APRE u join ENCARGADOS e on u.ID_materia = e.Mat where e.Profesor = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            NomMat.Add(dr["Materia"].ToString());
                        }
                    }
                    conn.Close();
                }
                int i = 0;
                foreach (string Mat in NomMat)
                {
                    foreach (string cues in Cues)
                    {
                        query = "select avg(p.Puntaje) as PuntajeProm from Progres_Estu p join ENCARGADOS e on p.uni_ap = e.Mat where e.Profesor = @id and p.Cuestionario = @cues";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@cues", cues);
                            conn.Open();
                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    if (!dr.IsDBNull(dr.GetOrdinal("PuntajeProm")))
                                    {
                                        PromCues.Add((int)dr["PuntajeProm"]);
                                    }
                                }
                            }
                            conn.Close();
                        }
                        query = "select COUNT(ID_pregunta) as NPreg from PREGUNTA where Cuestionario = @cues";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@cues", cues);
                            conn.Open();
                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    NPreg.Add((int)dr["NPreg"]);
                                }
                            }
                            conn.Close();
                        }
                    }
                    PromCuesSum.Add(PromCues.Sum());
                    NPregSum.Add(NPreg.Sum());
                    Prm.Add(PromCuesSum[i] / NPregSum[i] * 100);
                    i++;
                }
            }
            ViewBag.Nom = NomMat;
            ViewBag.Prom = Prm;
            return View();
        }
    }
}