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

        public PermisoCompleto ConstruirArbol(int permisoRaizId)
        {
            List<Permiso> todos = dal.ObtenerTodos();
            return ConstruirNodo(permisoRaizId, todos);
        }

        private PermisoCompleto ConstruirNodo(int id, List<Permiso> todos)
        {
            Permiso raiz = todos.FirstOrDefault(p => p.Id == id);
            if (raiz == null || !(raiz is PermisoCompleto))
                return null;

            PermisoCompleto compuesto = (PermisoCompleto)raiz;

            // Busca los hijos directos cuyo PermisoPadreId == id
            List<Permiso> hijos = todos.Where(p => p.PermisoPadreId == id).ToList();

            foreach (Permiso hijo in hijos)
            {
                if (hijo is PermisoCompleto)
                    compuesto.Agregar(ConstruirNodo(hijo.Id, todos));
                else
                    compuesto.Agregar(hijo);
            }
            return compuesto;
        }

        public PermisoCompleto ObtenerArbolPorPerfil(string nombrePerfil)
        {
            Perfil p = dal.ObtenerPerfilPorNombre(nombrePerfil);
            if (p == null || p.PermisoRaizId == 0) return null;
            return ConstruirArbol(p.PermisoRaizId);
        }
        public List<Perfil> ObtenerPerfiles()
        {
            return dal.ObtenerPerfiles();
        }
        public bool AgregarPermiso(Permiso p, int? padreId)
        {
            if (string.IsNullOrEmpty(p.Codigo) || string.IsNullOrEmpty(p.Nombre))
                throw new Exception("El codigo y el nombre son obligatorios.");
            return dal.Insertar(p, padreId);
        }
        public int CrearEmpleado(string nombre, string nroTerminal)
        {
            // Crea el usuario con perfil Operador (Id=2) por defecto
            UsuarioDAL usuarioDAL = new UsuarioDAL();
            Usuario u = new Usuario
            {
                NroTerminal = nroTerminal,
                Clave = Encriptador.HashSHA256("1234"), // clave por defecto
                Nombre = nombre,
                Apellido = "",
                PerfilId = 2 // Operador por defecto
            };
            return usuarioDAL.InsertarYObtenerID(u);
        }

        public bool AsignarFunciones(int usuarioId, string nombreEmpleado, List<string> funciones)
        {
            // Crea un perfil nuevo para este empleado
            int perfilId = dal.InsertarPerfilYObtenerID(
                $"Perfil_{nombreEmpleado}", $"Perfil de {nombreEmpleado}");

            if (perfilId == 0) return false;

            // Inserta cada función como permiso del perfil
            foreach (string funcion in funciones)
            {
                int permisoId = dal.ObtenerIdPermisoPorNombre(funcion);
                if (permisoId > 0)
                    dal.InsertarPerfilPermiso(perfilId, permisoId);
            }

            // Asigna el perfil al usuario
            UsuarioDAL usuarioDAL = new UsuarioDAL();
            return usuarioDAL.ActualizarPerfil(usuarioId, perfilId);
        }

        public List<string> ObtenerFuncionesDeUsuario(int usuarioId)
        {
            return dal.ObtenerFuncionesDeUsuario(usuarioId);
        }
        public bool EliminarPermiso(int id)
        {
            return dal.Eliminar(id);
        }
    }
}