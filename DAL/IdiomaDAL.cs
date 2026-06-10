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
    }
}