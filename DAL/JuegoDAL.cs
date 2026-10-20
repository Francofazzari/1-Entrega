using System.Collections.Generic;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class JuegoDAL
    {
        private string conexion = Conexion.Cadena;

        public List<Juego> ObtenerActivos()
        {
            List<Juego> lista = new List<Juego>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = "SELECT " + Mapeo.ColumnasJuego + " FROM JUEGOS j WHERE j.Activo = 1 ORDER BY j.Id";
                SqlCommand cmd = new SqlCommand(q, con);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        lista.Add(Mapeo.LeerJuego(dr));
                }
            }
            return lista;
        }
    }
}
