using apiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data;

namespace apiWeb.Controllers
{
    public class AccesoController : Controller
    {

        static string cadena = "Data Source=DESKTOP-UESQ7QG\\MSSQLSERVER01;Initial Catalog=DB_USUARIOS;Integrated Security=true";
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario registUsuario)
        {
            bool registrado;
            string mensaje;

            if (registUsuario.Contrasenia == registUsuario.ConfirmarContra)
            {
                registUsuario.Contrasenia = ConvertirContra(registUsuario.Contrasenia);
            }
            else
            {
                ViewData["Mensaje"] = "Las claves no coinciden";
                return View();
            }

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_Registro",cn);
                cmd.Parameters.AddWithValue("Email", registUsuario.Email);
                cmd.Parameters.AddWithValue("Contrasenia", registUsuario.Contrasenia);
                cmd.Parameters.Add("registrado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("Mensaje", SqlDbType.VarChar,100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();

                cmd.ExecuteNonQuery();

                registrado = Convert.ToBoolean(cmd.Parameters["registrado"].Value);
                mensaje = cmd.Parameters["Mensaje"].Value.ToString();
            }

            ViewData["Mensaje"] = mensaje;

            if(registrado)
            {
                return View("Login", "Acceso");
            }
            else
            {
                return View();
            }

        }


        [HttpPost]
        public IActionResult Login(Usuario registUsuario)
        {
            registUsuario.Contrasenia = ConvertirContra(registUsuario.Contrasenia);

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_ValidarUsuario", cn);
                cmd.Parameters.AddWithValue("Email", registUsuario.Email);
                cmd.Parameters.AddWithValue("Contrasenia", registUsuario.Contrasenia);
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();

                registUsuario.IdUsuario = Convert.ToInt32(cmd.ExecuteScalar().ToString);

            }

            if(registUsuario.IdUsuario != 0)
            {
                //FALTA LA LINEA DE SESION
                RedirectToAction("Index", "Home");
            }else
            {
                ViewData["Mensaje"] = "Usuario no encontrado";
                return View();
            }

            return View();

        }






        public static string ConvertirContra(string texto)
        {
            StringBuilder sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));

                foreach (byte b in result)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
