using System;
using System.Collections.ObjectModel;
using System.Linq;
using LAB_IPO.Modelos;

namespace LAB_IPO.Datos
{
    /// <summary>Actividades y talleres de prueba en memoria (boceto PG13a).</summary>
    public static class RepositorioActividades
    {
        public static string[] Publicos { get; } = { "Niños", "Adultos", "Familiar" };

        public static string[] Areas { get; } =
        {
            "Piscina", "Pradera", "Playa", "Monte", "Zona norte", "Sala multiusos", "Zona infantil"
        };

        public static string[] UnidadesPrecio { get; } = { "por persona", "por hora", "por mes" };

        private static readonly DayOfWeek L = DayOfWeek.Monday, M = DayOfWeek.Tuesday, X = DayOfWeek.Wednesday,
            J = DayOfWeek.Thursday, V = DayOfWeek.Friday, S = DayOfWeek.Saturday, D = DayOfWeek.Sunday;

        public static ObservableCollection<Actividad> Actividades { get; } = new ObservableCollection<Actividad>
        {
            new Actividad
            {
                Nombre = "Ruta de senderismo", Publico = "Adultos", Area = "Monte",
                Descripcion = "Ruta guiada de unos 6 km por los senderos del monte, con paradas para conocer la flora local.",
                Dias = { M, J, S }, HoraInicio = H(9, 0), HoraFin = H(10, 30), CupoMinimo = 4, CupoMaximo = 12,
                Precio = 6, UnidadPrecio = "por persona",
                Materiales = "Calzado de montaña, agua y gorra.",
                Monitores = { Monitor("Javier", "Martín") }
            },
            new Actividad
            {
                Nombre = "Juegos de agua", Publico = "Niños", Area = "Piscina",
                Descripcion = "Juegos y competiciones en la piscina pequeña para niños de 5 a 11 años.",
                Dias = { L, M, X, J, V }, HoraInicio = H(11, 0), HoraFin = H(12, 0), CupoMinimo = 6, CupoMaximo = 15,
                Materiales = "Bañador, toalla y crema solar.",
                Monitores = { Monitor("Lucía", "Fernández") }
            },
            new Actividad
            {
                Nombre = "Yoga al aire libre", Publico = "Adultos", Area = "Pradera",
                Descripcion = "Sesión de yoga suave al amanecer, apta para todos los niveles.",
                Dias = { L, X, V }, HoraInicio = H(8, 0), HoraFin = H(9, 0), CupoMinimo = 3, CupoMaximo = 10,
                Precio = 5, UnidadPrecio = "por persona",
                Materiales = "Esterilla (el camping presta algunas) y ropa cómoda.",
                Monitores = { Monitor("Ana", "López") }
            },
            new Actividad
            {
                Nombre = "Taller de naturaleza", Publico = "Niños", Area = "Zona norte",
                Descripcion = "Reconocimiento de hojas, insectos y huellas, y manualidades con materiales del bosque.",
                Dias = { M, J }, HoraInicio = H(16, 0), HoraFin = H(17, 0), CupoMinimo = 5, CupoMaximo = 12,
                Materiales = "Lupas, cuadernos de campo y material didáctico (los pone el camping).",
                Monitores = { Monitor("Elena", "Díaz") }
            },
            new Actividad
            {
                Nombre = "Iniciación al surf", Publico = "Familiar", Area = "Playa",
                Descripcion = "Clase de iniciación al surf para mayores de 12 años, con tabla y neopreno incluidos.",
                Dias = { S, D }, HoraInicio = H(17, 30), HoraFin = H(19, 0), CupoMinimo = 4, CupoMaximo = 8,
                Precio = 18, UnidadPrecio = "por hora",
                Materiales = "Tablas, neoprenos y botiquín.",
                Notas = "Se suspende si hay bandera roja.",
                Monitores = { Monitor("Javier", "Martín") }
            }
        };

        private static TimeSpan H(int horas, int minutos) => new TimeSpan(horas, minutos, 0);

        private static Empleado Monitor(string nombre, string apellidos) =>
            RepositorioPersonal.Empleados.First(e => e.Nombre == nombre && e.Apellidos == apellidos);

        public static bool Existe(string nombre) =>
            Actividades.Any(a => string.Equals(a.Nombre?.Trim(), nombre?.Trim(), StringComparison.OrdinalIgnoreCase));

        public static void Agregar(Actividad actividad)
        {
            if (actividad == null) throw new ArgumentNullException(nameof(actividad));
            if (Existe(actividad.Nombre))
                throw new InvalidOperationException($"Ya existe una actividad llamada {actividad.Nombre}.");
            Actividades.Add(actividad);
        }
    }
}
