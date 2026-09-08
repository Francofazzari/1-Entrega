using System;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class ApuestaDAL
    {
        private string conexion = "Server=.;Database=AgenciaQuiniela;Integrated Security=True;";

        public bool Insertar(Apuesta a)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    string q = @"INSERT INTO APUESTAS (UsuarioId, FechaHora, Tipo, Total, Activa)
                                 VALUES (@uid, GETDATE(), @tipo, @total, 1);
                                 SELECT SCOPE_IDENTITY();";
                    SqlCommand cmd = new SqlCommand(q, con, tx);
                    cmd.Parameters.AddWithValue("@uid", a.UsuarioId);
                    cmd.Parameters.AddWithValue("@tipo", a.Tipo);
                    cmd.Parameters.AddWithValue("@total", a.Total);
                    int idApuesta = Convert.ToInt32(cmd.ExecuteScalar());
                    a.Id = idApuesta;

                    foreach (Loteria l in a.Loterias)
                    {
                        SqlCommand cmdLot = new SqlCommand(
                            "INSERT INTO APUESTA_LOTERIA (IdApuesta, IdLoteria) VALUES (@idA, @idL)", con, tx);
                        cmdLot.Parameters.AddWithValue("@idA", idApuesta);
                        cmdLot.Parameters.AddWithValue("@idL", l.Id);
                        cmdLot.ExecuteNonQuery();
                    }

                    foreach (Turno t in a.Turnos)
                    {
                        SqlCommand cmdTur = new SqlCommand(
                            "INSERT INTO APUESTA_TURNO (IdApuesta, IdTurno) VALUES (@idA, @idT)", con, tx);
                        cmdTur.Parameters.AddWithValue("@idA", idApuesta);
                        cmdTur.Parameters.AddWithValue("@idT", t.Id);
                        cmdTur.ExecuteNonQuery();
                    }

                    int orden = 1;
                    foreach (Jugada j in a.Jugadas)
                    {
                        SqlCommand cmdNum = new SqlCommand(
                            @"INSERT INTO APUESTA_NUMERO (IdApuesta, Numero, Rango, Monto, Orden)
                              VALUES (@idA, @nro, @rango, @monto, @orden)", con, tx);
                        cmdNum.Parameters.AddWithValue("@idA", idApuesta);
                        cmdNum.Parameters.AddWithValue("@nro", j.Numero);
                        cmdNum.Parameters.AddWithValue("@rango", j.Rango);
                        cmdNum.Parameters.AddWithValue("@monto", j.Monto);
                        cmdNum.Parameters.AddWithValue("@orden", orden++);
                        cmdNum.ExecuteNonQuery();
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
