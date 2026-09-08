using System.Collections.Generic;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class TurnoDAL
    {
        private string conexion = "Server=.;Database=AgenciaQuiniela;Integrated Security=True;";

        public List<Turno> ObtenerActivos()
        {
            List<Turno> lista = new List<Turno>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                // Se ordena por Id (no por Nombre) para respetar el orden cronologico
                // real de los sorteos: Primera, Matutina, Vespertina, Nocturna.
                string q = "SELECT Id, Nombre, Activo FROM TURNOS WHERE Activo = 1 ORDER BY Id";
                SqlCommand cmd = new SqlCommand(q, con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new Turno
                    {
                        Id = (int)dr["Id"],
                        Nombre = dr["Nombre"].ToString(),
                        Activo = (bool)dr["Activo"]
                    });
                }
            }
            return lista;
        }
    }
}
