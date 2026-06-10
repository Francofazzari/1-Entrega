using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class BitacoraDAL
    {
        
        private string conexion = "Server=.;Database=AgenciaQuiniela;Integrated Security=True;";

        public void Registrar(Bitacora b)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string q = @"INSERT INTO Bitacora (FechaHora, UsuarioId, UsuarioNombre, Actividad, Detalle) 
                             VALUES (@fecha, @uid, @unom, @act, @det)";
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@fecha", b.FechaHora);
                cmd.Parameters.AddWithValue("@uid", b.UsuarioId);
                cmd.Parameters.AddWithValue("@unom", b.UsuarioNombre ?? "");
                cmd.Parameters.AddWithValue("@act", b.Actividad);
                cmd.Parameters.AddWithValue("@det", b.Detalle ?? "");

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Bitacora> Listar(DateTime desde, DateTime hasta, string texto)
        {
            List<Bitacora> lista = new List<Bitacora>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                // Busco por fechas y texto en cualquier columna de texto
                string q = @"SELECT Id, FechaHora, UsuarioId, UsuarioNombre, Actividad, Detalle 
                             FROM Bitacora 
                             WHERE FechaHora BETWEEN @desde AND @hasta 
                             AND (Actividad LIKE @texto OR Detalle LIKE @texto OR UsuarioNombre LIKE @texto)
                             ORDER BY FechaHora DESC";

                SqlCommand cmd = new SqlCommand(q, con);
                // Busco desde el inicio del primer día hasta el final del último día
                cmd.Parameters.AddWithValue("@desde", desde.Date);
                cmd.Parameters.AddWithValue("@hasta", hasta.Date.AddDays(1).AddTicks(-1));
                cmd.Parameters.AddWithValue("@texto", "%" + texto + "%");

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new Bitacora
                    {
                        Id = (int)dr["Id"],
                        FechaHora = (DateTime)dr["FechaHora"],
                        UsuarioId = (int)dr["UsuarioId"],
                        UsuarioNombre = dr["UsuarioNombre"].ToString(),
                        Actividad = dr["Actividad"].ToString(),
                        Detalle = dr["Detalle"].ToString()
                    });
                }
            }
            return lista;
        }
    }
}