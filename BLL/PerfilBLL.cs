using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;

namespace BLL
{
    public class PerfilBLL
    {
        private PerfilDAL dal = new PerfilDAL();

        public List<Permiso> ObtenerTodos()
        {
            return dal.ObtenerTodos();
        }

        public Permiso ObtenerArbol(int permisoRaizId)
        {
            return dal.ObtenerArbol(permisoRaizId);
        }

        public List<Permiso> ObtenerPermisosDeUsuario(int usuarioId)
        {
            return dal.ObtenerPermisosDeUsuario(usuarioId);
        }

        public int AgregarFamilia(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                throw new Exception("El nombre es obligatorio.");
            
            Permiso p = new PermisoCompleto 
            { 
                Nombre = nombre,
                Codigo = nombre, // Simplificacion
                EsPadre = true 
            };
            return dal.Insertar(p);
        }

        public int AgregarPatente(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                throw new Exception("El nombre es obligatorio.");
            
            Permiso p = new PermisoSimple 
            { 
                Nombre = nombre,
                Codigo = nombre,
                EsPadre = false 
            };
            return dal.Insertar(p);
        }

        public bool GuardarFamilia(int familiaId, List<int> permisosIds)
        {
            dal.LimpiarHijos(familiaId);
            bool ok = true;
            foreach (int idHijo in permisosIds)
            {
                if (!dal.InsertarPermisoPermiso(familiaId, idHijo))
                {
                    ok = false;
                }
            }
            return ok;
        }

        public int CrearEmpleado(string nombre, string nroTerminal)
        {
            UsuarioDAL usuarioDAL = new UsuarioDAL();
            Usuario u = new Usuario
            {
                NroTerminal = nroTerminal,
                Clave = Encriptador.HashSHA256("1234"), // clave por defecto
                Nombre = nombre,
                Apellido = ""
            };
            return usuarioDAL.InsertarYObtenerID(u);
        }

        public bool AsignarPermisosUsuario(int usuarioId, List<int> permisosIds)
        {
            UsuarioDAL usuarioDAL = new UsuarioDAL();
            usuarioDAL.LimpiarPermisos(usuarioId);
            
            bool ok = true;
            foreach (int pid in permisosIds)
            {
                if (!usuarioDAL.AsignarPermiso(usuarioId, pid))
                {
                    ok = false;
                }
            }
            return ok;
        }

        public bool EliminarPermiso(int id)
        {
            return dal.Eliminar(id);
        }
    }
}
