using System;
using LAB_IPO.Modelos;

namespace LAB_IPO.Servicios
{
    /// <summary>
    /// Sesión del usuario que ha iniciado sesión.
    /// Guarda el acceso anterior para mostrarlo y registra el acceso actual.
    /// </summary>
    public static class Sesion
    {
        public static Usuario UsuarioActual { get; private set; }

        /// <summary>Último acceso antes de esta sesión (lo que se muestra en la cabecera).</summary>
        public static DateTime? AccesoAnterior { get; private set; }

        public static void Iniciar(Usuario usuario)
        {
            UsuarioActual = usuario ?? throw new ArgumentNullException(nameof(usuario));
            AccesoAnterior = usuario.UltimoAcceso;
            usuario.UltimoAcceso = DateTime.Now;
        }

        public static void Cerrar()
        {
            UsuarioActual = null;
            AccesoAnterior = null;
        }
    }
}
