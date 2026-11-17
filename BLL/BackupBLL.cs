using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BE;
using DAL;

namespace BLL
{
    // Backup (.bak) y restore de la base de datos del sistema. Solo lo usa el Administrador.
    public class BackupBLL
    {
        private BackupDAL dal = new BackupDAL();

        public string NombreBase
        {
            get { return dal.NombreBase; }
        }

        public string Servidor
        {
            get { return dal.Servidor; }
        }

        // Carpeta propuesta para los backups: C:\Users\Public\Documents\AgenciaQuiniela\Backups.
        // Es una carpeta compartida de Windows, a la que acceden tanto el usuario (para elegir el
        // archivo) como el servicio de SQL Server (que es quien escribe y lee el .bak).
        public string CarpetaPorDefecto()
        {
            string carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                "AgenciaQuiniela", "Backups");
            Directory.CreateDirectory(carpeta);
            return carpeta;
        }

        // El backup mas reciente de la carpeta por defecto (o null si no hay ninguno). Se propone
        // como archivo a restaurar.
        public string UltimoBackup()
        {
            // Solo los que tienen el nombre que genera el sistema (AgenciaQuiniela_aaaammdd_hhmmss.bak),
            // para no proponer el backup de otra base (por ejemplo AgenciaQuiniela_Prueba_...).
            Regex nombreDelSistema = new Regex("^" + Regex.Escape(NombreBase) + @"_\d{8}_\d{6}\.bak$", RegexOptions.IgnoreCase);
            DirectoryInfo carpeta = new DirectoryInfo(CarpetaPorDefecto());
            FileInfo ultimo = carpeta.GetFiles("*.bak")
                .Where(f => nombreDelSistema.IsMatch(f.Name))
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();
            return ultimo != null ? ultimo.FullName : null;
        }

        // Ej.: AgenciaQuiniela_20261114_104530.bak
        public string NombreArchivoSugerido()
        {
            return NombreBase + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
        }

        public void RealizarBackup(string ruta)
        {
            ruta = ValidarRuta(ruta);

            // Se registra ANTES de hacer el backup para que el registro quede dentro del .bak: asi,
            // despues de restaurarlo, en la bitacora se sigue viendo el "Backup" y luego el "Restore".
            RegistrarEnBitacora("Backup", "Backup de la base " + NombreBase + " en " + ruta);
            try
            {
                dal.Backup(ruta);
            }
            catch (Exception)
            {
                RegistrarEnBitacora("Backup fallido", "No se pudo hacer el backup en " + ruta);
                throw;
            }
        }

        public void RealizarRestore(string ruta)
        {
            ruta = ValidarRuta(ruta);

            // Antes de pisar nada se controla que el archivo sea un backup de esta misma base.
            string baseDelBackup = dal.ObtenerBaseDelBackup(ruta);
            if (!string.Equals(baseDelBackup, NombreBase, StringComparison.OrdinalIgnoreCase))
                throw new Exception("El archivo es un backup de la base '" + baseDelBackup +
                                    "', no de '" + NombreBase + "'. No se restauró nada.");

            dal.Restore(ruta);

            // El idioma y las traducciones quedan guardados en memoria: se vuelven a leer de la base
            // restaurada para que no se sigan viendo los datos de antes del restore.
            try
            {
                IdiomaManager.Instancia.RecargarDesdeBase();
            }
            catch (Exception)
            {
                // no impide el restore, que ya se hizo
            }

            // Se registra despues del restore para que quede en la bitacora de la base restaurada.
            RegistrarEnBitacora("Restore", "Restore de la base " + NombreBase + " desde " + ruta);
        }

        private static string ValidarRuta(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                throw new Exception("Indicá el archivo .bak.");

            ruta = ruta.Trim();
            if (!Path.IsPathRooted(ruta))
                throw new Exception("Indicá la ruta completa del archivo (por ejemplo C:\\...\\archivo.bak).");
            if (!string.Equals(Path.GetExtension(ruta), ".bak", StringComparison.OrdinalIgnoreCase))
                throw new Exception("El archivo tiene que tener extensión .bak.");
            return ruta;
        }

        // Si no se puede registrar (por ejemplo, el usuario de la sesion no existe en la base
        // restaurada) no se informa error: el backup/restore ya se hizo.
        private static void RegistrarEnBitacora(string actividad, string detalle)
        {
            Usuario u = SesionManager.Instancia.UsuarioActual;
            if (u == null) return;
            try
            {
                new BitacoraBLL().Registrar(u.Id, u.Nombre, actividad, detalle);
            }
            catch (Exception)
            {
            }
        }
    }
}
