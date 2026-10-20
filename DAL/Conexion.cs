using System.Configuration;

namespace DAL
{
    // Cadena de conexion unica para toda la capa de datos. Se toma del App.config de la
    // aplicacion (connectionStrings > AgenciaQuiniela); si no esta definida, se usa la base
    // local por defecto. Para correr el sistema en otra maquina alcanza con cambiar el
    // App.config, sin tocar el codigo.
    internal static class Conexion
    {
        private const string PorDefecto = "Server=.;Database=AgenciaQuiniela;Integrated Security=True;";

        public static string Cadena
        {
            get
            {
                ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings["AgenciaQuiniela"];
                return cs != null && !string.IsNullOrWhiteSpace(cs.ConnectionString)
                    ? cs.ConnectionString
                    : PorDefecto;
            }
        }
    }
}
