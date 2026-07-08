using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class UsuarioDAL
    {
        private string conexion = "Server=.;Database=AgenciaQuiniela;Integrated Security=True;";

        public Usuario Login(string nroTerminal, string claveHasheada)
        {
            Usuario u = null;
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = "SELECT Id, NroTerminal, Clave, Nombre, Apellido, Activo FROM USUARIOS WHERE NroTerminal = @nt AND Clave = @cl AND Activo = 1";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@nt", nroTerminal);
                cmd.Parameters.AddWithValue("@cl", claveHasheada);

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
            return u;
        }

        public List<Usuario> ObtenerTodos()
        {
            List<Usuario> lista = new List<Usuario>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = "SELECT Id, NroTerminal, Clave, Nombre, Apellido, Activo FROM USUARIOS";
                SqlCommand cmd = new SqlCommand(q, con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new Usuario
                    {
                        Id = (int)dr["Id"],
                        NroTerminal = dr["NroTerminal"].ToString(),
                        Clave = dr["Clave"].ToString(),
                        Nombre = dr["Nombre"].ToString(),
                        Apellido = dr["Apellido"].ToString(),
                        Activo = (bool)dr["Activo"]
                    });
                }
            }
            return lista;
        }

        private void GuardarHistorial(Usuario u, string operacion, string responsable, SqlConnection con, SqlTransaction tx)
        {
            // Obtener roles actuales del usuario como un string separado por comas
            string permisosIds = "";
            string qPermisos = "SELECT IdPermiso FROM USUARIO_PERMISO WHERE IdUsuario = @uid";
            using (SqlCommand cmdPerm = new SqlCommand(qPermisos, con, tx))
            {
                cmdPerm.Parameters.AddWithValue("@uid", u.Id);
                using (SqlDataReader dr = cmdPerm.ExecuteReader())
                {
                    List<string> ids = new List<string>();
                    while(dr.Read())
                    {
                        ids.Add(dr["IdPermiso"].ToString());
                    }
                    permisosIds = string.Join(",", ids);
                }
            }

            string q = @"INSERT INTO CAMBIOS_USUARIO (UsuarioId, FechaCambio, ResponsableNombre, Operacion,
                                                      NroTerminal, Clave, Nombre, Apellido, Activo, PermisosIds)
                         VALUES (@uid, GETDATE(), @resp, @oper, @nro, @clave, @nom, @ape, @act, @perms)";
            using (SqlCommand cmd = new SqlCommand(q, con, tx))
            {
                cmd.Parameters.AddWithValue("@uid", u.Id);
                cmd.Parameters.AddWithValue("@resp", responsable);
                cmd.Parameters.AddWithValue("@oper", operacion);
                cmd.Parameters.AddWithValue("@nro", u.NroTerminal);
                cmd.Parameters.AddWithValue("@clave", u.Clave);
                cmd.Parameters.AddWithValue("@nom", u.Nombre);
                cmd.Parameters.AddWithValue("@ape", u.Apellido);
                cmd.Parameters.AddWithValue("@act", u.Activo);
                cmd.Parameters.AddWithValue("@perms", permisosIds);
                cmd.ExecuteNonQuery();
            }
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
                    cmd.Parameters.AddWithValue("@ape", u.Apellido ?? "");
                    
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
                    string q = @"UPDATE USUARIOS 
                                 SET NroTerminal=@nro, Clave=@clave, Nombre=@nom, Apellido=@ape, Activo=@act 
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
                                    NroTerminal, Clave, Nombre, Apellido, Activo, PermisosIds
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
                        Activo = (bool)dr["Activo"],
                        PermisosIds = dr["PermisosIds"] != DBNull.Value ? dr["PermisosIds"].ToString() : ""
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
                    string permisosIds = "";
                    if(dr.Read())
                    {
                        u.Id = (int)dr["UsuarioId"];
                        u.NroTerminal = dr["NroTerminal"].ToString();
                        u.Clave = dr["Clave"].ToString();
                        u.Nombre = dr["Nombre"].ToString();
                        u.Apellido = dr["Apellido"].ToString();
                        u.Activo = (bool)dr["Activo"];
                        permisosIds = dr["PermisosIds"] != DBNull.Value ? dr["PermisosIds"].ToString() : "";
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

                    // Restaurar roles
                    SqlCommand cmdLimpiar = new SqlCommand("DELETE FROM USUARIO_PERMISO WHERE IdUsuario = @uid", con, tx);
                    cmdLimpiar.Parameters.AddWithValue("@uid", u.Id);
                    cmdLimpiar.ExecuteNonQuery();

                    if (!string.IsNullOrEmpty(permisosIds))
                    {
                        string[] ids = permisosIds.Split(',');
                        foreach(string idRol in ids)
                        {
                            if (int.TryParse(idRol, out int idRolInt))
                            {
                                SqlCommand cmdAsignar = new SqlCommand("INSERT INTO USUARIO_PERMISO (IdUsuario, IdPermiso) VALUES (@uid, @pid)", con, tx);
                                cmdAsignar.Parameters.AddWithValue("@uid", u.Id);
                                cmdAsignar.Parameters.AddWithValue("@pid", idRolInt);
                                cmdAsignar.ExecuteNonQuery();
                            }
                        }
                    }

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
