using System.Collections.Generic;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class LoteriaDAL
    {
        private string conexion = "Server=.;Database=AgenciaQuiniela;Integrated Security=True;";

        public List<Loteria> ObtenerActivas()
        {
            List<Loteria> lista = new List<Loteria>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = "SELECT Id, Nombre, Codigo, Activa FROM LOTERIAS WHERE Activa = 1 ORDER BY Nombre";
                SqlCommand cmd = new SqlCommand(q, con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new Loteria
                    {
                        Id = (int)dr["Id"],
                        Nombre = dr["Nombre"].ToString(),
                        Codigo = dr["Codigo"].ToString(),
                        Activa = (bool)dr["Activa"]
                    });
                }
            }
            return lista;
        }
    }
}
