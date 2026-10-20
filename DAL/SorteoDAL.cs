using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class SorteoDAL
    {
        private string conexion = Conexion.Cadena;

        // Busca el sorteo de un juego en una fecha. Loteria y turno van en null para los juegos
        // que no los usan. Devuelve null si ese sorteo todavia no existe.
        public Sorteo Buscar(int idJuego, int? idLoteria, int? idTurno, DateTime fecha)
        {
            string q = "SELECT " + Mapeo.ColumnasSorteo + " FROM SORTEOS s " + Mapeo.JoinsSorteo + @"
                        WHERE s.IdJuego = @juego AND s.Fecha = @fecha
                          AND ((s.IdLoteria IS NULL AND @loteria IS NULL) OR s.IdLoteria = @loteria)
                          AND ((s.IdTurno IS NULL AND @turno IS NULL) OR s.IdTurno = @turno)";
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.Add("@juego", SqlDbType.Int).Value = idJuego;
                cmd.Parameters.Add("@fecha", SqlDbType.Date).Value = fecha.Date;
                cmd.Parameters.Add("@loteria", SqlDbType.Int).Value = (object)idLoteria ?? DBNull.Value;
                cmd.Parameters.Add("@turno", SqlDbType.Int).Value = (object)idTurno ?? DBNull.Value;
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    return dr.Read() ? Mapeo.LeerSorteo(dr) : null;
                }
            }
        }

        public int Insertar(Sorteo s)
        {
            string q = @"INSERT INTO SORTEOS (IdJuego, IdLoteria, IdTurno, Fecha, Estado)
                         VALUES (@juego, @loteria, @turno, @fecha, @estado);
                         SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.Add("@juego", SqlDbType.Int).Value = s.Juego.Id;
                cmd.Parameters.Add("@loteria", SqlDbType.Int).Value = s.Loteria != null ? (object)s.Loteria.Id : DBNull.Value;
                cmd.Parameters.Add("@turno", SqlDbType.Int).Value = s.Turno != null ? (object)s.Turno.Id : DBNull.Value;
                cmd.Parameters.Add("@fecha", SqlDbType.Date).Value = s.Fecha.Date;
                cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value = s.Estado;
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void CambiarEstado(int idSorteo, string estado)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("UPDATE SORTEOS SET Estado = @estado WHERE Id = @id", con);
                cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value = estado;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idSorteo;
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Guarda los numeros que salieron y deja el sorteo en el estado final, todo en una sola
        // transaccion. Devuelve false si el sorteo ya estaba en ese estado (por ejemplo, si
        // otra terminal lo realizo antes).
        public bool GuardarResultado(int idSorteo, List<NumeroGanador> numeros, string estadoFinal)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    SqlCommand upd = new SqlCommand(
                        "UPDATE SORTEOS SET Estado = @estado WHERE Id = @id AND Estado <> @estado", con, tx);
                    upd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value = estadoFinal;
                    upd.Parameters.Add("@id", SqlDbType.Int).Value = idSorteo;
                    if (upd.ExecuteNonQuery() != 1)
                    {
                        tx.Rollback();
                        return false;
                    }

                    foreach (NumeroGanador n in numeros)
                    {
                        SqlCommand ins = new SqlCommand(
                            "INSERT INTO NUMERO_GANADOR (IdSorteo, Numero, Posicion) VALUES (@id, @numero, @posicion)", con, tx);
                        ins.Parameters.Add("@id", SqlDbType.Int).Value = idSorteo;
                        ins.Parameters.Add("@numero", SqlDbType.VarChar, 4).Value = n.Numero;
                        ins.Parameters.Add("@posicion", SqlDbType.Int).Value = n.Posicion;
                        ins.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return true;
                }
            }
        }

        public List<NumeroGanador> ObtenerNumerosGanadores(int idSorteo)
        {
            List<NumeroGanador> lista = new List<NumeroGanador>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT Numero, Posicion FROM NUMERO_GANADOR WHERE IdSorteo = @id ORDER BY Posicion", con);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idSorteo;
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        lista.Add(new NumeroGanador { Numero = (string)dr["Numero"], Posicion = (int)dr["Posicion"] });
                }
            }
            return lista;
        }
    }
}
