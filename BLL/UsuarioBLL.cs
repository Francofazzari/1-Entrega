using System;
using System.Collections.Generic;
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
            if (string.IsNullOrEmpty(nroTerminal) || string.IsNullOrEmpty(clave))
                throw new Exception("Complete todos los campos.");

            string claveHash = Encriptador.HashSHA256(clave);
            Usuario u = dal.ObtenerPorTerminal(nroTerminal);

            if (u != null && u.Clave == claveHash)
            {
                SesionManager.Instancia.IniciarSesion(u);
                dal.RegistrarAuditoria(u.Id, "LOGIN");
                return true;
            }
            return false;
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
                throw new Exception("Terminal y contrasea son obligatorios.");

            u.Clave = Encriptador.HashSHA256(u.Clave);
            return dal.Insertar(u, ObtenerResponsable());
        }

        public bool ModificarUsuario(Usuario u)
        {
            if (string.IsNullOrEmpty(u.NroTerminal))
                throw new Exception("El numero de terminal es obligatorio.");

            if (!string.IsNullOrEmpty(u.Clave))
                u.Clave = Encriptador.HashSHA256(u.Clave);

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
