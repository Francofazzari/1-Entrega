using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;

namespace BLL
{
    public class UsuarioBLL
    {
        private UsuarioDAL dal = new UsuarioDAL();

        private string ObtenerResponsable()
        {
            if (SesionManager.Instancia != null && SesionManager.Instancia.UsuarioActual != null)
                return SesionManager.Instancia.UsuarioActual.Nombre + " " + SesionManager.Instancia.UsuarioActual.Apellido;
            return "Sistema";
        }

        public bool Login(string nroTerminal, string clave)
        {
            try
            {
                if (string.IsNullOrEmpty(nroTerminal) || string.IsNullOrEmpty(clave))
                    throw new Exception("Complete todos los campos.");

                string claveHash = Encriptador.HashSHA256(clave);
                Usuario u = dal.Login(nroTerminal, claveHash);

                if (u != null)
                {
                    // Cargar Permisos
                    PerfilBLL perfilBll = new PerfilBLL();
                    u.Permisos = perfilBll.ObtenerPermisosDeUsuario(u.Id);

                    SesionManager.Instancia.IniciarSesion(u);
                    dal.RegistrarAuditoria(u.Id, "LOGIN");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al intentar iniciar sesion: " + ex.Message);
            }
        }

        public void Logout()
        {
            if (SesionManager.Instancia.EstaLogueado)
            {
                dal.RegistrarAuditoria(SesionManager.Instancia.UsuarioActual.Id, "LOGOUT");
                SesionManager.Instancia.CerrarSesion();
            }
        }

        public List<Usuario> ObtenerTodos()
        {
            return dal.ObtenerTodos();
        }

        public bool AgregarUsuario(Usuario u)
        {
            if (string.IsNullOrEmpty(u.NroTerminal) || string.IsNullOrEmpty(u.Clave))
                throw new Exception("Terminal y contraseña son obligatorios.");

            u.Clave = Encriptador.HashSHA256(u.Clave);
            return dal.Insertar(u, ObtenerResponsable());
        }

        public bool ModificarUsuario(Usuario u)
        {
            if (string.IsNullOrEmpty(u.NroTerminal))
                throw new Exception("El numero de terminal es obligatorio.");

            if (string.IsNullOrEmpty(u.Clave))
            {
                // El campo Contraseña se deja en blanco a proposito cuando no se la quiere cambiar
                // (dgvUsuarios_CellClick lo vacia al seleccionar una fila). Si se guardara vacio tal
                // cual, se pisaria el hash real y el usuario quedaria sin poder iniciar sesion.
                Usuario actual = dal.ObtenerTodos().FirstOrDefault(x => x.Id == u.Id);
                u.Clave = actual != null ? actual.Clave : "";
            }
            else if (u.Clave.Length < 64) // Si no esta hasheada
            {
                u.Clave = Encriptador.HashSHA256(u.Clave);
            }

            return dal.Modificar(u, ObtenerResponsable());
        }

        public bool EliminarUsuario(int id)
        {
            Usuario u = new Usuario { Id = id };
            return dal.Eliminar(u, ObtenerResponsable());
        }

        public List<UsuarioHistorial> ObtenerHistorial(int usuarioId)
        {
            return dal.ObtenerHistorial(usuarioId);
        }

        public bool RestaurarVersion(int idHistorial)
        {
            return dal.RestaurarVersion(idHistorial, ObtenerResponsable());
        }
    }
}
