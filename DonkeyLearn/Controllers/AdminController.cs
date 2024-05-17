using DonkeyLearn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

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
                HttpContext.Session.SetString("Grupo", datos.grupo);  // Establecer el valor de la sesión "Grupo" aquí
                return RedirectToAction("MenuAdmin", datos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("Grupo");
            }
        }
        [HttpGet]
        public IActionResult Materias(DatosModel datos)
        {
            List<MateriaModel> materias = ObtenerMaterias(grupo);
            ViewData["Materias"] = materias;
            ViewData["Datos"] = datos;
            return View(materias);
        }
        [HttpGet]
        public IActionResult Profes(DatosModel datos)
        {
            var profesores = GetProfesores();
            return View(profesores);
        }
        public List<ProfesorModel> GetProfesores()
        {
            List<ProfesorModel> profesores = new List<ProfesorModel>();
            string query = "SELECT Materia, Profesor, Nom_usuario, AP_PAT, AP_MAT, correo FROM ENCARGADOS RIGHT JOIN usuario ON Profesor = ID_usuario WHERE Tipo_usuario = 'Profesor'";
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            profesores.Add(new ProfesorModel
                            {
                                Materia = dr["Materia"].ToString(),
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

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Inicio", "Inicio");
        }
        public List<MateriaModel> ObtenerMaterias(string grupo)
        {
            grupo = HttpContext.Session.GetString("Grupo");  // Obtener el valor de la sesión "Grupo" aquí
            List<MateriaModel> materias = new List<MateriaModel>();
            string query = "SELECT ID_materia, Materia FROM UNI_APRE WHERE Grupo = @Grupo";
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
        public IActionResult EliminarMateria(string id, DatosModel datos)
        {
            
            string query = "DELETE FROM UNI_APRE WHERE ID_materia = @ID_materia";
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
            TempData["Mensaje"] = "Materia eliminada";
            datos.IdUsuario = HttpContext.Session.GetInt32("IdUsuario").GetValueOrDefault();
            return RedirectToAction("Materias", datos);
        }
        [HttpPost]
        public IActionResult CrearClase(string nombreClase, DatosModel datos)
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
            string queryInsert = "INSERT INTO UNI_APRE VALUES (@ID, @Materia, @Grupo)";
            using (SqlConnection conn = new SqlConnection(cadenaCon))
            {
                using (SqlCommand cmd = new SqlCommand(queryInsert, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@Materia", nombreClase);
                    cmd.Parameters.AddWithValue("@Grupo", grupo);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            TempData["Mensaje"] = "Clase creada";
            datos.IdUsuario = HttpContext.Session.GetInt32("IdUsuario").GetValueOrDefault();
            return RedirectToAction("Materias", datos);
        }

    }
}
