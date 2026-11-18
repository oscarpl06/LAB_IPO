using System;
using System.Linq;

namespace LAB_IPO.Modelos
{
    /// <summary>Estado de una reserva (boceto PG10).</summary>
    public enum EstadoReserva
    {
        Pendiente,
        Confirmada,
        EnEstancia,
        Cancelada
    }

    /// <summary>
    /// Reserva de una parcela o un bungalow (RF-14). La estancia ocupa las noches
    /// desde la entrada hasta el día anterior a la salida.
    /// </summary>
    public class Reserva
    {
        public string Localizador { get; set; }
        public string Titular { get; set; }

        /// <summary>Parcela o Bungalow reservado.</summary>
        public object Alojamiento { get; set; }

        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }

        /// <summary>Franja horaria estimada de llegada y de salida (RF-15).</summary>
        public string HoraLlegada { get; set; }
        public string HoraSalida { get; set; }

        /// <summary>Uno o más teléfonos separados por comas; e-mail y documento opcionales (RF-16).</summary>
        public string Telefonos { get; set; }
        public string Correo { get; set; }
        public string Documento { get; set; }

        public int Adultos { get; set; }
        public int Ninos { get; set; }
        public int Ocupantes => Adultos + Ninos;

        public string Solicitudes { get; set; }

        public int Noches => (int)(Salida.Date - Entrada.Date).TotalDays;

        /// <summary>Estado guardado: Pendiente, Confirmada o Cancelada. Las reservas nuevas empiezan pendientes.</summary>
        public EstadoReserva EstadoRegistrado { get; set; } = EstadoReserva.Pendiente;

        public bool Cancelada => EstadoRegistrado == EstadoReserva.Cancelada;

        /// <summary>Estado que se muestra: "En estancia" mientras el cliente está en el camping.</summary>
        public EstadoReserva Estado =>
            Cancelada ? EstadoReserva.Cancelada
            : OcupaNoche(DateTime.Today) ? EstadoReserva.EnEstancia
            : EstadoRegistrado;

        public string EstadoTexto
        {
            get
            {
                switch (Estado)
                {
                    case EstadoReserva.Confirmada: return "Confirmada";
                    case EstadoReserva.EnEstancia: return "En estancia";
                    case EstadoReserva.Cancelada: return "Cancelada";
                    default: return "Pendiente";
                }
            }
        }

        /// <summary>Todavía no ha llegado (ni se ha cancelado).</summary>
        public bool PendienteDeLlegada => !Cancelada && Entrada.Date >= DateTime.Today && Estado != EstadoReserva.EnEstancia;

        public string EntradaSalidaTexto => $"{Entrada:dd/MM} - {Salida:dd/MM}";
        public string NochesTexto => Noches == 1 ? "1 noche" : $"{Noches} noches";

        public string OcupantesTexto
        {
            get
            {
                string adultos = Adultos == 1 ? "1 adulto" : $"{Adultos} adultos";
                if (Ninos == 0) return $"{Ocupantes} ({adultos})";
                string ninos = Ninos == 1 ? "1 niño" : $"{Ninos} niños";
                return $"{Ocupantes} ({adultos} y {ninos})";
            }
        }

        public string TelefonosLista => string.Join("\n", (Telefonos ?? "").Split(',')
            .Select(t => t.Trim()).Where(t => t.Length > 0));
        public string CorreoTexto => string.IsNullOrWhiteSpace(Correo) ? "No indicado" : Correo;
        public string DocumentoTexto => string.IsNullOrWhiteSpace(Documento) ? "No indicado" : Documento;
        public string SolicitudesTexto => string.IsNullOrWhiteSpace(Solicitudes) ? "Sin solicitudes especiales." : Solicitudes;

        /// <summary>Foto del alojamiento reservado (parcela o portada del bungalow).</summary>
        public string FotoAlojamiento => (Alojamiento as Parcela)?.RutaFoto ?? (Alojamiento as Bungalow)?.Portada;

        public string DetalleAlojamiento
        {
            get
            {
                switch (Alojamiento)
                {
                    case Parcela p: return $"Parcela {p.Tipo?.Nombre?.ToLowerInvariant()} · {p.TamanoTexto} · {p.Ubicacion}";
                    case Bungalow b: return $"Bungalow · {b.CapacidadTexto} · {b.PrecioTexto}/noche";
                    default: return "";
                }
            }
        }

        public string NombreAlojamiento => Alojamientos.Nombre(Alojamiento);
        public string FechasTexto => $"{Entrada:dd/MM} – {Salida:dd/MM}";

        /// <summary>Indica si el alojamiento está ocupado la noche de esa fecha.</summary>
        public bool OcupaNoche(DateTime fecha) => fecha.Date >= Entrada.Date && fecha.Date < Salida.Date;

        /// <summary>Indica si la estancia coincide con alguna noche del intervalo [desde, hasta].</summary>
        public bool CoincideCon(DateTime desde, DateTime hasta) =>
            Entrada.Date <= hasta.Date && Salida.Date > desde.Date;
    }

    /// <summary>Utilidades comunes a parcelas y bungalows.</summary>
    public static class Alojamientos
    {
        public static string Nombre(object alojamiento)
        {
            switch (alojamiento)
            {
                case Parcela p: return "Parcela " + p.Identificador;
                case Bungalow b: return b.Nombre;
                default: return "";
            }
        }

        public static EstadoAlojamiento Estado(object alojamiento)
        {
            switch (alojamiento)
            {
                case Parcela p: return p.Estado;
                case Bungalow b: return b.Estado;
                default: return EstadoAlojamiento.Libre;
            }
        }

        public static string EstadoTexto(EstadoAlojamiento estado)
        {
            switch (estado)
            {
                case EstadoAlojamiento.Ocupado: return "Ocupado";
                case EstadoAlojamiento.EnReparacion: return "En reparación";
                case EstadoAlojamiento.PendienteLimpieza: return "Pendiente de limpieza";
                default: return "Libre";
            }
        }
    }
}
