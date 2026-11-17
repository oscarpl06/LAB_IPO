using System.Collections.Generic;

namespace LAB_IPO.Modelos
{
    /// <summary>
    /// Parcela del camping: identificador, tipo, tamaño, ubicación,
    /// servicios y características (RF-01, RF-04, RF-05).
    /// </summary>
    public class Parcela
    {
        public string Identificador { get; set; }
        public TipoParcela Tipo { get; set; }
        public int TamanoM2 { get; set; }
        public string Ubicacion { get; set; }

        public bool TomaAgua { get; set; }
        public bool TomaLuz { get; set; }
        public bool Desague { get; set; }
        public bool DelimitadaSetos { get; set; }
        public bool ZonaSombra { get; set; }

        public EstadoAlojamiento Estado { get; set; } = EstadoAlojamiento.Libre;

        /// <summary>Ruta de la foto (pack:// de los recursos o ruta de un archivo), o null.</summary>
        public string RutaFoto { get; set; }

        public string TamanoTexto => $"{TamanoM2} m²";

        /// <summary>Los cinco servicios y características, indicando si la parcela los tiene (RF-05).</summary>
        public IEnumerable<ServicioParcela> Servicios => new[]
        {
            new ServicioParcela("Toma de agua", TomaAgua),
            new ServicioParcela("Toma de luz", TomaLuz),
            new ServicioParcela("Desagüe", Desague),
            new ServicioParcela("Delimitada por setos", DelimitadaSetos),
            new ServicioParcela("Zona de sombra", ZonaSombra)
        };

        public string ServiciosTexto
        {
            get
            {
                var lista = new List<string>();
                if (TomaAgua) lista.Add("Agua");
                if (TomaLuz) lista.Add("luz");
                if (Desague) lista.Add("desagüe");
                if (DelimitadaSetos) lista.Add("setos");
                if (ZonaSombra) lista.Add("sombra");
                if (lista.Count == 0) return "Sin servicios";
                lista[0] = char.ToUpper(lista[0][0]) + lista[0].Substring(1);
                return string.Join(", ", lista);
            }
        }

        /// <summary>Texto del estado en femenino, como aparece en el listado de parcelas.</summary>
        public string EstadoTexto
        {
            get
            {
                switch (Estado)
                {
                    case EstadoAlojamiento.Ocupado: return "Ocupada";
                    case EstadoAlojamiento.EnReparacion: return "En reparación";
                    case EstadoAlojamiento.PendienteLimpieza: return "Pend. limpieza";
                    default: return "Libre";
                }
            }
        }
    }
}

namespace LAB_IPO.Modelos
{
    /// <summary>Un servicio o característica de una parcela y si está disponible.</summary>
    public class ServicioParcela
    {
        public ServicioParcela(string nombre, bool disponible)
        {
            Nombre = nombre;
            Disponible = disponible;
        }

        public string Nombre { get; }
        public bool Disponible { get; }
    }
}
