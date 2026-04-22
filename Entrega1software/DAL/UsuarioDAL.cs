using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public class UsuarioDAL
    {
        private string conexion = "Server=.;Database=AgenciaQuiniela;Integrated Security=True;";
        public Usuario ObtenerPorTerminal(string nroTerminal)
        {
            Usuario u = null;
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"SELECT u.Id, u.NroTerminal, u.Clave, u.Nombre, 
                                    u.Apellido, u.PerfilId, p.Nombre AS Perfil, u.Activo
                             FROM USUARIOS u
                             INNER JOIN PERFILES p ON u.PerfilId = p.Id
                             WHERE u.NroTerminal = @nro AND u.Activo = 1";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@nro", nroTerminal);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    u = new Usuario
                    {
                        Id = (int)dr["Id"],
                        NroTerminal = dr["NroTerminal"].ToString(),
                        Clave = dr["Clave"].ToString(),
                        Nombre = dr["Nombre"].ToString(),
                        Apellido = dr["Apellido"].ToString(),
                        PerfilId = (int)dr["PerfilId"],
                        Perfil = dr["Perfil"].ToString(),
                        Activo = (bool)dr["Activo"]
                    };
                }
            }
            return u;
        }

        public List<Usuario> ObtenerTodos()
        {
            List<Usuario> lista = new List<Usuario>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"SELECT u.Id, u.NroTerminal, u.Nombre, u.Apellido, 
                                    u.PerfilId, p.Nombre AS Perfil, u.Activo
                             FROM USUARIOS u
                             INNER JOIN PERFILES p ON u.PerfilId = p.Id";
                SqlCommand cmd = new SqlCommand(q, con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new Usuario
                    {
                        Id = (int)dr["Id"],
                        NroTerminal = dr["NroTerminal"].ToString(),
                        Nombre = dr["Nombre"].ToString(),
                        Apellido = dr["Apellido"].ToString(),
                        PerfilId = (int)dr["PerfilId"],
                        Perfil = dr["Perfil"].ToString(),
                        Activo = (bool)dr["Activo"]
                    });
                }
            }
            return lista;
        }

        public bool Insertar(Usuario u)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"INSERT INTO USUARIOS (NroTerminal, Clave, Nombre, Apellido, PerfilId, Activo)
                             VALUES (@nro, @clave, @nom, @ape, @per, 1)";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@nro", u.NroTerminal);
                cmd.Parameters.AddWithValue("@clave", u.Clave);
                cmd.Parameters.AddWithValue("@nom", u.Nombre);
                cmd.Parameters.AddWithValue("@ape", u.Apellido);
                cmd.Parameters.AddWithValue("@per", u.PerfilId);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Modificar(Usuario u)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"UPDATE USUARIOS 
                             SET NroTerminal=@nro, Clave=@clave, Nombre=@nom, 
                                 Apellido=@ape, PerfilId=@per
                             WHERE Id=@id";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@nro", u.NroTerminal);
                cmd.Parameters.AddWithValue("@clave", u.Clave);
                cmd.Parameters.AddWithValue("@nom", u.Nombre);
                cmd.Parameters.AddWithValue("@ape", u.Apellido);
                cmd.Parameters.AddWithValue("@per", u.PerfilId);
                cmd.Parameters.AddWithValue("@id", u.Id);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Eliminar(int id)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(
                    "UPDATE USUARIOS SET Activo=0 WHERE Id=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public void RegistrarAuditoria(int usuarioId, string accion)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"INSERT INTO AUDITORIA_LOGIN (UsuarioId, FechaHora, Accion)
                             VALUES (@uid, GETDATE(), @acc)";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@uid", usuarioId);
                cmd.Parameters.AddWithValue("@acc", accion);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
