using System;
using System.Collections.ObjectModel;
using System.Linq;
using LAB_IPO.Modelos;

namespace LAB_IPO.Datos
{
    /// <summary>
    /// Empleados temporales de ejemplo (PG11 y los monitores de PG13), en memoria.
    /// Óscar Puebla y Carlos López no llevan foto: se muestran con sus iniciales.
    /// </summary>
    public static class RepositorioPersonal
    {
        private const string RutaFotos = "pack://application:,,,/LAB_IPO;component/Recursos/Fotos/Empleados/";

        /// <summary>Opciones de disponibilidad que se pueden asignar a un empleado.</summary>
        public static string[] OpcionesDisponibilidad { get; } = { "Completa", "Mañanas", "Tardes", "Fines de semana" };

        public static ObservableCollection<Empleado> Empleados { get; } = new ObservableCollection<Empleado>
        {
            new Empleado
            {
                Nombre = "Jorge", Apellidos = "Sánchez", Telefonos = "600 111 222", Correos = "jorge@mail.es",
                Idiomas = "Español, Inglés", Formacion = "Técnico en guía de actividades en el medio natural",
                Disponibilidad = "Completa", RutaFoto = RutaFotos + "jorge-sanchez.jpg"
            },
            new Empleado
            {
                Nombre = "Óscar", Apellidos = "Puebla", Telefonos = "600 333 444", Correos = "oscar@mail.es",
                Idiomas = "Español, Francés", Formacion = "Monitor de ocio y tiempo libre",
                Disponibilidad = "Tardes"
            },
            new Empleado
            {
                Nombre = "Fernando", Apellidos = "Izquierdo", Telefonos = "600 555 666", Correos = "fernando@mail.es",
                Idiomas = "Español, Inglés", Formacion = "Socorrista acuático",
                Disponibilidad = "Fines de semana", RutaFoto = RutaFotos + "fernando-izquierdo.jpg"
            },
            new Empleado
            {
                Nombre = "Diego", Apellidos = "Pérez", Telefonos = "600 777 888", Correos = "diego@mail.es",
                Idiomas = "Español", Formacion = "Técnico deportivo en media montaña",
                Disponibilidad = "Completa", RutaFoto = RutaFotos + "diego-perez.jpg"
            },
            new Empleado
            {
                Nombre = "Carlos", Apellidos = "López", Telefonos = "600 999 000", Correos = "carlos@mail.es",
                Idiomas = "Español, Alemán", Formacion = "Animador sociocultural",
                Disponibilidad = "Mañanas"
            },
            new Empleado
            {
                Nombre = "Lucía", Apellidos = "Fernández", Telefonos = "611 222 333", Correos = "lucia@mail.es",
                Idiomas = "Español, Inglés C1", Formacion = "Grado en Educación Infantil; monitora de actividades acuáticas",
                Disponibilidad = "Completa", RutaFoto = RutaFotos + "lucia-fernandez.jpg"
            },
            new Empleado
            {
                Nombre = "Javier", Apellidos = "Martín", Telefonos = "611 444 555", Correos = "javier@mail.es",
                Idiomas = "Español, Inglés", Formacion = "Guía de senderismo homologado; primeros auxilios",
                Disponibilidad = "Mañanas", RutaFoto = RutaFotos + "javier-martin.jpg"
            },
            new Empleado
            {
                Nombre = "Ana", Apellidos = "López", Telefonos = "611 666 777", Correos = "ana@mail.es",
                Idiomas = "Español, Italiano", Formacion = "Instructora de yoga (200 h)",
                Disponibilidad = "Mañanas", RutaFoto = RutaFotos + "ana-lopez.jpg"
            },
            new Empleado
            {
                Nombre = "Elena", Apellidos = "Díaz", Telefonos = "611 888 999", Correos = "elena@mail.es",
                Idiomas = "Español, Inglés", Formacion = "Técnica superior en educación infantil",
                Disponibilidad = "Tardes", DiasNoDisponibles = "Domingos", RutaFoto = RutaFotos + "elena-diaz.jpg"
            }
        };

        /// <summary>True si algún empleado ya usa alguno de esos correos.</summary>
        public static bool ExisteCorreo(string correo) =>
            Empleados.Any(e => (e.Correos ?? "")
                .Split(',')
                .Any(c => string.Equals(c.Trim(), correo?.Trim(), StringComparison.OrdinalIgnoreCase)));

        public static void AgregarEmpleado(Empleado empleado)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));
            Empleados.Add(empleado);
        }
    }
}
