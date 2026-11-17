using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BE
{
    // Contenido del archivo JSON que genera "Exportar bitacora": los datos de la exportacion
    // (quien, cuando y con que filtros) y los registros que se estaban viendo en pantalla.
    [DataContract]
    public class BitacoraExportacion
    {
        [DataMember(Order = 1)]
        public DateTime FechaExportacion { get; set; }

        [DataMember(Order = 2)]
        public string ExportadoPor { get; set; }

        [DataMember(Order = 3)]
        public DateTime Desde { get; set; }

        [DataMember(Order = 4)]
        public DateTime Hasta { get; set; }

        [DataMember(Order = 5)]
        public string Filtro { get; set; }

        [DataMember(Order = 6)]
        public int CantidadRegistros { get; set; }

        [DataMember(Order = 7)]
        public List<Bitacora> Registros { get; set; } = new List<Bitacora>();
    }
}
