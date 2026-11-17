using System;
using System.Data.SqlClient;

namespace DAL
{
    // Backup y restore de la base completa del sistema en un archivo .bak de SQL Server.
    // - Los comandos se ejecutan conectados a "master": no se puede restaurar una base mientras
    //   se esta conectado a ella.

    public class BackupDAL
    {
        // "Cannot open backup device": la carpeta no existe o el servicio no tiene permiso.
        private const int ErrorAbrirArchivo = 3201;
        
        // el archivo no es un backup valido.
        private const int ErrorArchivoInvalido = 3241;
        private const int ErrorArchivoVacio = 3254;

        public string NombreBase
        {
            get { return new SqlConnectionStringBuilder(Conexion.Cadena).InitialCatalog; }
        }

        public string Servidor
        {
            get { return new SqlConnectionStringBuilder(Conexion.Cadena).DataSource; }
        }

        // Misma cadena de conexion del sistema, pero apuntando a la base master.
        private static string CadenaMaster()
        {
            SqlConnectionStringBuilder b = new SqlConnectionStringBuilder(Conexion.Cadena);
            b.InitialCatalog = "master";
            return b.ConnectionString;
        }

        
        private string BaseEntreCorchetes()
        {
            return "[" + NombreBase.Replace("]", "]]") + "]";
        }

        public void Backup(string ruta)
        {
            string q = "BACKUP DATABASE " + BaseEntreCorchetes() +
                       " TO DISK = @ruta WITH FORMAT, INIT, NAME = @nombre";

            using (SqlConnection con = new SqlConnection(CadenaMaster()))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(q, con);
                cmd.CommandTimeout = 0; 
                cmd.Parameters.AddWithValue("@ruta", ruta);
                cmd.Parameters.AddWithValue("@nombre", NombreBase + " - Backup completo " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                EjecutarTraduciendoErrores(cmd, ruta);
            }
        }

        // Lee la cabecera del .bak (sin restaurar nada) y devuelve de que base es el backup.
        public string ObtenerBaseDelBackup(string ruta)
        {
            using (SqlConnection con = new SqlConnection(CadenaMaster()))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("RESTORE HEADERONLY FROM DISK = @ruta", con);
                cmd.Parameters.AddWithValue("@ruta", ruta);
                try
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        return dr.Read() ? dr["DatabaseName"].ToString() : null;
                    }
                }
                catch (SqlException ex) when (EsErrorDeArchivo(ex))
                {
                    throw new Exception(MensajeErrorDeArchivo(ex, ruta), ex);
                }
            }
        }

        public void Restore(string ruta)
        {
            string db = BaseEntreCorchetes();

            // Se descartan las conexiones que el programa tiene abiertas en el pool contra la base.
            SqlConnection.ClearAllPools();

            using (SqlConnection con = new SqlConnection(CadenaMaster()))
            {
                con.Open();

                // Se pone la base fuera de linea: se desconecta a todos los que la esten usando.
           
                // Management Studio) y nadie se puede volver a conectar mientras se restaura.
                EjecutarSql(con, "ALTER DATABASE " + db + " SET OFFLINE WITH ROLLBACK IMMEDIATE");

                try
                {
                    // El restore reemplaza los archivos de la base y la vuelve a poner en linea.
                    SqlCommand cmd = new SqlCommand("RESTORE DATABASE " + db + " FROM DISK = @ruta WITH REPLACE", con);
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    EjecutarTraduciendoErrores(cmd, ruta);
                }
                catch
                {
                    // Si el restore fallo, la base queda en linea como estaba antes.
                    EjecutarSql(con, "ALTER DATABASE " + db + " SET ONLINE");
                    throw;
                }

                // Por si el backup se hizo con la base en modo "un solo usuario".
                EjecutarSql(con, "ALTER DATABASE " + db + " SET MULTI_USER");
            }

            // Las conexiones viejas del pool apuntan a la base anterior: se descartan para que la
            // proxima consulta abra una conexion nueva.
            SqlConnection.ClearAllPools();
        }

        private static void EjecutarSql(SqlConnection con, string sql)
        {
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
        }

        private static void EjecutarTraduciendoErrores(SqlCommand cmd, string ruta)
        {
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex) when (EsErrorDeArchivo(ex))
            {
                throw new Exception(MensajeErrorDeArchivo(ex, ruta), ex);
            }
        }

        private static bool EsErrorDeArchivo(SqlException ex)
        {
            foreach (SqlError e in ex.Errors)
                if (e.Number == ErrorAbrirArchivo || e.Number == ErrorArchivoInvalido || e.Number == ErrorArchivoVacio)
                    return true;
            return false;
        }

        private static string MensajeErrorDeArchivo(SqlException ex, string ruta)
        {
            foreach (SqlError e in ex.Errors)
                if (e.Number == ErrorArchivoInvalido || e.Number == ErrorArchivoVacio)
                    return "El archivo no es un backup válido de SQL Server:\n" + ruta;

            return "SQL Server no pudo abrir el archivo:\n" + ruta + "\n\n" +
                   "El archivo .bak lo lee y escribe el servicio de SQL Server, no el programa: " +
                   "la carpeta tiene que existir y el servicio tiene que tener permiso sobre ella. " +
                   "Las carpetas del perfil del usuario (Escritorio, Documentos, Descargas) no sirven; " +
                   "usá la carpeta de backups que propone el sistema.";
        }
    }
}
