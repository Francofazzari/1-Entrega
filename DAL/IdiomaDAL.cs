using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BE;

namespace DAL
{
    public class IdiomaDAL
    {
        private string conexion = "Server=.;Database=AgenciaQuiniela;Integrated Security=True;";

        // Digito horizontal: suma ponderada (valor de caracter x posicion del caracter x posicion
        // del atributo dentro de la entidad: Nombre=1, Codigo=2) modulo 11.
        private static int CalcularDigitoHorizontal(Idioma idioma)
        {
            long suma = 0;
            int posicionAtributo = 1;
            foreach (string valor in new[] { idioma.Nombre, idioma.Codigo })
            {
                if (!string.IsNullOrEmpty(valor))
                {
                    for (int i = 0; i < valor.Length; i++)
                    {
                        int posicionCaracter = i + 1;
                        suma += (long)valor[i] * posicionCaracter * posicionAtributo;
                    }
                }
                posicionAtributo++;
            }
            return (int)(suma % 11);
        }

        // Digito vertical: se calcula directamente sobre los digitos verificadores horizontales
        // (la columna DigitoVerificador de IDIOMAS), no sobre Nombre/Codigo. Cada digito horizontal
        // se pondera por la posicion de su fila (orden por Id) y se suma; el resultado va modulo 11.
        // Como usa los digitos horizontales ya calculados, detecta tanto filas agregadas/eliminadas
        // (cambia cuantos terminos entran en la suma) como una alteracion directa del propio
        // DigitoVerificador hecha por fuera del sistema.
        private static int CalcularDigitoVertical(IEnumerable<int> digitosHorizontalesEnOrden)
        {
            long suma = 0;
            int posicionFila = 1;
            foreach (int digito in digitosHorizontalesEnOrden)
            {
                suma += (long)digito * posicionFila;
                posicionFila++;
            }
            return (int)(suma % 11);
        }

        // Recalcula y persiste el digito horizontal de cada fila y el vertical (unico, de toda la
        // tabla). Se ejecuta dentro de la misma transaccion que cualquier alta/baja/modificacion
        // legitima, de forma que la unica manera de que los digitos queden "desactualizados" sea una
        // modificacion hecha por fuera del sistema.
        private void RecalcularDigitos(SqlConnection con, SqlTransaction tx)
        {
            List<Idioma> idiomas = new List<Idioma>();
            SqlCommand cmdSel = new SqlCommand("SELECT Id, Nombre, Codigo FROM IDIOMAS ORDER BY Id", con, tx);
            using (SqlDataReader dr = cmdSel.ExecuteReader())
            {
                while (dr.Read())
                {
                    idiomas.Add(new Idioma
                    {
                        Id = (int)dr["Id"],
                        Nombre = dr["Nombre"].ToString(),
                        Codigo = dr["Codigo"].ToString()
                    });
                }
            }

            List<int> digitosHorizontales = new List<int>();
            foreach (Idioma i in idiomas)
            {
                int digitoHorizontal = CalcularDigitoHorizontal(i);
                digitosHorizontales.Add(digitoHorizontal);

                SqlCommand cmdUpd = new SqlCommand("UPDATE IDIOMAS SET DigitoVerificador = @d WHERE Id = @id", con, tx);
                cmdUpd.Parameters.AddWithValue("@d", digitoHorizontal);
                cmdUpd.Parameters.AddWithValue("@id", i.Id);
                cmdUpd.ExecuteNonQuery();
            }

            SqlCommand cmdDel = new SqlCommand("DELETE FROM IDIOMAS_DIGITO_VERTICAL", con, tx);
            cmdDel.ExecuteNonQuery();

            SqlCommand cmdIns = new SqlCommand("INSERT INTO IDIOMAS_DIGITO_VERTICAL (Digito) VALUES (@d)", con, tx);
            cmdIns.Parameters.AddWithValue("@d", CalcularDigitoVertical(digitosHorizontales));
            cmdIns.ExecuteNonQuery();
        }

        // Recalculo forzado (horizontal y vertical) a pedido del administrador, por ejemplo
        // despues de corregir a mano en la base un dato que disparo un error de integridad.
        // Abre su propia conexion/transaccion porque, a diferencia de RecalcularDigitos, no se
        // llama desde dentro de otra operacion (Insertar/Renombrar/Eliminar) que ya tenga una.
        public bool RecalcularTodosLosDigitos()
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    RecalcularDigitos(con, tx);
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

        // Verifica la integridad horizontal (fila vs su digito) y vertical (columna vs su digito)
        // de la tabla IDIOMAS. Devuelve la lista de inconsistencias encontradas (vacia si esta todo OK).
        // La primera vez que corre (digitos aun no calculados) establece la linea base en vez de reportar error.
        public List<string> VerificarIntegridad()
        {
            List<string> errores = new List<string>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    List<Idioma> idiomas = new List<Idioma>();
                    SqlCommand cmdSel = new SqlCommand(
                        "SELECT Id, Nombre, Codigo, DigitoVerificador FROM IDIOMAS ORDER BY Id", con, tx);
                    using (SqlDataReader dr = cmdSel.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            idiomas.Add(new Idioma
                            {
                                Id = (int)dr["Id"],
                                Nombre = dr["Nombre"].ToString(),
                                Codigo = dr["Codigo"].ToString(),
                                DigitoVerificador = dr["DigitoVerificador"] == DBNull.Value ? (int?)null : (int)dr["DigitoVerificador"]
                            });
                        }
                    }

                    int? verticalGuardado = null;
                    SqlCommand cmdVert = new SqlCommand("SELECT TOP 1 Digito FROM IDIOMAS_DIGITO_VERTICAL", con, tx);
                    object verticalObj = cmdVert.ExecuteScalar();
                    if (verticalObj != null && verticalObj != DBNull.Value)
                    {
                        verticalGuardado = Convert.ToInt32(verticalObj);
                    }

                    // La linea base solo se establece la primera vez que corre (todavia no existe
                    // el digito vertical). Una vez establecida, cualquier fila sin digito horizontal
                    // (por ejemplo insertada por fuera del sistema) es un error, no un caso a
                    // adoptar silenciosamente.
                    bool primeraVez = verticalGuardado == null;

                    if (primeraVez)
                    {
                        RecalcularDigitos(con, tx);
                    }
                    else
                    {
                        foreach (Idioma i in idiomas)
                        {
                            if (i.DigitoVerificador == null)
                            {
                                errores.Add(string.Format(
                                    "IDIOMAS: la fila Id={0} ('{1}') no tiene digito verificador " +
                                    "(posible alta hecha fuera del sistema).",
                                    i.Id, i.Nombre));
                            }
                            else if (CalcularDigitoHorizontal(i) != i.DigitoVerificador)
                            {
                                errores.Add(string.Format(
                                    "IDIOMAS: la fila Id={0} ('{1}') no coincide con su digito verificador horizontal.",
                                    i.Id, i.Nombre));
                            }
                        }

                        // El vertical se calcula sobre los DigitoVerificador guardados; si falta
                        // alguno, ya quedo reportado arriba y no tiene sentido sumar un hueco.
                        if (idiomas.All(i => i.DigitoVerificador.HasValue))
                        {
                            int esperado = CalcularDigitoVertical(idiomas.Select(i => i.DigitoVerificador.Value));
                            if (esperado != verticalGuardado.Value)
                            {
                                errores.Add(
                                    "IDIOMAS: el digito verificador vertical no coincide (posible fila agregada, " +
                                    "eliminada, reordenada o con su DigitoVerificador alterado fuera del sistema).");
                            }
                        }
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            return errores;
        }

        public List<Idioma> ObtenerIdiomas()
        {
            List<Idioma> lista = new List<Idioma>();
            using (SqlConnection con = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("SELECT Id, Nombre, Codigo, DigitoVerificador FROM IDIOMAS", con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new Idioma
                    {
                        Id = (int)dr["Id"],
                        Nombre = dr["Nombre"].ToString(),
                        Codigo = dr["Codigo"].ToString(),
                        DigitoVerificador = dr["DigitoVerificador"] == DBNull.Value ? (int?)null : (int)dr["DigitoVerificador"]
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
                SqlCommand cmd = new SqlCommand("SELECT Tag, Traduccion FROM TRADUCCIONES WHERE IdiomaId = @id", con);
                cmd.Parameters.AddWithValue("@id", idiomaId);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    traducciones.Add(dr["Tag"].ToString(), dr["Traduccion"].ToString());
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
                    SELECT tAll.Tag as Clave, ISNULL(tId.Traduccion, '') as Valor
                    FROM (SELECT Tag FROM PALABRAS) tAll
                    LEFT JOIN TRADUCCIONES tId ON tAll.Tag = tId.Tag AND tId.IdiomaId = @id
                    ORDER BY tAll.Tag";

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
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO IDIOMAS (Nombre, Codigo) VALUES (@nom, @cod); SELECT SCOPE_IDENTITY();", con, tx);
                    cmd.Parameters.AddWithValue("@nom", idioma.Nombre);
                    cmd.Parameters.AddWithValue("@cod", idioma.Codigo ?? "");
                    int newId = System.Convert.ToInt32(cmd.ExecuteScalar());
                    idioma.Id = newId;

                    RecalcularDigitos(con, tx);

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

        public bool RenombrarIdioma(int id, string nuevoNombre)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand("UPDATE IDIOMAS SET Nombre = @nom WHERE Id = @id", con, tx);
                    cmd.Parameters.AddWithValue("@nom", nuevoNombre);
                    cmd.Parameters.AddWithValue("@id", id);
                    int filas = cmd.ExecuteNonQuery();

                    RecalcularDigitos(con, tx);

                    tx.Commit();
                    return filas > 0;
                }
                catch
                {
                    tx.Rollback();
                    return false;
                }
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

                    RecalcularDigitos(con, tx);

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
                        if (string.IsNullOrWhiteSpace(t.Valor)) continue; 
                        
                        SqlCommand cmdIns = new SqlCommand("INSERT INTO TRADUCCIONES (IdiomaId, Tag, Traduccion) VALUES (@id, @clave, @valor)", con, tx);
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
