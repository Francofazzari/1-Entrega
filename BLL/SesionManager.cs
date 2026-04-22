using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace BLL
{
    public class SesionManager
    {
        private static SesionManager _instancia;

        public static SesionManager Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new SesionManager();
                return _instancia;
            }
        }

        private SesionManager() { }

        public Usuario UsuarioActual { get; private set; }
        public bool EstaLogueado => UsuarioActual != null;

        public void IniciarSesion(Usuario u)
        {
            UsuarioActual = u;
        }

        public void CerrarSesion()
        {
            UsuarioActual = null;
        }
    }
}
