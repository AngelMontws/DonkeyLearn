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
        [HttpGet]
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
                            string grupo = dr.GetString(0);
                            HttpContext.Session.SetString("Grupo", grupo);
                            ViewBag.Grupo = grupo;
                            datos.grupo = grupo;
                            HttpContext.Session.SetString("Grupo", grupo);
                            List<MateriaModel> materiaList = ObtenerMaterias(grupo);
                            return View(materiaList);
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

        public List<MateriaModel> ObtenerMaterias(string grupo)
        {
            List<MateriaModel> materiaList = new List<MateriaModel>();
            string query = "SELECT * FROM UNI_APRE WHERE Grupo = @Grupo";
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
                            MateriaModel materia = new MateriaModel
                            {
                                ID = dr.GetString("ID_materia"),
                                Nombre = dr.GetString("Materia"),
                                Grupo = grupo,
                            };
                            materiaList.Add(materia);
                        }
                    }
                }
            }
            return materiaList;
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
        [HttpPost]
        public IActionResult GuardarCambios(List<MateriaModel> materiaList)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaCon))
                {
                    conn.Open();
                    var grupo = HttpContext.Session.GetString("Grupo");
                    foreach (var materia in materiaList)
                    {
                        var id = materia.ID;
                        var nombre = materia.Nombre;
                        materia.Grupo = grupo;
                        if (string.IsNullOrEmpty(materia.ID))  // Nuevo registro
                        {
                            //Aqui da error
                            string insertQuery = "INSERT INTO UNI_APRE (ID_materia, Materia, Grupo) VALUES (@ID_materia, @Materia, @Grupo)";
                            using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@ID_materia", materia.ID);
                                cmd.Parameters.AddWithValue("@Materia", materia.Nombre);
                                cmd.Parameters.AddWithValue("@Grupo", grupo);  // Usar el valor del parámetro
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else  // Registro existente
                        {
                            string updateQuery = "UPDATE UNI_APRE SET Materia = @Materia, ID_materia = @ID_materia WHERE Grupo = @Grupo";
                            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@ID_materia", materia.ID);
                                cmd.Parameters.AddWithValue("@Materia", materia.Nombre);
                                cmd.Parameters.AddWithValue("@Grupo", grupo);  // Usar el valor del parámetro
                                cmd.ExecuteNonQuery();
                            }
                        }
                        TempData["Mensaje"] = "Cambios guardados correctamente";
                    }
                }

                return RedirectToAction("MenuAdmin");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("MenuAdmin");
            }
        }

    }
}
