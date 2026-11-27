using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;
using LAB_IPO.Servicios;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG2 · Pantalla de inicio: resumen del día del camping. Muestra las cifras de hoy
    /// (llegadas, salidas, ocupación y alojamientos por atender), las listas de llegadas,
    /// salidas y actividades del día y accesos directos a las tareas más frecuentes.
    /// </summary>
    public partial class InicioVista : VistaBase
    {
        private static readonly CultureInfo Es = new CultureInfo("es-ES");

        public InicioVista()
        {
            InitializeComponent();
            MostrarSaludo();
            MostrarResumen();
        }

        private void MostrarSaludo()
        {
            var hoy = DateTime.Today.ToString("dddd, d 'de' MMMM 'de' yyyy", Es);
            var nombre = Sesion.UsuarioActual?.Nombre;
            var saludo = string.IsNullOrWhiteSpace(nombre) ? "Hola" : $"Hola, {nombre}";
            TxtBienvenida.Text = $"{saludo}. Hoy es {hoy}. Este es el resumen del día en el camping.";
        }

        private void MostrarResumen()
        {
            var hoy = DateTime.Today;
            var activas = RepositorioReservas.Reservas.Where(r => !r.Cancelada).ToList();

            // Llegadas y salidas
            var llegadas = activas.Where(r => r.Entrada.Date == hoy).OrderBy(r => r.Titular, StringComparer.Create(Es, true)).ToList();
            var salidas = activas.Where(r => r.Salida.Date == hoy).OrderBy(r => r.Titular, StringComparer.Create(Es, true)).ToList();

            ListaLlegadas.ItemsSource = llegadas;
            TxtSinLlegadas.Visibility = llegadas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            TxtNumLlegadas.Text = llegadas.Count.ToString();
            int sinConfirmar = llegadas.Count(r => r.EstadoRegistrado == EstadoReserva.Pendiente);
            TxtDetalleLlegadas.Text = llegadas.Count == 0 ? "Sin llegadas previstas"
                : sinConfirmar == 0 ? "Todas confirmadas"
                : sinConfirmar == 1 ? "1 sin confirmar"
                : $"{sinConfirmar} sin confirmar";

            ListaSalidas.ItemsSource = salidas;
            TxtSinSalidas.Visibility = salidas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            TxtNumSalidas.Text = salidas.Count.ToString();
            int ocupantesSalen = salidas.Sum(r => r.Ocupantes);
            TxtDetalleSalidas.Text = salidas.Count == 0 ? "Sin salidas previstas"
                : ocupantesSalen == 1 ? "1 huésped deja el camping"
                : $"{ocupantesSalen} huéspedes dejan el camping";

            // Ocupación de esta noche
            var alojamientos = RepositorioReservas.TodosLosAlojamientos().ToList();
            int total = alojamientos.Count;
            int ocupados = alojamientos.Count(a => RepositorioReservas.EstadoEn(a, hoy).Estado == EstadoAlojamiento.Ocupado);
            int porcentaje = total == 0 ? 0 : (int)Math.Round(100.0 * ocupados / total);

            RunOcupados.Text = ocupados.ToString();
            RunTotal.Text = $" / {total}";
            ColOcupados.Width = new GridLength(ocupados, GridUnitType.Star);
            ColLibres.Width = new GridLength(Math.Max(total - ocupados, 0), GridUnitType.Star);
            TxtDetalleOcupacion.Text = total == 0 ? "No hay alojamientos registrados"
                : $"{porcentaje} % ocupado · {total - ocupados} libres";

            // Alojamientos en reparación o pendientes de limpieza
            int reparacion = alojamientos.Count(a => Alojamientos.Estado(a) == EstadoAlojamiento.EnReparacion);
            int limpieza = alojamientos.Count(a => Alojamientos.Estado(a) == EstadoAlojamiento.PendienteLimpieza);
            int atencion = reparacion + limpieza;

            TxtNumAtencion.Text = atencion.ToString();
            if (atencion == 0)
            {
                TxtDetalleAtencion.Text = "Todo en orden";
                IconoAtencion.Text = (string)FindResource("Icono.Correcto");
            }
            else
            {
                var partes = new[]
                {
                    reparacion > 0 ? $"{reparacion} en reparación" : null,
                    limpieza > 0 ? $"{limpieza} por limpiar" : null
                };
                TxtDetalleAtencion.Text = string.Join(" · ", partes.Where(p => p != null));
                IconoAtencion.Text = (string)FindResource("Icono.Aviso");
                IconoAtencion.Foreground = (System.Windows.Media.Brush)FindResource("Pincel.Aviso");
                FondoIconoAtencion.Background = (System.Windows.Media.Brush)FindResource("Pincel.AvisoFondo");
            }

            // Actividades programadas para hoy
            var actividades = RepositorioActividades.Actividades
                .Where(a => a.Dias.Contains(hoy.DayOfWeek))
                .OrderBy(a => a.HoraInicio)
                .ToList();
            ListaActividades.ItemsSource = actividades;
            TxtSinActividades.Visibility = actividades.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // ===== Fichas en el panel lateral =====

        private void VerReserva_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Reserva reserva)
                Navegador?.AbrirPanelLateral(new FichaReserva(reserva));
        }

        private void VerActividad_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Actividad actividad)) return;

            var ficha = new FichaActividad(actividad);
            ficha.MonitoresCambiados += (s, a) => ListaActividades.Items.Refresh();
            Navegador?.AbrirPanelLateral(ficha);
        }

        // ===== Navegación desde las tarjetas y los accesos rápidos =====

        private void IrReservas_Click(object sender, RoutedEventArgs e) => Navegar(new ReservasVista());

        private void IrDisponibilidad_Click(object sender, RoutedEventArgs e) => Navegar(new DisponibilidadVista());

        private void IrAlojamientos_Click(object sender, RoutedEventArgs e) => Navegar(new AlojamientosVista());

        private void NuevaReserva_Click(object sender, RoutedEventArgs e) => Navegar(new FormularioReservaVista());

        /// <summary>Abre el listado de reservas con el cursor ya en el cuadro de búsqueda.</summary>
        private void BuscarReserva_Click(object sender, RoutedEventArgs e)
        {
            var reservas = new ReservasVista();
            reservas.Loaded += (s, _) => Keyboard.Focus(reservas.TxtBuscar);
            Navegar(reservas);
        }
    }
}
