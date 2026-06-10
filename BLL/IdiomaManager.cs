using System.Collections.Generic;
using BE;
using DAL;

namespace BLL
{
    public class IdiomaManager
    {
        private static IdiomaManager _instancia;
        public static IdiomaManager Instancia
        {
            get
            {
                if (_instancia == null) _instancia = new IdiomaManager();
                return _instancia;
            }
        }

        private IdiomaManager() { }

        // Aquí guardamos todos los formularios que están "escuchando" los cambios de idioma
        private List<IObservadorIdioma> observadores = new List<IObservadorIdioma>();

        public Idioma IdiomaActual { get; private set; }
        private Dictionary<string, string> traduccionesActuales = new Dictionary<string, string>();
        private IdiomaDAL dal = new IdiomaDAL();

        public void Suscribir(IObservadorIdioma obs)
        {
            observadores.Add(obs);
            // Si ya hay un idioma seleccionado, se lo aplicamos automáticamente al suscribirse
            if (IdiomaActual != null)
            {
                obs.ActualizarIdioma(IdiomaActual);
            }
        }

        public void Desuscribir(IObservadorIdioma obs)
        {
            observadores.Remove(obs);
        }

        public void CambiarIdioma(Idioma nuevoIdioma)
        {
            IdiomaActual = nuevoIdioma;
            // Al cambiar el idioma, vamos a la BD a buscar sus traducciones
            traduccionesActuales = dal.ObtenerTraducciones(nuevoIdioma.Id);
            Notificar(); // Avisamos a todos los formularios
        }

        private void Notificar()
        {
            foreach (var obs in observadores)
            {
                obs.ActualizarIdioma(IdiomaActual);
            }
        }

        public string ObtenerMensaje(string clave)
        {
            if (traduccionesActuales.ContainsKey(clave))
                return traduccionesActuales[clave];

            return clave; // Si no encuentra la traducción, muestra la clave por defecto
        }

        public List<Idioma> ObtenerIdiomas()
        {
            return dal.ObtenerIdiomas();
        }
    }
}