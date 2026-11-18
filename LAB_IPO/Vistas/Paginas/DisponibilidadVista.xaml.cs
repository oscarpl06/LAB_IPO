using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// CU-14 · Consultar disponibilidad (RF-13), boceto PG8. Cuadrícula de alojamientos por día
    /// con su estado y las reservas que los ocupan, y consulta de un intervalo de fechas.
    /// </summary>
    public partial class DisponibilidadVista : VistaBase
    {
        private const int DiasVisibles = 14;
        private const int PasoDias = 7;
        private const double AnchoEtiqueta = 132;

        private static readonly CultureInfo Cultura = new CultureInfo("es-ES");

        private DateTime _inicio = DateTime.Today;
        private (DateTime Desde, DateTime Hasta)? _intervalo;

        private object _seleccionAlojamiento;
        private DateTime? _seleccionFecha;
        private readonly List<ToggleButton> _casillas = new List<ToggleButton>();

        public override string OpcionMenu => "Alojamientos";

        public DisponibilidadVista()
        {
            InitializeComponent();

            CmbTipo.ItemsSource = new[] { "Todos los alojamientos", "Parcelas", "Bungalows" };
            CmbTipo.SelectedIndex = 0;
            CmbSelEstado.ItemsSource = Enum.GetValues(typeof(EstadoAlojamiento))
                .Cast<EstadoAlojamiento>().Select(Alojamientos.EstadoTexto).ToList();

            FechaDesde.FechaCambiada += (s, e) => QuitarError();
            FechaHasta.FechaCambiada += (s, e) => QuitarError();

            Dibujar();
        }

        // ===== Filtro y navegación =====

        private void CmbTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized) return;
            Dibujar();
            if (_intervalo.HasValue) MostrarResultado();
        }

        private void Anterior_Click(object sender, RoutedEventArgs e)
        {
            _inicio = _inicio.AddDays(-PasoDias);
            Dibujar();
        }

        private void Siguiente_Click(object sender, RoutedEventArgs e)
        {
            _inicio = _inicio.AddDays(PasoDias);
            Dibujar();
        }

        private IEnumerable<object> AlojamientosFiltrados()
        {
            switch (CmbTipo.SelectedIndex)
            {
                case 1: return RepositorioAlojamientos.Parcelas;
                case 2: return RepositorioAlojamientos.Bungalows;
                default: return RepositorioReservas.TodosLosAlojamientos();
            }
        }

        // ===== Consulta por intervalo =====

        private void Consultar_Click(object sender, RoutedEventArgs e)
        {
            QuitarError();

            string error = null;
            CampoFecha campoError = null;

            if (FechaDesde.EstaVacio)
                (error, campoError) = ("Indica la fecha «Desde» del intervalo que quieres consultar.", FechaDesde);
            else if (FechaDesde.TextoNoValido)
                (error, campoError) = ("La fecha «Desde» no es válida. Escríbela con el formato dd/mm/aaaa o elígela en el calendario.", FechaDesde);
            else if (FechaHasta.EstaVacio)
                (error, campoError) = ("Indica la fecha «Hasta» del intervalo que quieres consultar.", FechaHasta);
            else if (FechaHasta.TextoNoValido)
                (error, campoError) = ("La fecha «Hasta» no es válida. Escríbela con el formato dd/mm/aaaa o elígela en el calendario.", FechaHasta);
            else if (FechaHasta.Fecha < FechaDesde.Fecha)
                (error, campoError) = ("El intervalo no es válido: la fecha «Hasta» no puede ser anterior a la fecha «Desde».", FechaHasta);

            if (error != null)
            {
                _intervalo = null;
                Dibujar();
                campoError.TieneError = true;
                MostrarMensaje(error, esError: true);
                campoError.Enfocar();
                return;
            }

            _intervalo = (FechaDesde.Fecha.Value, FechaHasta.Fecha.Value);
            _inicio = FechaDesde.Fecha.Value;
            Dibujar();
            MostrarResultado();
        }

        private void LimpiarIntervalo_Click(object sender, RoutedEventArgs e)
        {
            FechaDesde.Limpiar();
            FechaHasta.Limpiar();
            QuitarError();
            PanelResultado.Visibility = Visibility.Collapsed;
            _intervalo = null;
            Dibujar();
        }

        private void QuitarError()
        {
            FechaDesde.TieneError = false;
            FechaHasta.TieneError = false;
            if (!_intervalo.HasValue) PanelResultado.Visibility = Visibility.Collapsed;
        }

        private void MostrarResultado()
        {
            var (desde, hasta) = _intervalo.Value;
            var alojamientos = AlojamientosFiltrados().ToList();
            var conReservas = alojamientos
                .Select(a => (Alojamiento: a, Reservas: RepositorioReservas.ReservasEntre(a, desde, hasta)))
                .Where(x => x.Reservas.Count > 0)
                .ToList();
            int libres = alojamientos.Count - conReservas.Count;

            string periodo = desde == hasta
                ? $"La noche del {desde:dd/MM/yyyy}"
                : $"Del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}";

            string texto;
            if (conReservas.Count == 0)
                texto = libres == 1
                    ? $"{periodo} no hay reservas: el alojamiento está libre."
                    : $"{periodo} no hay reservas: los {libres} alojamientos están libres.";
            else
            {
                var detalle = conReservas
                    .SelectMany(x => x.Reservas)
                    .Select(r => r.Localizador);
                texto = $"{periodo}: {Plural(libres, "alojamiento sin reservas", "alojamientos sin reservas")} y "
                      + $"{Plural(conReservas.Count, "alojamiento con reservas que coinciden", "alojamientos con reservas que coinciden")}. "
                      + "Reservas que coinciden: " + string.Join(", ", detalle)
                      + ". Pasa el ratón sobre la marca de cada alojamiento para ver las suyas.";
            }

            if ((hasta - desde).TotalDays >= DiasVisibles)
                texto += $" La cuadrícula muestra los {DiasVisibles} primeros días; usa las flechas para ver el resto.";

            MostrarMensaje(texto, esError: false);
        }

        private static string Plural(int n, string singular, string plural) => n == 1 ? $"1 {singular}" : $"{n} {plural}";

        private void MostrarMensaje(string texto, bool esError)
        {
            TxtResultado.Text = texto;
            IconoResultado.Text = (string)FindResource(esError ? "Icono.Error" : "Icono.Info");
            var color = (Brush)FindResource(esError ? "Pincel.Error" : "Pincel.Bosque700");
            IconoResultado.Foreground = color;
            TxtResultado.Foreground = esError ? color : (Brush)FindResource("Pincel.TextoPrincipal");
            PanelResultado.Visibility = Visibility.Visible;
        }

        private bool EnIntervalo(DateTime dia) =>
            _intervalo.HasValue && dia >= _intervalo.Value.Desde && dia <= _intervalo.Value.Hasta;

        // ===== Cuadrícula =====

        private void Dibujar()
        {
            var fin = _inicio.AddDays(DiasVisibles - 1);
            TxtPeriodo.Text = TextoPeriodo(_inicio, fin);

            DibujarCabecera();

            Filas.Children.Clear();
            _casillas.Clear();
            var alojamientos = AlojamientosFiltrados().ToList();
            foreach (var alojamiento in alojamientos)
                Filas.Children.Add(CrearFila(alojamiento));

            TxtSinAlojamientos.Visibility = alojamientos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            // La selección se conserva solo si sigue a la vista.
            if (_seleccionAlojamiento != null && !alojamientos.Contains(_seleccionAlojamiento))
                Seleccionar(null, null);
        }

        private static string TextoPeriodo(DateTime inicio, DateTime fin)
        {
            if (inicio.Year != fin.Year)
                return $"{inicio.ToString("d 'de' MMMM 'de' yyyy", Cultura)} – {fin.ToString("d 'de' MMMM 'de' yyyy", Cultura)}";
            if (inicio.Month != fin.Month)
                return $"{inicio.ToString("d 'de' MMMM", Cultura)} – {fin.ToString("d 'de' MMMM 'de' yyyy", Cultura)}";
            return $"{inicio.Day} – {fin.ToString("d 'de' MMMM 'de' yyyy", Cultura)}";
        }

        private Grid NuevaRejilla()
        {
            var rejilla = new Grid();
            rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(AnchoEtiqueta) });
            for (int i = 0; i < DiasVisibles; i++)
                rejilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            return rejilla;
        }

        private void DibujarCabecera()
        {
            Cabecera.Children.Clear();
            Cabecera.ColumnDefinitions.Clear();
            Cabecera.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(AnchoEtiqueta) });
            for (int i = 0; i < DiasVisibles; i++)
                Cabecera.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var etiqueta = new TextBlock
            {
                Text = "Alojamiento",
                Style = (Style)FindResource("Texto.Etiqueta"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 0, 0, 0)
            };
            Cabecera.Children.Add(etiqueta);
            AgregarGuias(Cabecera);

            for (int i = 0; i < DiasVisibles; i++)
            {
                var dia = _inicio.AddDays(i);
                bool hoy = dia == DateTime.Today;
                bool finDeSemana = dia.DayOfWeek == DayOfWeek.Saturday || dia.DayOfWeek == DayOfWeek.Sunday;

                var celda = new Border
                {
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(1, 5, 1, 5),
                    Background = EnIntervalo(dia) ? (Brush)FindResource("Pincel.Salvia200") : Brushes.Transparent,
                    ToolTip = dia.ToString("dddd, d 'de' MMMM 'de' yyyy", Cultura) + (hoy ? " (hoy)" : "")
                };
                var pila = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                pila.Children.Add(new TextBlock
                {
                    Text = Cultura.DateTimeFormat.GetShortestDayName(dia.DayOfWeek).Substring(0, 1).ToUpper(),
                    FontSize = 10,
                    FontFamily = (FontFamily)FindResource("Fuente.Texto"),
                    Foreground = (Brush)FindResource(finDeSemana ? "Pincel.Bosque600" : "Pincel.TextoTenue"),
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                var numero = new TextBlock
                {
                    Text = dia.Day.ToString(),
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    FontFamily = (FontFamily)FindResource("Fuente.Texto"),
                    Foreground = (Brush)FindResource(hoy ? "Pincel.Blanco" : "Pincel.TextoPrincipal"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                pila.Children.Add(hoy
                    ? (UIElement)new Border
                    {
                        Background = (Brush)FindResource("Pincel.Bosque700"),
                        CornerRadius = new CornerRadius(9),
                        MinWidth = 24,
                        Height = 18,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Child = numero
                    }
                    : numero);

                celda.Child = pila;
                Grid.SetColumn(celda, i + 1);
                Cabecera.Children.Add(celda);
            }
        }

        private Border CrearFila(object alojamiento)
        {
            var rejilla = NuevaRejilla();
            rejilla.Height = 42;

            // Nombre del alojamiento y, si hay intervalo, si tiene reservas en él.
            var cabeza = new DockPanel { Margin = new Thickness(16, 0, 6, 0), LastChildFill = true };
            if (_intervalo.HasValue)
            {
                var reservas = RepositorioReservas.ReservasEntre(alojamiento, _intervalo.Value.Desde, _intervalo.Value.Hasta);
                bool libre = reservas.Count == 0;
                var marca = new Border
                {
                    Width = 18, Height = 18, CornerRadius = new CornerRadius(9),
                    Background = (Brush)FindResource(libre ? "Pincel.Salvia100" : "Pincel.ErrorFondo"),
                    Margin = new Thickness(6, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    ToolTip = libre
                        ? "Sin reservas en el intervalo consultado"
                        : "Reservas en el intervalo: " + string.Join(", ", reservas.Select(r => r.Localizador)),
                    Child = new TextBlock
                    {
                        Style = (Style)FindResource("Icono"),
                        Text = libre ? (string)FindResource("Icono.Correcto") : "",
                        FontSize = 9,
                        Foreground = (Brush)FindResource(libre ? "Pincel.Bosque700" : "Pincel.Error")
                    }
                };
                DockPanel.SetDock(marca, Dock.Right);
                cabeza.Children.Add(marca);
            }
            cabeza.Children.Add(new TextBlock
            {
                Text = Alojamientos.Nombre(alojamiento),
                Style = (Style)FindResource("Texto.Base"),
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            });
            rejilla.Children.Add(cabeza);

            // Franja del intervalo consultado, por debajo de los tramos.
            if (_intervalo.HasValue)
            {
                int a = Math.Max(0, (int)(_intervalo.Value.Desde - _inicio).TotalDays);
                int b = Math.Min(DiasVisibles - 1, (int)(_intervalo.Value.Hasta - _inicio).TotalDays);
                if (a <= b)
                {
                    var franja = new Border { Background = (Brush)FindResource("Pincel.Salvia100"), IsHitTestVisible = false };
                    Grid.SetColumn(franja, a + 1);
                    Grid.SetColumnSpan(franja, b - a + 1);
                    rejilla.Children.Add(franja);
                }
            }

            // Tramos: días seguidos con el mismo estado y la misma reserva.
            var estados = Enumerable.Range(0, DiasVisibles)
                .Select(i => RepositorioReservas.EstadoEn(alojamiento, _inicio.AddDays(i)))
                .ToList();
            int inicioTramo = 0;
            for (int i = 1; i <= DiasVisibles; i++)
            {
                if (i < DiasVisibles && estados[i].Estado == estados[inicioTramo].Estado
                    && estados[i].Reserva == estados[inicioTramo].Reserva)
                    continue;

                rejilla.Children.Add(CrearTramo(estados[inicioTramo], inicioTramo, i - inicioTramo));
                inicioTramo = i;
            }

            // Guías verticales entre días, por encima de los tramos.
            AgregarGuias(rejilla);

            // Casillas pulsables, una por día.
            for (int i = 0; i < DiasVisibles; i++)
            {
                var dia = _inicio.AddDays(i);
                var casilla = new ToggleButton
                {
                    Style = (Style)FindResource("Casilla"),
                    Tag = (alojamiento, dia),
                    IsChecked = alojamiento == _seleccionAlojamiento && dia == _seleccionFecha,
                    IsTabStop = i == 0,
                    ToolTip = $"{Alojamientos.Nombre(alojamiento)} · {dia.ToString("ddd d MMM", Cultura)}: {estados[i].Texto}"
                };
                System.Windows.Automation.AutomationProperties.SetName(casilla,
                    $"{Alojamientos.Nombre(alojamiento)} {dia:dd/MM/yyyy}");
                casilla.Checked += Casilla_Checked;
                casilla.Unchecked += Casilla_Unchecked;
                Grid.SetColumn(casilla, i + 1);
                rejilla.Children.Add(casilla);
                _casillas.Add(casilla);
            }

            return new Border
            {
                BorderBrush = (Brush)FindResource("Pincel.Borde"),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = rejilla
            };
        }

        /// <summary>Líneas verticales finas en el borde izquierdo de cada día para seguir las columnas.</summary>
        private void AgregarGuias(Grid rejilla)
        {
            var pincel = new SolidColorBrush(Color.FromArgb(0x26, 0x2B, 0x24, 0x18));
            pincel.Freeze();
            for (int i = 0; i < DiasVisibles; i++)
            {
                var guia = new Border
                {
                    Width = 1,
                    Background = pincel,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    IsHitTestVisible = false,
                    SnapsToDevicePixels = true
                };
                Grid.SetColumn(guia, i + 1);
                rejilla.Children.Add(guia);
            }
        }

        // La barra de desplazamiento de las filas les quita ancho: la cabecera reserva
        // el mismo hueco para que cada día quede encima de su columna.
        private void PanelFilas_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double barra = PanelFilas.ComputedVerticalScrollBarVisibility == Visibility.Visible
                ? PanelFilas.ActualWidth - PanelFilas.ViewportWidth
                : 0;
            var margen = new Thickness(0, 0, 10 + barra, 0);
            if (Cabecera.Margin != margen) Cabecera.Margin = margen;
        }

        private Border CrearTramo(EstadoDia estado, int columna, int dias)
        {
            string fondo, borde, texto;
            switch (estado.Estado)
            {
                case EstadoAlojamiento.Ocupado: fondo = "#FBEDE7"; borde = "#E7B8A8"; texto = "#B4472E"; break;
                case EstadoAlojamiento.EnReparacion: fondo = "#FBF1DE"; borde = "#E8CC94"; texto = "#9A6418"; break;
                case EstadoAlojamiento.PendienteLimpieza: fondo = "#F6EFC9"; borde = "#DCCB7E"; texto = "#7D6410"; break;
                default: fondo = "#E8EFE6"; borde = "#C5D6C4"; texto = "#2B5239"; break;
            }

            var tramo = new Border
            {
                Background = Pincel(fondo),
                BorderBrush = Pincel(borde),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(7),
                Margin = new Thickness(2, 7, 2, 7),
                Padding = dias == 1 ? new Thickness(0) : new Thickness(6, 0, 6, 0),
                IsHitTestVisible = false,
                Child = new TextBlock
                {
                    Text = TextoTramo(estado, dias),
                    FontFamily = (FontFamily)FindResource("Fuente.Texto"),
                    FontSize = dias == 1 ? 10.5 : 11.5,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Pincel(texto),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = dias == 1 ? HorizontalAlignment.Center : HorizontalAlignment.Left
                }
            };
            Grid.SetColumn(tramo, columna + 1);
            Grid.SetColumnSpan(tramo, dias);
            return tramo;
        }

        private static string TextoTramo(EstadoDia estado, int dias)
        {
            if (dias >= 3) return estado.Texto;
            switch (estado.Estado)
            {
                case EstadoAlojamiento.Ocupado: return dias == 1 ? "Ocup." : "Ocupado";
                case EstadoAlojamiento.EnReparacion: return dias == 1 ? "Rep." : "Reparación";
                case EstadoAlojamiento.PendienteLimpieza: return dias == 1 ? "Limp." : "Limpieza";
                default: return "Libre";
            }
        }

        private static SolidColorBrush Pincel(string hex) =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

        // ===== Cambiar el estado (CU-15) =====

        // Alojamiento del último cambio de estado, para mantener su aviso visible tras redibujar.
        private object _ultimoConAviso;

        private void AplicarEstado_Click(object sender, RoutedEventArgs e)
        {
            var alojamiento = _seleccionAlojamiento;
            if (alojamiento == null || _seleccionFecha != DateTime.Today) return;

            var actual = RepositorioReservas.EstadoEn(alojamiento, DateTime.Today).Estado;
            var nuevo = (EstadoAlojamiento)Math.Max(0, CmbSelEstado.SelectedIndex);
            string nombre = Alojamientos.Nombre(alojamiento);

            if (nuevo == actual)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Sin cambios",
                    $"{nombre} ya está «{Alojamientos.EstadoTexto(actual)}». Elige otro estado en el desplegable.");
                return;
            }

            string motivo = RepositorioReservas.MotivoNoCambioEstado(alojamiento, nuevo);
            if (motivo != null)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No se puede cambiar el estado",
                    motivo + "\n\nSe conserva el estado anterior.", TipoDialogo.Aviso);
                CmbSelEstado.SelectedIndex = (int)actual;
                return;
            }

            string detalle = $"Pasará de «{Alojamientos.EstadoTexto(actual)}» a «{Alojamientos.EstadoTexto(nuevo)}».";
            if (actual == EstadoAlojamiento.Ocupado)
                detalle += "\n\nAhora está ocupado: comprueba que los clientes ya lo han dejado antes de cambiarlo.";
            if (!DialogoMensaje.Confirmar(VentanaPropietaria, $"¿Cambiar el estado de {nombre}?", detalle,
                    "Cambiar estado", "Cancelar"))
            {
                CmbSelEstado.SelectedIndex = (int)actual;
                return;
            }

            try
            {
                RepositorioReservas.CambiarEstadoActual(alojamiento, nuevo);
            }
            catch (InvalidOperationException ex)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No se puede cambiar el estado",
                    ex.Message + " Se conserva el estado anterior.", TipoDialogo.Error);
                return;
            }

            _ultimoConAviso = alojamiento;
            TxtAvisoEstado.Text = $"Estado actualizado: {nombre} está «{Alojamientos.EstadoTexto(nuevo)}».";
            AvisoEstado.Visibility = Visibility.Visible;
            Dibujar();
            Seleccionar(alojamiento, DateTime.Today);
        }

        // ===== Nueva reserva (CU-16) =====

        private void NuevaReserva_Click(object sender, RoutedEventArgs e)
        {
            if (_seleccionAlojamiento == null || !_seleccionFecha.HasValue) return;
            Navegar(new FormularioReservaVista(_seleccionAlojamiento, _seleccionFecha.Value));
        }

        // ===== Selección =====

        // Se activa mientras se marcan las casillas desde el código, para no reaccionar a esos cambios.
        private bool _marcando;

        // Volver a pulsar la casilla seleccionada no la desmarca.
        private void Casilla_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_marcando) ((ToggleButton)sender).IsChecked = true;
        }

        private void Casilla_Checked(object sender, RoutedEventArgs e)
        {
            if (_marcando) return;

            var (alojamiento, dia) = ((object, DateTime))((FrameworkElement)sender).Tag;

            if (!RepositorioReservas.Existe(alojamiento))
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Alojamiento no disponible",
                    "El alojamiento seleccionado ya no existe. Se actualizará la cuadrícula.", TipoDialogo.Aviso);
                Seleccionar(null, null);
                Dibujar();
                return;
            }

            Seleccionar(alojamiento, dia);
        }

        private void Seleccionar(object alojamiento, DateTime? dia)
        {
            _seleccionAlojamiento = alojamiento;
            _seleccionFecha = dia;

            _marcando = true;
            foreach (var casilla in _casillas)
            {
                var (a, d) = ((object, DateTime))casilla.Tag;
                casilla.IsChecked = a == alojamiento && d == dia;
            }
            _marcando = false;

            if (alojamiento == null)
            {
                PanelSeleccion.Visibility = Visibility.Collapsed;
                PanelSinSeleccion.Visibility = Visibility.Visible;
                return;
            }

            var estado = RepositorioReservas.EstadoEn(alojamiento, dia.Value);
            TxtSelNombre.Text = Alojamientos.Nombre(alojamiento);
            TxtSelFecha.Text = "Fecha: " + dia.Value.ToString("dd/MM/yyyy")
                + (dia.Value == DateTime.Today ? " (hoy)" : "");
            ChipSelEstado.DataContext = estado;
            TxtSelEstado.Text = Alojamientos.EstadoTexto(estado.Estado);
            CmbSelEstado.SelectedIndex = (int)estado.Estado;

            // CU-15: el estado que se cambia es el actual, así que solo se edita en la casilla de hoy.
            bool hoy = dia.Value == DateTime.Today;
            CmbSelEstado.IsEnabled = BtnAplicarEstado.IsEnabled = hoy;
            CmbSelEstado.ToolTip = hoy
                ? "Nuevo estado del alojamiento"
                : "El estado solo se cambia en la casilla de hoy; los demás días salen de las reservas";
            BtnAplicarEstado.ToolTip = hoy
                ? "Cambiar el estado actual del alojamiento (se pedirá confirmación)"
                : "Selecciona la casilla de hoy para cambiar el estado actual";
            if (!ReferenceEquals(alojamiento, _ultimoConAviso)) AvisoEstado.Visibility = Visibility.Collapsed;

            // CU-16: solo se puede reservar un día libre a partir de hoy.
            bool reservable = estado.Estado == EstadoAlojamiento.Libre && dia.Value >= DateTime.Today;
            BtnNuevaReserva.IsEnabled = reservable;
            BtnNuevaReserva.ToolTip = reservable
                ? $"Crear una reserva de {Alojamientos.Nombre(alojamiento)} con entrada el {dia.Value:dd/MM/yyyy}"
                : dia.Value < DateTime.Today
                    ? "No se pueden crear reservas en fechas pasadas"
                    : "El alojamiento no está libre en esa fecha";

            if (estado.Reserva != null)
            {
                var r = estado.Reserva;
                TxtSelReserva.Text = $"Reserva {r.Localizador} a nombre de {r.Titular}, "
                                   + $"del {r.Entrada:dd/MM/yyyy} al {r.Salida:dd/MM/yyyy}.";
                TxtSelReserva.Visibility = Visibility.Visible;
            }
            else
                TxtSelReserva.Visibility = Visibility.Collapsed;

            PanelSinSeleccion.Visibility = Visibility.Collapsed;
            PanelSeleccion.Visibility = Visibility.Visible;
        }
    }
}
