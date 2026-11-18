using System.Linq;

namespace LAB_IPO.Modelos
{
    /// <summary>
    /// Empleado temporal (guía o monitor) contratado para la temporada:
    /// datos personales, foto, idiomas, formación y restricciones de disponibilidad (RF-22, RF-24).
    /// </summary>
    public class Empleado
    {
        public string Nombre { get; set; }
        public string Apellidos { get; set; }

        /// <summary>Uno o varios teléfonos separados por comas.</summary>
        public string Telefonos { get; set; }

        /// <summary>Uno o varios correos separados por comas.</summary>
        public string Correos { get; set; }

        public string Idiomas { get; set; }
        public string Formacion { get; set; }

        /// <summary>Completa, Mañanas, Tardes o Fines de semana.</summary>
        public string Disponibilidad { get; set; } = "Completa";

        public string DiasNoDisponibles { get; set; }
        public string Restricciones { get; set; }

        /// <summary>Ruta de la foto (pack:// o archivo), o null para mostrar las iniciales.</summary>
        public string RutaFoto { get; set; }

        /// <summary>False cuando se da de baja (CU-21).</summary>
        public bool Activo { get; set; } = true;

        public string NombreCompleto => $"{Nombre} {Apellidos}";

        public string IdiomasTexto => string.IsNullOrWhiteSpace(Idiomas) ? "—" : Idiomas;

        // Textos para la ficha (CU-22): lista separada por líneas y valores por defecto.
        public string TelefonosLista => Lista(Telefonos);
        public string CorreosLista => Lista(Correos);
        public string FormacionTexto => Vacio(Formacion, "Sin formación registrada");
        public string DiasNoDisponiblesTexto => Vacio(DiasNoDisponibles, "Ninguno");
        public string RestriccionesTexto => Vacio(Restricciones, "Sin restricciones registradas");
        public string EstadoTexto => Activo ? "Activo" : "De baja";

        public string AyudaBaja => Activo
            ? "Dar de baja al empleado (se pedirá confirmación)"
            : "El empleado ya está de baja";

        private static string Lista(string texto) =>
            string.Join("\n", (texto ?? "").Split(',')
                .Select(t => t.Trim()).Where(t => t.Length > 0));

        private static string Vacio(string texto, string porDefecto) =>
            string.IsNullOrWhiteSpace(texto) ? porDefecto : texto.Trim();
    }
}
