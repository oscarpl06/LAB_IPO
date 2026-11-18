using System.Collections.Generic;

namespace LAB_IPO.Datos
{
    /// <summary>Tema del Centro de ayuda (PG14): grupo del índice, título y pasos a seguir.</summary>
    public class TemaAyuda
    {
        public string Clave { get; set; }
        public string Grupo { get; set; }
        public string Titulo { get; set; }
        public string Encabezado { get; set; }
        public string Introduccion { get; set; }
        public string[] Pasos { get; set; }
    }

    /// <summary>Contenido de la ayuda general (RF-34). Los grupos siguen el índice del boceto PG14.</summary>
    public static class TemasAyuda
    {
        public const string CrearReserva = "reserva-crear";
        public const string ModificarReserva = "reserva-modificar";
        public const string AltaBajaPersonal = "personal-alta";
        public const string DisponibilidadPersonal = "personal-disponibilidad";
        public const string HorariosCupos = "actividades-horarios";
        public const string MonitoresActividad = "actividades-monitores";
        public const string EstadosDisponibilidad = "alojamientos-estados";
        public const string ParcelasBungalows = "alojamientos-gestion";
        public const string GaleriaFotos = "alojamientos-galeria";
        public const string Sesion = "cuenta-sesion";

        public static IReadOnlyList<TemaAyuda> Todos { get; } = new List<TemaAyuda>
        {
            new TemaAyuda
            {
                Clave = CrearReserva, Grupo = "Reservas", Titulo = "Crear una reserva",
                Encabezado = "Cómo crear una reserva",
                Introduccion = "Sigue estos pasos para registrar una estancia:",
                Pasos = new[]
                {
                    "Pulsa «Nueva reserva» desde el listado de reservas (o desde una casilla libre de Disponibilidad).",
                    "Introduce las fechas de entrada y salida.",
                    "Selecciona tipo y alojamiento. Usa «Ver disponibilidad»: si no está libre, te propondrá alternativas.",
                    "Completa los datos de contacto y el número de ocupantes.",
                    "Añade solicitudes especiales si las hubiera.",
                    "Revisa los avisos y pulsa «Guardar reserva»."
                }
            },
            new TemaAyuda
            {
                Clave = ModificarReserva, Grupo = "Reservas", Titulo = "Modificar / cancelar",
                Encabezado = "Cómo modificar o cancelar una reserva",
                Introduccion = "Desde el listado de reservas puedes cambiar una reserva o anularla:",
                Pasos = new[]
                {
                    "Busca la reserva por titular, localizador o teléfono, o fíltrala por estado y fechas.",
                    "Pulsa «Editar» para cambiar fechas, alojamiento, ocupantes o contacto, y guarda los cambios.",
                    "Si el cliente ya está alojado, solo se pueden cambiar la salida, los ocupantes y el contacto.",
                    "Pulsa «Cancelar» para anularla: el alojamiento vuelve a quedar libre en esas fechas.",
                    "Las reservas en estancia, terminadas o canceladas no se pueden cancelar."
                }
            },
            new TemaAyuda
            {
                Clave = AltaBajaPersonal, Grupo = "Empleados y monitores", Titulo = "Alta y baja de personal",
                Encabezado = "Cómo dar de alta o de baja a un empleado",
                Introduccion = "La sección Personal reúne a los empleados temporales, guías y monitores:",
                Pasos = new[]
                {
                    "Pulsa «Nuevo empleado» y rellena nombre, apellidos, teléfono y correo (obligatorios).",
                    "Añade idiomas, formación, disponibilidad y, si quieres, una foto.",
                    "Para cambiar sus datos, pulsa «Editar» en su fila o en su ficha.",
                    "Para darlo de baja, pulsa «Dar de baja»: dejará de ser monitor de sus actividades.",
                    "Los empleados de baja se consultan con el filtro «De baja»; sus datos se conservan."
                }
            },
            new TemaAyuda
            {
                Clave = DisponibilidadPersonal, Grupo = "Empleados y monitores", Titulo = "Disponibilidad",
                Encabezado = "Disponibilidad del personal",
                Introduccion = "Indica cuándo puede trabajar cada empleado para organizar las actividades:",
                Pasos = new[]
                {
                    "En la ficha del empleado elige su disponibilidad: completa, mañanas, tardes o fines de semana.",
                    "Anota en «Días no disponibles» las fechas en que no puede trabajar.",
                    "Usa «Restricciones» para vacaciones, incompatibilidades o tareas que no puede hacer.",
                    "Al asignar monitores a una actividad verás la disponibilidad de cada empleado."
                }
            },
            new TemaAyuda
            {
                Clave = HorariosCupos, Grupo = "Actividades", Titulo = "Horarios y cupos",
                Encabezado = "Cómo programar una actividad",
                Introduccion = "Cada actividad tiene sus días, su horario y sus plazas:",
                Pasos = new[]
                {
                    "Pulsa «Nueva actividad» e indica el nombre y el público (niños, adultos o familiar).",
                    "Marca los días con los botones L-D o con los atajos «De lunes a viernes», «Fin de semana» o «Todos».",
                    "Escribe la hora de inicio y de fin con el formato hh:mm (entre las 07:00 y las 23:00).",
                    "El cupo máximo es obligatorio; el mínimo es opcional y no puede superarlo.",
                    "El precio es opcional (vacío = gratuita) y puede ser por persona, por hora o por mes."
                }
            },
            new TemaAyuda
            {
                Clave = MonitoresActividad, Grupo = "Actividades", Titulo = "Monitores de una actividad",
                Encabezado = "Cómo asignar monitores a una actividad",
                Introduccion = "Los monitores responsables se gestionan desde la ficha de la actividad:",
                Pasos = new[]
                {
                    "Pulsa «Ver» en la actividad para abrir su ficha.",
                    "En la sección «Monitores responsables», pulsa «Gestionar monitores».",
                    "Marca los empleados que la llevarán y desmarca los que quieras quitar.",
                    "Pulsa «Guardar»: la ficha y el listado se actualizan."
                }
            },
            new TemaAyuda
            {
                Clave = EstadosDisponibilidad, Grupo = "Alojamientos", Titulo = "Estados y disponibilidad",
                Encabezado = "Estados y disponibilidad de los alojamientos",
                Introduccion = "Cada parcela o bungalow está libre, ocupado, en reparación o pendiente de limpieza:",
                Pasos = new[]
                {
                    "En Alojamientos pulsa «Disponibilidad» para ver el calendario de los próximos 14 días.",
                    "Usa las flechas para cambiar de semana y «Desde / Hasta» para consultar un intervalo.",
                    "Pulsa una casilla para ver el alojamiento y la fecha en el panel de la derecha.",
                    "En la casilla de hoy puedes cambiar el estado con «Aplicar estado».",
                    "Desde una casilla libre puedes crear directamente una reserva."
                }
            },
            new TemaAyuda
            {
                Clave = ParcelasBungalows, Grupo = "Alojamientos", Titulo = "Parcelas y bungalows",
                Encabezado = "Cómo gestionar parcelas y bungalows",
                Introduccion = "Las pestañas Parcelas y Bungalows muestran los alojamientos del complejo:",
                Pasos = new[]
                {
                    "Filtra por tipo, capacidad o estado y pulsa «Ver» para abrir la ficha.",
                    "Pulsa «Nueva parcela» o «Nuevo bungalow» para dar de alta un alojamiento.",
                    "Pulsa «Editar» para cambiar sus datos; el identificador o el nombre no se pueden cambiar.",
                    "Pulsa «Eliminar» para darlo de baja: no se permite si tiene reservas que no han terminado."
                }
            },
            new TemaAyuda
            {
                Clave = GaleriaFotos, Grupo = "Alojamientos", Titulo = "Galería de fotos",
                Encabezado = "Cómo gestionar la galería de un bungalow",
                Introduccion = "Cada bungalow tiene su galería de fotos:",
                Pasos = new[]
                {
                    "Abre la ficha del bungalow y pulsa «Galería» (o pulsa su foto).",
                    "Pasa de foto con las flechas o pulsando una miniatura.",
                    "Pulsa «Añadir foto» para elegir una o varias imágenes JPG o PNG.",
                    "Pulsa «Eliminar foto» para quitar la que se ve en grande; la primera es la portada."
                }
            },
            new TemaAyuda
            {
                Clave = Sesion, Grupo = "Cuenta y aplicación", Titulo = "Sesión y atajos",
                Encabezado = "Sesión, ayuda y atajos de teclado",
                Introduccion = "Algunas ayudas para moverte por la aplicación:",
                Pasos = new[]
                {
                    "Pulsa F1 en cualquier pantalla para abrir la ayuda de esa pantalla.",
                    "Pasa el cursor sobre un campo o un botón para ver su ayuda contextual.",
                    "Pulsa Esc para cerrar una ficha lateral.",
                    "Pulsa «Cerrar sesión» para volver a la pantalla de acceso sin cerrar la aplicación."
                }
            }
        };
    }
}
