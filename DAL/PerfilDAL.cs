using BE;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class PerfilDAL
    {
        private string conexion = "Server=.;Database=AgenciaQuiniela;Integrated Security=True;";

        public List<Permiso> ObtenerTodos()
        {
            List<Permiso> lista = new List<Permiso>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = "SELECT Id, Codigo, Nombre, Descripcion, Tipo, PermisoPadreId FROM PERMISOS ORDER BY PermisoPadreId, Id";
                SqlCommand cmd = new SqlCommand(q, con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    string tipo = dr["Tipo"].ToString();
                    Permiso p;
                    if (tipo == "A")
                        p = new PermisoSimple();
                    else
                        p = new PermisoCompleto();

                    p.Id = (int)dr["Id"];
                    p.Codigo = dr["Codigo"].ToString();
                    p.Nombre = dr["Nombre"].ToString();
                    p.Descripcion = dr["Descripcion"].ToString();
                    p.PermisoPadreId = dr["PermisoPadreId"] == DBNull.Value ? (int?)null : (int)dr["PermisoPadreId"];
                    lista.Add(p);

                }
            }
            return lista;
        }

        public List<Perfil> ObtenerPerfiles()
        {
            List<Perfil> lista = new List<Perfil>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT Id, Nombre, Descripcion, PermisoRaizId FROM PERFILES", con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new Perfil
                    {
                        Id = (int)dr["Id"],
                        Nombre = dr["Nombre"].ToString(),
                        Descripcion = dr["Descripcion"].ToString(),
                        PermisoRaizId = dr["PermisoRaizId"] == DBNull.Value ? 0 : (int)dr["PermisoRaizId"]
                    });
                }
            }
            return lista;
        }

        public bool Insertar(Permiso p, int? padreId)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string tipo = p is PermisoSimple ? "A" : "C";
                string q = @"INSERT INTO PERMISOS (Codigo, Nombre, Descripcion, Tipo, PermisoPadreId)
                             VALUES (@cod, @nom, @des, @tip, @pad)";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@cod", p.Codigo);
                cmd.Parameters.AddWithValue("@nom", p.Nombre);
                cmd.Parameters.AddWithValue("@des", p.Descripcion ?? "");
                cmd.Parameters.AddWithValue("@tip", tipo);
                cmd.Parameters.AddWithValue("@pad", padreId.HasValue ? (object)padreId.Value : DBNull.Value);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Eliminar(int id)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM PERMISOS WHERE Id=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public int InsertarPerfilYObtenerID(string nombre, string descripcion)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"INSERT INTO PERFILES (Nombre, Descripcion)
                     VALUES (@nom, @des);
                     SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@nom", nombre);
                cmd.Parameters.AddWithValue("@des", descripcion ?? "");
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool InsertarPerfilPermiso(int perfilId, int permisoId)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO PERFIL_PERMISOS (PerfilId, PermisoId) VALUES (@per, @perm)", con);
                cmd.Parameters.AddWithValue("@per", perfilId);
                cmd.Parameters.AddWithValue("@perm", permisoId);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public int ObtenerIdPermisoPorNombre(string nombre)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT Id FROM PERMISOS WHERE Nombre = @nom", con);
                cmd.Parameters.AddWithValue("@nom", nombre);
                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        public List<string> ObtenerFuncionesDeUsuario(int usuarioId)
        {
            List<string> lista = new List<string>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"SELECT p.Nombre FROM PERMISOS p
                     INNER JOIN PERFIL_PERMISOS pp ON p.Id = pp.PermisoId
                     INNER JOIN PERFILES pf ON pp.PerfilId = pf.Id
                     INNER JOIN USUARIOS u ON u.PerfilId = pf.Id
                     WHERE u.Id = @uid";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@uid", usuarioId);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    lista.Add(dr["Nombre"].ToString());
            }
            return lista;
        }
        public Perfil ObtenerPerfilPorNombre(string nombre)
        {
            Perfil p = null;
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = "SELECT Id, Nombre, Descripcion, PermisoRaizId FROM PERFILES WHERE Nombre = @nom";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@nom", nombre);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    p = new Perfil
                    {
                        Id = (int)dr["Id"],
                        Nombre = dr["Nombre"].ToString(),
                        Descripcion = dr["Descripcion"].ToString(),
                        PermisoRaizId = dr["PermisoRaizId"] == DBNull.Value ? 0 : (int)dr["PermisoRaizId"]
                    };
                }
            }
            return p;
        }
    }
}

