using System;

namespace LAB_IPO.Modelos
{
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
