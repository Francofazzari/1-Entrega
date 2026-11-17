using System;
using System.Collections.Generic;
using System.IO;
using BE;
using DAL;

namespace BLL
{
    public class BitacoraBLL
    {
        private BitacoraDAL dal = new BitacoraDAL();

        // Método rápido y fácil para llamar desde cualquier formulario
        public void Registrar(int usuarioId, string usuarioNombre, string actividad, string detalle)
        {
            Bitacora b = new Bitacora
            {
                FechaHora = DateTime.Now,
                UsuarioId = usuarioId,
                UsuarioNombre = usuarioNombre,
                Actividad = actividad,
                Detalle = detalle
            };
            dal.Registrar(b);
        }

        public List<Bitacora> Listar(DateTime desde, DateTime hasta, string texto)
        {
            // Validación de negocio: la fecha 'desde' no puede ser el futuro de 'hasta'
            if (desde > hasta)
            {
                throw new Exception("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.");
            }

            List<Bitacora> lista = dal.Listar(desde, hasta, texto);
            CompletarPerfiles(lista);
            return lista;
        }

        // Completa el perfil (Administrador / Operador) de cada registro. Se consulta una sola vez
        // por usuario, no una vez por fila.
        private static void CompletarPerfiles(List<Bitacora> lista)
        {
            PerfilBLL perfilBll = new PerfilBLL();
            Dictionary<int, string> perfiles = new Dictionary<int, string>();
            foreach (Bitacora b in lista)
            {
                string perfil;
                if (!perfiles.TryGetValue(b.UsuarioId, out perfil))
                {
                    perfil = perfilBll.ObtenerPerfilDeUsuario(b.UsuarioId);
                    perfiles[b.UsuarioId] = perfil;
                }
                b.Perfil = perfil;
            }
        }

        // Serializa a JSON los registros de la bitacora (los que se estan viendo en pantalla, con
        // los filtros usados para obtenerlos) y los guarda en el archivo indicado.
        public void ExportarJson(List<Bitacora> registros, DateTime desde, DateTime hasta, string filtro, string ruta)
        {
            if (registros == null || registros.Count == 0)
                throw new Exception("No hay registros para exportar.");
            if (string.IsNullOrWhiteSpace(ruta))
                throw new Exception("Indicá el archivo de destino.");

            Usuario usuario = SesionManager.Instancia.UsuarioActual;

            BitacoraExportacion exportacion = new BitacoraExportacion
            {
                FechaExportacion = DateTime.Now,
                ExportadoPor = usuario != null ? (usuario.Nombre + " " + usuario.Apellido).Trim() : "",
                Desde = desde.Date,
                Hasta = hasta.Date,
                Filtro = filtro ?? "",
                CantidadRegistros = registros.Count,
                Registros = registros
            };

            SerializadorJson.GuardarArchivo(exportacion, ruta);

            if (usuario != null)
                Registrar(usuario.Id, usuario.Nombre, "Exportar bitacora",
                    registros.Count + " registros exportados a " + Path.GetFileName(ruta));
        }
    }
}
