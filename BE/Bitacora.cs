using System;
using System.Runtime.Serialization;

namespace BE
{
    // [DataContract] / [DataMember] indican que propiedades se escriben al serializar a JSON
    // (exportacion de la bitacora) y en que orden.
    [DataContract]
    public class Bitacora
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public DateTime FechaHora { get; set; }

        [DataMember(Order = 3)]
        public int UsuarioId { get; set; }

        [DataMember(Order = 4)]
        public string UsuarioNombre { get; set; }

        [DataMember(Order = 5)]
        public string Actividad { get; set; }

        [DataMember(Order = 6)]
        public string Detalle { get; set; }

        // Perfil del usuario (Administrador / Operador). No es una columna de la tabla Bitacora:
        // lo completa BitacoraBLL.Listar a partir de los permisos del usuario.
        [DataMember(Order = 7)]
        public string Perfil { get; set; }
    }
}
