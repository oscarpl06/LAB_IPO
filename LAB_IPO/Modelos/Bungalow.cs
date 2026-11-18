using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LAB_IPO.Modelos
{
    /// <summary>
    /// Bungaló o cabaña de madera del complejo: tamaño, capacidad, precio, estancia mínima,
    /// equipamiento, descripción y galería de fotos (RF-06, RF-09).
    /// </summary>
    public class Bungalow
    {
        private static readonly CultureInfo Es = new CultureInfo("es-ES");

        public string Nombre { get; set; }
        public int TamanoM2 { get; set; }
        public int CapacidadMaxima { get; set; }
        public decimal PrecioNoche { get; set; }

        /// <summary>Noches mínimas de estancia, o null si no hay restricción.</summary>
        public int? EstanciaMinima { get; set; }

        public bool Menaje { get; set; }
        public bool Microondas { get; set; }
        public bool RopaCamaToallas { get; set; }
        public bool WiFi { get; set; }

        public string Descripcion { get; set; }

        /// <summary>Rutas de las fotos (pack:// o archivo). La primera es la portada.</summary>
        public List<string> Fotos { get; set; } = new List<string>();

        public EstadoAlojamiento Estado { get; set; } = EstadoAlojamiento.Libre;

        public string Portada => Fotos.FirstOrDefault();

        public string TamanoTexto => $"{TamanoM2} m²";

        public string CapacidadTexto => CapacidadMaxima == 1 ? "1 persona" : $"Hasta {CapacidadMaxima} personas";

        public string PrecioTexto => PrecioNoche.ToString(PrecioNoche % 1 == 0 ? "0" : "0.00", Es) + " €";

        public string EstanciaTexto => EstanciaMinima.HasValue
            ? $"Estancia mínima: {EstanciaMinima} {(EstanciaMinima == 1 ? "noche" : "noches")}"
            : "Sin estancia mínima";

        /// <summary>Estancia mínima para la ficha: "2 noches" o "Sin mínimo".</summary>
        public string EstanciaFichaTexto => EstanciaMinima.HasValue
            ? $"{EstanciaMinima} {(EstanciaMinima == 1 ? "noche" : "noches")}"
            : "Sin mínimo";

        public string DescripcionTexto => string.IsNullOrWhiteSpace(Descripcion)
            ? "Este bungalow todavía no tiene descripción."
            : Descripcion;

        /// <summary>Los cuatro elementos de equipamiento, indicando si el bungalow los tiene (RF-09).</summary>
        public IEnumerable<ServicioParcela> Equipamiento => new[]
        {
            new ServicioParcela("Menaje", Menaje),
            new ServicioParcela("Microondas", Microondas),
            new ServicioParcela("Ropa de cama y toallas", RopaCamaToallas),
            new ServicioParcela("Wi-Fi", WiFi)
        };

        public string EquipamientoTexto
        {
            get
            {
                var lista = new List<string>();
                if (Menaje) lista.Add("Menaje");
                if (Microondas) lista.Add("Microondas");
                if (RopaCamaToallas) lista.Add("Ropa de cama");
                if (WiFi) lista.Add("Wi-Fi");
                return lista.Count == 0 ? "Sin equipamiento" : string.Join(" · ", lista);
            }
        }

        public string FotosTexto => Fotos.Count == 0 ? "Sin fotos" : Fotos.Count == 1 ? "1 foto" : $"{Fotos.Count} fotos";

        /// <summary>Estado en masculino, como se dice de un bungaló.</summary>
        public string EstadoTexto
        {
            get
            {
                switch (Estado)
                {
                    case EstadoAlojamiento.Ocupado: return "Ocupado";
                    case EstadoAlojamiento.EnReparacion: return "En reparación";
                    case EstadoAlojamiento.PendienteLimpieza: return "Pend. limpieza";
                    default: return "Libre";
                }
            }
        }
    }
}
