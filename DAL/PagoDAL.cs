using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class PagoDAL
    {
        private string conexion = Conexion.Cadena;

        // Registra el pago y deja el ticket como pagado, en una sola transaccion. Devuelve false
        // si el ticket ya no estaba en el estado esperado (por ejemplo, si ya se habia pagado).
        public bool Registrar(Pago p, string estadoPagado, string estadoEsperado)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    SqlCommand upd = new SqlCommand(
                        "UPDATE APUESTAS SET Estado = @pagado WHERE Id = @id AND Estado = @esperado", con, tx);
                    upd.Parameters.Add("@pagado", SqlDbType.VarChar, 20).Value = estadoPagado;
                    upd.Parameters.Add("@id", SqlDbType.Int).Value = p.IdApuesta;
                    upd.Parameters.Add("@esperado", SqlDbType.VarChar, 20).Value = estadoEsperado;
                    if (upd.ExecuteNonQuery() != 1)
                    {
                        tx.Rollback();
                        return false;
                    }

                    SqlCommand ins = new SqlCommand(
                        @"INSERT INTO PAGOS (IdApuesta, UsuarioId, FechaHora, Monto)
                          VALUES (@apuesta, @usuario, @fecha, @monto);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx);
                    ins.Parameters.Add("@apuesta", SqlDbType.Int).Value = p.IdApuesta;
                    ins.Parameters.Add("@usuario", SqlDbType.Int).Value = p.UsuarioId;
                    ins.Parameters.Add("@fecha", SqlDbType.DateTime).Value = p.FechaHora;
                    ins.Parameters.AddWithValue("@monto", p.Monto);
                    p.Id = (int)ins.ExecuteScalar();

                    tx.Commit();
                    return true;
                }
            }
        }

        // Pagos realizados entre dos fechas (hasta es exclusivo), con el ticket y su juego.
        public List<Pago> Listar(DateTime desde, DateTime hastaExclusivo)
        {
            List<Pago> lista = new List<Pago>();
            string q = @"SELECT p.Id, p.IdApuesta, a.CodigoTicket, j.Nombre AS JuegoNombre, p.UsuarioId,
                                " + Mapeo.NombreUsuario + @" AS UsuarioNombre, p.FechaHora, p.Monto
                         FROM PAGOS p
                         JOIN APUESTAS a ON a.Id = p.IdApuesta
                         JOIN USUARIOS u ON u.Id = p.UsuarioId
                         CROSS APPLY (SELECT TOP 1 s.IdJuego
                                      FROM JUGADAS jg JOIN SORTEOS s ON s.Id = jg.IdSorteo
                                      WHERE jg.IdApuesta = a.Id ORDER BY jg.Orden) x
                         JOIN JUEGOS j ON j.Id = x.IdJuego
                         WHERE p.FechaHora >= @desde AND p.FechaHora < @hasta
                         ORDER BY p.FechaHora DESC";
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.Add("@desde", SqlDbType.DateTime).Value = desde;
                cmd.Parameters.Add("@hasta", SqlDbType.DateTime).Value = hastaExclusivo;
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Pago
                        {
                            Id = (int)dr["Id"],
                            IdApuesta = (int)dr["IdApuesta"],
                            CodigoTicket = (Guid)dr["CodigoTicket"],
                            JuegoNombre = (string)dr["JuegoNombre"],
                            UsuarioId = (int)dr["UsuarioId"],
                            UsuarioNombre = (string)dr["UsuarioNombre"],
                            FechaHora = (DateTime)dr["FechaHora"],
                            Monto = (decimal)dr["Monto"]
                        });
                    }
                }
            }
            return lista;
        }
    }
}
