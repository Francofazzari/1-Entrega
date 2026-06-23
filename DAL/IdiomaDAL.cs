using System.Collections.Generic;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class IdiomaDAL
    {
        private string conexion = "Server=.;Database=AgenciaQuiniela;Integrated Security=True;";

        public List<Idioma> ObtenerIdiomas()
        {
            List<Idioma> lista = new List<Idioma>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("SELECT Id, Nombre, Codigo FROM IDIOMAS", con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new Idioma
                    {
                        Id = (int)dr["Id"],
                        Nombre = dr["Nombre"].ToString(),
                        Codigo = dr["Codigo"].ToString()
                    });
                }
            }
            return lista;
        }

        public Dictionary<string, string> ObtenerTraducciones(int idiomaId)
        {
            Dictionary<string, string> traducciones = new Dictionary<string, string>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("SELECT Clave, Valor FROM TRADUCCIONES WHERE IdiomaId = @id", con);
                cmd.Parameters.AddWithValue("@id", idiomaId);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    traducciones.Add(dr["Clave"].ToString(), dr["Valor"].ToString());
                }
            }
            return traducciones;
        }

        public List<Traduccion> ObtenerTraduccionesGrid(int idiomaId)
        {
            List<Traduccion> traducciones = new List<Traduccion>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"
                    SELECT tAll.Clave, ISNULL(tId.Valor, '') as Valor
                    FROM (SELECT DISTINCT Clave FROM TRADUCCIONES) tAll
                    LEFT JOIN TRADUCCIONES tId ON tAll.Clave = tId.Clave AND tId.IdiomaId = @id
                    ORDER BY tAll.Clave";

                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@id", idiomaId);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    traducciones.Add(new Traduccion {
                        Clave = dr["Clave"].ToString(),
                        Valor = dr["Valor"].ToString()
                    });
                }
            }
            return traducciones;
        }

        public bool InsertarIdioma(Idioma idioma)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("INSERT INTO IDIOMAS (Nombre, Codigo) VALUES (@nom, @cod); SELECT SCOPE_IDENTITY();", con);
                cmd.Parameters.AddWithValue("@nom", idioma.Nombre);
                cmd.Parameters.AddWithValue("@cod", idioma.Codigo ?? "");
                con.Open();
                int newId = System.Convert.ToInt32(cmd.ExecuteScalar());
                idioma.Id = newId;
                return true;
            }
        }

        public bool RenombrarIdioma(int id, string nuevoNombre)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("UPDATE IDIOMAS SET Nombre = @nom WHERE Id = @id", con);
                cmd.Parameters.AddWithValue("@nom", nuevoNombre);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool EliminarIdioma(int id)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    SqlCommand cmd1 = new SqlCommand("DELETE FROM TRADUCCIONES WHERE IdiomaId = @id", con, tx);
                    cmd1.Parameters.AddWithValue("@id", id);
                    cmd1.ExecuteNonQuery();

                    SqlCommand cmd2 = new SqlCommand("DELETE FROM IDIOMAS WHERE Id = @id", con, tx);
                    cmd2.Parameters.AddWithValue("@id", id);
                    cmd2.ExecuteNonQuery();

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

        public bool GuardarTraducciones(int idiomaId, List<Traduccion> traducciones)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    SqlCommand cmdDel = new SqlCommand("DELETE FROM TRADUCCIONES WHERE IdiomaId = @id", con, tx);
                    cmdDel.Parameters.AddWithValue("@id", idiomaId);
                    cmdDel.ExecuteNonQuery();

                    foreach (var t in traducciones)
                    {
                        if (string.IsNullOrWhiteSpace(t.Valor)) continue; // Don't insert empty values if not filled
                        
                        SqlCommand cmdIns = new SqlCommand("INSERT INTO TRADUCCIONES (IdiomaId, Clave, Valor) VALUES (@id, @clave, @valor)", con, tx);
                        cmdIns.Parameters.AddWithValue("@id", idiomaId);
                        cmdIns.Parameters.AddWithValue("@clave", t.Clave);
                        cmdIns.Parameters.AddWithValue("@valor", t.Valor);
                        cmdIns.ExecuteNonQuery();
                    }

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
    }
}
