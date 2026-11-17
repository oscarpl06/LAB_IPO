using System;

namespace LAB_IPO.Modelos
{
    /// <summary>
    /// Persona encargada de la gestión del camping que puede acceder a la aplicación.
    /// </summary>
    public class Usuario
    {
        public string Identificacion { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Rol { get; set; }

        /// <summary>Ruta pack de la foto de perfil, o null si no tiene.</summary>
        public string RutaFoto { get; set; }

        /// <summary>Fecha del último acceso registrado, o null si nunca ha entrado.</summary>
        public DateTime? UltimoAcceso { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellidos}";
    }
}
