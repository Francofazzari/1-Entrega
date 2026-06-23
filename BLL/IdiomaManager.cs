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

        private List<IObservadorIdioma> observadores = new List<IObservadorIdioma>();

        public Idioma IdiomaActual { get; private set; }
        private Dictionary<string, string> traduccionesActuales = new Dictionary<string, string>();
        private IdiomaDAL dal = new IdiomaDAL();

        public void Suscribir(IObservadorIdioma obs)
        {
            observadores.Add(obs);
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
            traduccionesActuales = dal.ObtenerTraducciones(nuevoIdioma.Id);
            Notificar();
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

            return clave;
        }

        public List<Idioma> ObtenerIdiomas()
        {
            return dal.ObtenerIdiomas();
        }

        public List<Traduccion> ObtenerTraduccionesGrid(int idiomaId)
        {
            return dal.ObtenerTraduccionesGrid(idiomaId);
        }

        public bool InsertarIdioma(Idioma i)
        {
            return dal.InsertarIdioma(i);
        }

        public bool RenombrarIdioma(int id, string nuevoNombre)
        {
            return dal.RenombrarIdioma(id, nuevoNombre);
        }

        public bool EliminarIdioma(int id)
        {
            return dal.EliminarIdioma(id);
        }

        public bool GuardarTraducciones(int idiomaId, List<Traduccion> traducciones)
        {
            bool res = dal.GuardarTraducciones(idiomaId, traducciones);
            if (res && IdiomaActual != null && IdiomaActual.Id == idiomaId)
            {
                CambiarIdioma(IdiomaActual); // Refresca si es el idioma actual
            }
            return res;
        }
    }
}
