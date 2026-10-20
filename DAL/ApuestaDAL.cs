using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class ApuestaDAL
    {
        private string conexion = Conexion.Cadena;

        // Guarda el ticket completo (cabecera, jugadas y numeros) en una sola transaccion:
        // o se guarda todo, o no se guarda nada. Completa los Id generados.
        public void Insertar(Apuesta a)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO APUESTAS (CodigoTicket, UsuarioId, FechaHora, Total, Estado)
                          VALUES (@codigo, @usuario, @fecha, @total, @estado);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx);
                    cmd.Parameters.Add("@codigo", SqlDbType.UniqueIdentifier).Value = a.CodigoTicket;
                    cmd.Parameters.Add("@usuario", SqlDbType.Int).Value = a.UsuarioId;
                    cmd.Parameters.Add("@fecha", SqlDbType.DateTime).Value = a.FechaHora;
                    cmd.Parameters.AddWithValue("@total", a.Total);
                    cmd.Parameters.Add("@estado", SqlDbType.VarChar, 20).Value = a.Estado;
                    a.Id = (int)cmd.ExecuteScalar();

                    foreach (Jugada j in a.Jugadas)
                    {
                        SqlCommand cj = new SqlCommand(
                            @"INSERT INTO JUGADAS (IdApuesta, IdSorteo, Tipo, EsAutomatica, Monto, Orden)
                              VALUES (@apuesta, @sorteo, @tipo, @automatica, @monto, @orden);
                              SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx);
                        cj.Parameters.Add("@apuesta", SqlDbType.Int).Value = a.Id;
                        cj.Parameters.Add("@sorteo", SqlDbType.Int).Value = j.Sorteo.Id;
                        cj.Parameters.Add("@tipo", SqlDbType.VarChar, 20).Value = (object)j.Tipo ?? DBNull.Value;
                        cj.Parameters.Add("@automatica", SqlDbType.Bit).Value = j.EsAutomatica;
                        cj.Parameters.AddWithValue("@monto", j.Monto);
                        cj.Parameters.Add("@orden", SqlDbType.Int).Value = j.Orden;
                        j.Id = (int)cj.ExecuteScalar();

                        foreach (NumeroElegido n in j.Numeros)
                        {
                            SqlCommand cn = new SqlCommand(
                                @"INSERT INTO JUGADA_NUMERO (IdJugada, Numero, Orden, Rango)
                                  VALUES (@jugada, @numero, @orden, @rango)", con, tx);
                            cn.Parameters.Add("@jugada", SqlDbType.Int).Value = j.Id;
                            cn.Parameters.Add("@numero", SqlDbType.VarChar, 4).Value = n.Numero;
                            cn.Parameters.Add("@orden", SqlDbType.Int).Value = n.Orden;
                            cn.Parameters.Add("@rango", SqlDbType.Int).Value = (object)n.Rango ?? DBNull.Value;
                            cn.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
        }

        // Codigos de tickets que empiezan con el texto dado. Trae como maximo dos: alcanza
        // para saber si el prefijo identifica a un solo ticket.
        public List<Guid> BuscarCodigosPorPrefijo(string prefijo)
        {
            List<Guid> lista = new List<Guid>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT TOP 2 CodigoTicket FROM APUESTAS WHERE CONVERT(VARCHAR(36), CodigoTicket) LIKE @prefijo", con);
                cmd.Parameters.Add("@prefijo", SqlDbType.VarChar, 40).Value = prefijo + "%";
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        lista.Add((Guid)dr["CodigoTicket"]);
                }
            }
            return lista;
        }

        // Trae el ticket completo: cabecera, jugadas con su sorteo y numeros elegidos.
        // Devuelve null si no existe.
        public Apuesta ObtenerPorCodigo(Guid codigo)
        {
            Apuesta a;
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                    @"SELECT a.Id, a.CodigoTicket, a.UsuarioId, " + Mapeo.NombreUsuario + @" AS UsuarioNombre,
                             a.FechaHora, a.Total, a.Estado
                      FROM APUESTAS a JOIN USUARIOS u ON u.Id = a.UsuarioId
                      WHERE a.CodigoTicket = @codigo", con);
                cmd.Parameters.Add("@codigo", SqlDbType.UniqueIdentifier).Value = codigo;
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;
                    a = LeerCabecera(dr);
                }

                Dictionary<int, Jugada> porId = new Dictionary<int, Jugada>();
                SqlCommand cj = new SqlCommand(
                    "SELECT jg.Id AS JugadaId, jg.Tipo, jg.EsAutomatica, jg.Monto, jg.Orden, " + Mapeo.ColumnasSorteo + @"
                     FROM JUGADAS jg JOIN SORTEOS s ON s.Id = jg.IdSorteo " + Mapeo.JoinsSorteo + @"
                     WHERE jg.IdApuesta = @id ORDER BY jg.Orden", con);
                cj.Parameters.Add("@id", SqlDbType.Int).Value = a.Id;
                using (SqlDataReader dr = cj.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Jugada j = new Jugada
                        {
                            Id = (int)dr["JugadaId"],
                            Tipo = dr["Tipo"] == DBNull.Value ? null : (string)dr["Tipo"],
                            EsAutomatica = (bool)dr["EsAutomatica"],
                            Monto = (decimal)dr["Monto"],
                            Orden = (int)dr["Orden"],
                            Sorteo = Mapeo.LeerSorteo(dr)
                        };
                        a.Jugadas.Add(j);
                        porId[j.Id] = j;
                    }
                }

                SqlCommand cn = new SqlCommand(
                    @"SELECT n.IdJugada, n.Numero, n.Orden, n.Rango
                      FROM JUGADA_NUMERO n JOIN JUGADAS jg ON jg.Id = n.IdJugada
                      WHERE jg.IdApuesta = @id ORDER BY n.IdJugada, n.Orden", con);
                cn.Parameters.Add("@id", SqlDbType.Int).Value = a.Id;
                using (SqlDataReader dr = cn.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        porId[(int)dr["IdJugada"]].Numeros.Add(new NumeroElegido
                        {
                            Numero = (string)dr["Numero"],
                            Orden = (int)dr["Orden"],
                            Rango = dr["Rango"] == DBNull.Value ? (int?)null : (int)dr["Rango"]
                        });
                    }
                }
            }

            a.Juego = a.Jugadas.Count > 0 ? a.Jugadas[0].Sorteo.Juego : null;
            return a;
        }

        // Cambia el estado del ticket solo si sigue en el estado esperado. Devuelve false si
        // mientras tanto alguien lo modifico.
        public bool CambiarEstado(int idApuesta, string estadoNuevo, string estadoEsperado)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(
                    "UPDATE APUESTAS SET Estado = @nuevo WHERE Id = @id AND Estado = @esperado", con);
                cmd.Parameters.Add("@nuevo", SqlDbType.VarChar, 20).Value = estadoNuevo;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idApuesta;
                cmd.Parameters.Add("@esperado", SqlDbType.VarChar, 20).Value = estadoEsperado;
                con.Open();
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        // Tickets vendidos entre dos fechas (hasta es exclusivo), con su juego. El juego del
        // ticket se deduce de su primera jugada: todas sus jugadas son del mismo juego.
        public List<Apuesta> ListarVentas(DateTime desde, DateTime hastaExclusivo, int? idJuego)
        {
            List<Apuesta> lista = new List<Apuesta>();
            string q = @"SELECT a.Id, a.CodigoTicket, a.UsuarioId, " + Mapeo.NombreUsuario + @" AS UsuarioNombre,
                                a.FechaHora, a.Total, a.Estado, " + Mapeo.ColumnasJuego + @"
                         FROM APUESTAS a
                         JOIN USUARIOS u ON u.Id = a.UsuarioId
                         CROSS APPLY (SELECT TOP 1 s.IdJuego
                                      FROM JUGADAS jg JOIN SORTEOS s ON s.Id = jg.IdSorteo
                                      WHERE jg.IdApuesta = a.Id ORDER BY jg.Orden) x
                         JOIN JUEGOS j ON j.Id = x.IdJuego
                         WHERE a.FechaHora >= @desde AND a.FechaHora < @hasta
                           AND (@juego IS NULL OR j.Id = @juego)
                         ORDER BY a.FechaHora DESC";
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.Add("@desde", SqlDbType.DateTime).Value = desde;
                cmd.Parameters.Add("@hasta", SqlDbType.DateTime).Value = hastaExclusivo;
                cmd.Parameters.Add("@juego", SqlDbType.Int).Value = (object)idJuego ?? DBNull.Value;
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Apuesta a = LeerCabecera(dr);
                        a.Juego = Mapeo.LeerJuego(dr);
                        lista.Add(a);
                    }
                }
            }
            return lista;
        }

        private static Apuesta LeerCabecera(IDataRecord r)
        {
            return new Apuesta
            {
                Id = (int)r["Id"],
                CodigoTicket = (Guid)r["CodigoTicket"],
                UsuarioId = (int)r["UsuarioId"],
                UsuarioNombre = (string)r["UsuarioNombre"],
                FechaHora = (DateTime)r["FechaHora"],
                Total = (decimal)r["Total"],
                Estado = (string)r["Estado"]
            };
        }
    }
}
