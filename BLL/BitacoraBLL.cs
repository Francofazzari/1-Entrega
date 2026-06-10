using System;
using System.Collections.Generic;
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

            return dal.Listar(desde, hasta, texto);
        }
    }
}