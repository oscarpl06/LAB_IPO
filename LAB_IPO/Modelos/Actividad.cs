using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LAB_IPO.Modelos
{
    /// <summary>
    /// Actividad o taller del camping (RF-26): horario, cupos, destinatarios, precio,
    /// área, descripción, materiales y monitores responsables.
    /// </summary>
    public class Actividad
    {
        private static readonly CultureInfo Es = new CultureInfo("es-ES");

        /// <summary>Días de la semana en el orden en que se muestran (empezando en lunes).</summary>
        public static readonly DayOfWeek[] Semana =
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
            DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

        private static readonly string[] Abreviaturas = { "Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb" };

        public string Nombre { get; set; }

        /// <summary>Niños, Adultos o Familiar.</summary>
        public string Publico { get; set; }

        public string Descripcion { get; set; }

        public List<DayOfWeek> Dias { get; set; } = new List<DayOfWeek>();
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        public int? CupoMinimo { get; set; }
        public int CupoMaximo { get; set; }

        /// <summary>Precio en euros, o null si la actividad es gratuita.</summary>
        public decimal? Precio { get; set; }

        /// <summary>por persona, por hora o por mes.</summary>
        public string UnidadPrecio { get; set; }

        public string Area { get; set; }
        public string Materiales { get; set; }
        public string Notas { get; set; }

        /// <summary>Monitores responsables (se asignan en el CU-28).</summary>
        public List<Empleado> Monitores { get; set; } = new List<Empleado>();

        // ===== Textos para el listado y la ficha =====

        public string HorarioTexto => $"{HoraInicio:hh\\:mm}-{HoraFin:hh\\:mm}";

        public string DiasTexto
        {
            get
            {
                var dias = Semana.Where(Dias.Contains).ToList();
                if (dias.Count == 7) return "Todos los días";
                if (dias.Count == 5 && !dias.Contains(DayOfWeek.Saturday) && !dias.Contains(DayOfWeek.Sunday))
                    return "De lunes a viernes";
                if (dias.Count == 2 && dias.Contains(DayOfWeek.Saturday) && dias.Contains(DayOfWeek.Sunday))
                    return "Fines de semana";
                return string.Join(", ", dias.Select(d => Abreviaturas[(int)d]));
            }
        }

        public string CupoTexto => CupoMinimo.HasValue ? $"{CupoMinimo}-{CupoMaximo}" : $"Hasta {CupoMaximo}";

        public string PrecioTexto => Precio.HasValue && Precio.Value > 0
            ? Precio.Value.ToString(Precio.Value % 1 == 0 ? "0" : "0.00", Es) + " € " + UnidadPrecio
            : "Gratuita";

        /// <summary>"J. Martín, L. Fernández" o "Sin asignar".</summary>
        public string MonitoresTexto => Monitores.Count == 0
            ? "Sin asignar"
            : string.Join(", ", Monitores.Select(m => $"{m.Nombre?.FirstOrDefault()}. {m.Apellidos}"));

        public bool TieneMonitores => Monitores.Count > 0;

        // ===== Textos para la ficha (CU-27) =====

        public string DiasCompletosTexto
        {
            get
            {
                var dias = Semana.Where(Dias.Contains)
                    .Select(d => Es.DateTimeFormat.GetDayName(d)).ToList();
                if (dias.Count == 7) return "Todos los días";
                if (dias.Count == 1) return Mayuscula(dias[0]);
                return Mayuscula(string.Join(", ", dias.Take(dias.Count - 1)) + " y " + dias.Last());
            }
        }

        public string DuracionTexto
        {
            get
            {
                var duracion = HoraFin - HoraInicio;
                if (duracion.TotalMinutes < 60) return $"{duracion.TotalMinutes:0} min";
                return duracion.Minutes == 0 ? $"{duracion.Hours} h" : $"{duracion.Hours} h {duracion.Minutes} min";
            }
        }

        public string CupoMinimoTexto => CupoMinimo.HasValue
            ? (CupoMinimo == 1 ? "1 participante" : $"{CupoMinimo} participantes")
            : "Sin mínimo";
        public string CupoMaximoTexto => CupoMaximo == 1 ? "1 participante" : $"{CupoMaximo} participantes";

        public string DescripcionTexto => string.IsNullOrWhiteSpace(Descripcion) ? "Sin descripción." : Descripcion;
        public string MaterialesTexto => string.IsNullOrWhiteSpace(Materiales) ? "No se necesita material." : Materiales;
        public string NotasTexto => string.IsNullOrWhiteSpace(Notas) ? "—" : Notas;

        private static string Mayuscula(string texto) =>
            string.IsNullOrEmpty(texto) ? texto : char.ToUpper(texto[0]) + texto.Substring(1);
    }
}
