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
                                    u.Apellido, u.Activo
                             FROM USUARIOS u
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
                        Activo = (bool)dr["Activo"]
                    };
                }
            }
            if (u != null)
            {
                u.Permisos = ObtenerPermisosDeUsuario(u.Id);
            }
            return u;
        }

        public List<Permiso> ObtenerPermisosDeUsuario(int usuarioId)
        {
            PerfilDAL pDal = new PerfilDAL();
            List<Permiso> lista = new List<Permiso>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = "SELECT IdPermiso FROM USUARIO_PERMISO WHERE IdUsuario = @id";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@id", usuarioId);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    int permisoId = (int)dr["IdPermiso"];
                    Permiso p = pDal.ObtenerArbol(permisoId);
                    if (p != null) lista.Add(p);
                }
            }
            return lista;
        }

        public List<Usuario> ObtenerTodos()
        {
            List<Usuario> lista = new List<Usuario>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"SELECT u.Id, u.NroTerminal, u.Nombre, u.Apellido, u.Activo
                             FROM USUARIOS u";
                SqlCommand cmd = new SqlCommand(q, con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Usuario u = new Usuario
                    {
                        Id = (int)dr["Id"],
                        NroTerminal = dr["NroTerminal"].ToString(),
                        Nombre = dr["Nombre"].ToString(),
                        Apellido = dr["Apellido"].ToString(),
                        Activo = (bool)dr["Activo"]
                    };
                    lista.Add(u);
                }
            }
            foreach (var u in lista)
            {
                u.Permisos = ObtenerPermisosDeUsuario(u.Id);
            }
            return lista;
        }

        private void GuardarHistorial(Usuario u, string operacion, string responsableNombre, SqlConnection con, SqlTransaction tx = null)
        {
            string q = @"INSERT INTO CAMBIOS_USUARIO (UsuarioId, FechaCambio, ResponsableNombre, Operacion, NroTerminal, Clave, Nombre, Apellido, Activo)
                         VALUES (@uid, GETDATE(), @resp, @op, @nro, @clave, @nom, @ape, @act)";
            SqlCommand cmd = new SqlCommand(q, con, tx);
            cmd.Parameters.AddWithValue("@uid", u.Id);
            cmd.Parameters.AddWithValue("@resp", responsableNombre);
            cmd.Parameters.AddWithValue("@op", operacion);
            cmd.Parameters.AddWithValue("@nro", u.NroTerminal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clave", u.Clave ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@nom", u.Nombre ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ape", u.Apellido ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@act", u.Activo);
            cmd.ExecuteNonQuery();
        }

        public bool Insertar(Usuario u, string responsable)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    string q = @"INSERT INTO USUARIOS (NroTerminal, Clave, Nombre, Apellido, Activo)
                                 VALUES (@nro, @clave, @nom, @ape, 1);
                                 SELECT SCOPE_IDENTITY();";
                    SqlCommand cmd = new SqlCommand(q, con, tx);
                    cmd.Parameters.AddWithValue("@nro", u.NroTerminal);
                    cmd.Parameters.AddWithValue("@clave", u.Clave);
                    cmd.Parameters.AddWithValue("@nom", u.Nombre);
                    cmd.Parameters.AddWithValue("@ape", u.Apellido);
                    
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    u.Id = newId;
                    u.Activo = true;
                    
                    GuardarHistorial(u, "ALTA", responsable, con, tx);
                    
                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    return false;
                }
            }
        }

        public bool Modificar(Usuario u, string responsable)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    // Recuperar el estado actual de la BD (Clave antigua y Activo actual)
                    string qOld = "SELECT Clave, Activo FROM USUARIOS WHERE Id=@id";
                    SqlCommand cmdOld = new SqlCommand(qOld, con, tx);
                    cmdOld.Parameters.AddWithValue("@id", u.Id);
                    SqlDataReader dr = cmdOld.ExecuteReader();
                    if (dr.Read())
                    {
                        if (string.IsNullOrEmpty(u.Clave)) u.Clave = dr["Clave"].ToString();
                        u.Activo = (bool)dr["Activo"];
                    }
                    dr.Close();

                    string q = @"UPDATE USUARIOS 
                                 SET NroTerminal=@nro, Clave=@clave, Nombre=@nom, 
                                     Apellido=@ape, Activo=@act
                                 WHERE Id=@id";
                    SqlCommand cmd = new SqlCommand(q, con, tx);
                    cmd.Parameters.AddWithValue("@nro", u.NroTerminal);
                    cmd.Parameters.AddWithValue("@clave", u.Clave);
                    cmd.Parameters.AddWithValue("@nom", u.Nombre);
                    cmd.Parameters.AddWithValue("@ape", u.Apellido);
                    cmd.Parameters.AddWithValue("@act", u.Activo);
                    cmd.Parameters.AddWithValue("@id", u.Id);
                    cmd.ExecuteNonQuery();

                    GuardarHistorial(u, "MODIFICACION", responsable, con, tx);

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    return false;
                }
            }
        }

        public bool Eliminar(Usuario u, string responsable)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE USUARIOS SET Activo=0 WHERE Id=@id", con, tx);
                    cmd.Parameters.AddWithValue("@id", u.Id);
                    cmd.ExecuteNonQuery();

                    // Recuperar datos para el historial
                    string qOld = "SELECT NroTerminal, Clave, Nombre, Apellido FROM USUARIOS WHERE Id=@id";
                    SqlCommand cmdOld = new SqlCommand(qOld, con, tx);
                    cmdOld.Parameters.AddWithValue("@id", u.Id);
                    SqlDataReader dr = cmdOld.ExecuteReader();
                    if (dr.Read())
                    {
                        u.NroTerminal = dr["NroTerminal"].ToString();
                        u.Clave = dr["Clave"].ToString();
                        u.Nombre = dr["Nombre"].ToString();
                        u.Apellido = dr["Apellido"].ToString();
                    }
                    dr.Close();

                    u.Activo = false;
                    GuardarHistorial(u, "BAJA", responsable, con, tx);

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    return false;
                }
            }
        }

        public List<UsuarioHistorial> ObtenerHistorial(int usuarioId)
        {
            List<UsuarioHistorial> lista = new List<UsuarioHistorial>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"SELECT IdHistorial, UsuarioId, FechaCambio, ResponsableNombre, Operacion,
                                    NroTerminal, Clave, Nombre, Apellido, Activo
                             FROM CAMBIOS_USUARIO
                             WHERE UsuarioId = @uid
                             ORDER BY FechaCambio DESC";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@uid", usuarioId);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    UsuarioHistorial uh = new UsuarioHistorial
                    {
                        IdHistorial = (int)dr["IdHistorial"],
                        Id = (int)dr["UsuarioId"],
                        FechaCambio = (DateTime)dr["FechaCambio"],
                        ResponsableNombre = dr["ResponsableNombre"].ToString(),
                        Operacion = dr["Operacion"].ToString(),
                        NroTerminal = dr["NroTerminal"].ToString(),
                        Clave = dr["Clave"].ToString(),
                        Nombre = dr["Nombre"].ToString(),
                        Apellido = dr["Apellido"].ToString(),
                        Activo = (bool)dr["Activo"]
                    };
                    lista.Add(uh);
                }
            }
            return lista;
        }

        public bool RestaurarVersion(int idHistorial, string responsable)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    // Obtener la version historica
                    string q1 = "SELECT * FROM CAMBIOS_USUARIO WHERE IdHistorial = @id";
                    SqlCommand cmd1 = new SqlCommand(q1, con, tx);
                    cmd1.Parameters.AddWithValue("@id", idHistorial);
                    SqlDataReader dr = cmd1.ExecuteReader();
                    
                    Usuario u = new Usuario();
                    if(dr.Read())
                    {
                        u.Id = (int)dr["UsuarioId"];
                        u.NroTerminal = dr["NroTerminal"].ToString();
                        u.Clave = dr["Clave"].ToString();
                        u.Nombre = dr["Nombre"].ToString();
                        u.Apellido = dr["Apellido"].ToString();
                        u.Activo = (bool)dr["Activo"];
                    }
                    dr.Close();

                    // Sobrescribir en USUARIOS
                    string q2 = @"UPDATE USUARIOS 
                                 SET NroTerminal=@nro, Clave=@clave, Nombre=@nom, 
                                     Apellido=@ape, Activo=@act
                                 WHERE Id=@id";
                    SqlCommand cmd2 = new SqlCommand(q2, con, tx);
                    cmd2.Parameters.AddWithValue("@nro", u.NroTerminal);
                    cmd2.Parameters.AddWithValue("@clave", u.Clave);
                    cmd2.Parameters.AddWithValue("@nom", u.Nombre);
                    cmd2.Parameters.AddWithValue("@ape", u.Apellido);
                    cmd2.Parameters.AddWithValue("@act", u.Activo);
                    cmd2.Parameters.AddWithValue("@id", u.Id);
                    cmd2.ExecuteNonQuery();

                    GuardarHistorial(u, "RESTAURACION", responsable, con, tx);

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    return false;
                }
            }
        }

        public int InsertarYObtenerID(Usuario u)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"INSERT INTO USUARIOS (NroTerminal, Clave, Nombre, Apellido, Activo)
                     VALUES (@nro, @clave, @nom, @ape, 1);
                     SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@nro", u.NroTerminal);
                cmd.Parameters.AddWithValue("@clave", u.Clave);
                cmd.Parameters.AddWithValue("@nom", u.Nombre);
                cmd.Parameters.AddWithValue("@ape", u.Apellido ?? "");
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool AsignarPermiso(int usuarioId, int permisoId)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO USUARIO_PERMISO (IdUsuario, IdPermiso) VALUES (@idU, @idP)", con);
                cmd.Parameters.AddWithValue("@idU", usuarioId);
                cmd.Parameters.AddWithValue("@idP", permisoId);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool LimpiarPermisos(int usuarioId)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM USUARIO_PERMISO WHERE IdUsuario = @idU", con);
                cmd.Parameters.AddWithValue("@idU", usuarioId);
                con.Open();
                return cmd.ExecuteNonQuery() >= 0;
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
