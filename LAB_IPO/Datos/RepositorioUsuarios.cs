using System;
using System.Collections.Generic;
using System.Linq;
using LAB_IPO.Modelos;

namespace LAB_IPO.Datos
{
    /// <summary>
    /// Usuarios de prueba. La práctica no exige persistencia,
    /// así que los datos viven en memoria mientras la aplicación está abierta.
    /// Las credenciales están también en leeme.txt.
    /// </summary>
    public static class RepositorioUsuarios
    {
        private static readonly List<Usuario> Usuarios = new List<Usuario>
        {
            new Usuario
            {
                Identificacion = "federico.g",
                Clave = "timoteo2026",
                Nombre = "Federico",
                Apellidos = "Gutiérrez",
                Rol = "Administrador",
                UltimoAcceso = new DateTime(2026, 11, 16, 19, 2, 0)
            },
            new Usuario
            {
                Identificacion = "marta.g",
                Clave = "camping2026",
                Nombre = "Marta",
                Apellidos = "Gil",
                Rol = "Administradora",
                UltimoAcceso = new DateTime(2026, 11, 14, 9, 45, 0)
            }
        };

        /// <summary>
        /// Devuelve el usuario si la identificación y la clave coinciden; si no, null.
        /// La identificación no distingue mayúsculas; la clave sí.
        /// </summary>
        public static Usuario Autenticar(string identificacion, string clave)
        {
            if (string.IsNullOrWhiteSpace(identificacion) || string.IsNullOrEmpty(clave))
                return null;

            return Usuarios.FirstOrDefault(u =>
                string.Equals(u.Identificacion, identificacion.Trim(), StringComparison.OrdinalIgnoreCase)
                && u.Clave == clave);
        }
    }
}
