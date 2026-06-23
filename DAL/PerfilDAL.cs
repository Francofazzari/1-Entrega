using BE;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

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
                string q = "SELECT Id, Codigo, Nombre, Descripcion, EsPadre FROM PERMISOS";
                SqlCommand cmd = new SqlCommand(q, con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    bool esPadre = dr["EsPadre"] != DBNull.Value && (bool)dr["EsPadre"];
                    Permiso p;
                    if (!esPadre)
                        p = new PermisoSimple();
                    else
                        p = new PermisoCompleto();

                    p.Id = (int)dr["Id"];
                    p.Codigo = dr["Codigo"].ToString();
                    p.Nombre = dr["Nombre"].ToString();
                    p.Descripcion = dr["Descripcion"].ToString();
                    p.EsPadre = esPadre;
                    lista.Add(p);
                }
            }
            return lista;
        }

        public Permiso ObtenerArbol(int permisoRaizId)
        {
            List<Permiso> todos = ObtenerTodos();
            return ConstruirNodo(permisoRaizId, todos);
        }

        private Permiso ConstruirNodo(int id, List<Permiso> todos)
        {
            Permiso raiz = todos.FirstOrDefault(p => p.Id == id);
            if (raiz == null) return null;

            if (!raiz.EsPadre) return raiz; // Es patente, no tiene hijos

            PermisoCompleto compuesto = (PermisoCompleto)raiz;

            // Busca los hijos en PERMISO_PERMISO
            List<int> idsHijos = ObtenerIdsHijos(id);

            foreach (int idHijo in idsHijos)
            {
                Permiso hijo = ConstruirNodo(idHijo, todos);
                if (hijo != null)
                {
                    compuesto.Agregar(hijo);
                }
            }
            return compuesto;
        }

        private List<int> ObtenerIdsHijos(int idPadre)
        {
            List<int> hijos = new List<int>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = "SELECT IdHijo FROM PERMISO_PERMISO WHERE IdPadre = @padre";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@padre", idPadre);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    hijos.Add((int)dr["IdHijo"]);
                }
            }
            return hijos;
        }

        public int Insertar(Permiso p)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"INSERT INTO PERMISOS (Codigo, Nombre, Descripcion, EsPadre)
                             VALUES (@cod, @nom, @des, @espadre);
                             SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@cod", p.Codigo);
                cmd.Parameters.AddWithValue("@nom", p.Nombre);
                cmd.Parameters.AddWithValue("@des", p.Descripcion ?? "");
                cmd.Parameters.AddWithValue("@espadre", p.EsPadre);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool InsertarPermisoPermiso(int idPadre, int idHijo)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"INSERT INTO PERMISO_PERMISO (IdPadre, IdHijo) VALUES (@padre, @hijo)";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@padre", idPadre);
                cmd.Parameters.AddWithValue("@hijo", idHijo);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool LimpiarHijos(int idPadre)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"DELETE FROM PERMISO_PERMISO WHERE IdPadre = @padre";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@padre", idPadre);
                con.Open();
                return cmd.ExecuteNonQuery() >= 0;
            }
        }

        public bool Eliminar(int id)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                // Primero eliminar referencias en USUARIO_PERMISO y PERMISO_PERMISO
                SqlCommand cmdDel1 = new SqlCommand("DELETE FROM USUARIO_PERMISO WHERE IdPermiso=@id", con);
                cmdDel1.Parameters.AddWithValue("@id", id);
                
                SqlCommand cmdDel2 = new SqlCommand("DELETE FROM PERMISO_PERMISO WHERE IdPadre=@id OR IdHijo=@id", con);
                cmdDel2.Parameters.AddWithValue("@id", id);
                
                SqlCommand cmd = new SqlCommand("DELETE FROM PERMISOS WHERE Id=@id", con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    cmdDel1.Transaction = tx;
                    cmdDel2.Transaction = tx;
                    cmd.Transaction = tx;
                    cmdDel1.ExecuteNonQuery();
                    cmdDel2.ExecuteNonQuery();
                    bool res = cmd.ExecuteNonQuery() > 0;
                    tx.Commit();
                    return res;
                }
                catch
                {
                    tx.Rollback();
                    return false;
                }
            }
        }
    }
}

