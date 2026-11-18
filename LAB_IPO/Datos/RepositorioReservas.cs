using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LAB_IPO.Modelos;

namespace LAB_IPO.Datos
{
    /// <summary>Estado de un alojamiento en un día concreto, con la reserva que lo ocupa si la hay.</summary>
    public class EstadoDia
    {
        public EstadoAlojamiento Estado { get; set; }
        public Reserva Reserva { get; set; }
        public string Texto => Alojamientos.EstadoTexto(Estado) + (Reserva != null ? " · " + Reserva.Localizador : "");
    }

    /// <summary>
    /// Reservas de prueba en memoria (boceto PG8). Las fechas se calculan a partir de hoy
    /// para que el calendario de disponibilidad siempre tenga datos que mostrar.
    /// </summary>
    public static class RepositorioReservas
    {
        private static DateTime Hoy => DateTime.Today;

        public static ObservableCollection<Reserva> Reservas { get; } = CrearReservas();

        /// <summary>Periodos previstos de reparación o limpieza que se extienden más allá de hoy.</summary>
        private static readonly List<(object Alojamiento, EstadoAlojamiento Estado, int Desde, int Hasta)> Incidencias =
            new List<(object, EstadoAlojamiento, int, int)>
            {
                (Parcela("P-15"), EstadoAlojamiento.EnReparacion, 0, 5)
            };

        private static ObservableCollection<Reserva> CrearReservas() => new ObservableCollection<Reserva>
        {
            Nueva("R-0281", "Carmen Ruiz",      Parcela("P-06"),        -2, 5),
            Nueva("R-0283", "Pablo Moreno",     Parcela("P-12"),         1, 5),
            Nueva("R-0284", "Lucía Fernández",  Bungalow("Bungalow 04"), 0, 6),
            Nueva("R-0291", "Marta Gil",        Bungalow("Bungalow 01"), -1, 3),
            Nueva("R-0295", "Andrés Navarro",   Parcela("P-03"),         7, 12),
            Nueva("R-0296", "Sofía Herrera",    Parcela("P-21"),         3, 6),
            Nueva("R-0299", "Raúl Domínguez",   Bungalow("Bungalow 07"), 2, 9),
            Nueva("R-0302", "Irene Castro",     Bungalow("Bungalow 02"), 4, 8),
            Nueva("R-0305", "Familia Ortega",   Bungalow("Bungalow 01"), 6, 10),
            Nueva("R-0308", "Nuria Vidal",      Parcela("P-15"),         9, 13)
        };

        private static Reserva Nueva(string localizador, string titular, object alojamiento, int entrada, int salida) =>
            new Reserva
            {
                Localizador = localizador, Titular = titular, Alojamiento = alojamiento,
                Entrada = Hoy.AddDays(entrada), Salida = Hoy.AddDays(salida)
            };

        private static Parcela Parcela(string id) =>
            RepositorioAlojamientos.Parcelas.First(p => p.Identificador == id);

        private static Bungalow Bungalow(string nombre) =>
            RepositorioAlojamientos.Bungalows.First(b => b.Nombre == nombre);

        /// <summary>Todos los alojamientos del camping: primero las parcelas y después los bungalows.</summary>
        public static IEnumerable<object> TodosLosAlojamientos() =>
            RepositorioAlojamientos.Parcelas.Cast<object>().Concat(RepositorioAlojamientos.Bungalows);

        public static bool Existe(object alojamiento) =>
            RepositorioAlojamientos.Parcelas.Contains(alojamiento as Parcela)
            || RepositorioAlojamientos.Bungalows.Contains(alojamiento as Bungalow);

        /// <summary>
        /// Estado del alojamiento en una fecha (RF-13): ocupado si hay una reserva esa noche;
        /// hoy, el estado actual del alojamiento; otro día, la incidencia prevista o libre.
        /// </summary>
        public static EstadoDia EstadoEn(object alojamiento, DateTime fecha)
        {
            fecha = fecha.Date;
            var reserva = Reservas.FirstOrDefault(r => r.Alojamiento == alojamiento && r.OcupaNoche(fecha));
            if (reserva != null)
                return new EstadoDia { Estado = EstadoAlojamiento.Ocupado, Reserva = reserva };

            if (fecha == Hoy)
                return new EstadoDia { Estado = Alojamientos.Estado(alojamiento) };

            var incidencia = Incidencias.FirstOrDefault(i => i.Alojamiento == alojamiento
                && fecha >= Hoy.AddDays(i.Desde) && fecha <= Hoy.AddDays(i.Hasta));
            if (incidencia.Alojamiento != null && fecha > Hoy)
                return new EstadoDia { Estado = incidencia.Estado };

            return new EstadoDia { Estado = EstadoAlojamiento.Libre };
        }

        /// <summary>Reservas de un alojamiento que coinciden con alguna noche del intervalo.</summary>
        public static List<Reserva> ReservasEntre(object alojamiento, DateTime desde, DateTime hasta) =>
            Reservas.Where(r => r.Alojamiento == alojamiento && r.CoincideCon(desde, hasta))
                    .OrderBy(r => r.Entrada)
                    .ToList();
    }
}
