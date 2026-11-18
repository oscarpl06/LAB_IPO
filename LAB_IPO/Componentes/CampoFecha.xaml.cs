using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LAB_IPO.Componentes
{
    /// <summary>
    /// Campo de fecha reutilizable (reservas, disponibilidad...). Se puede escribir la fecha
    /// en formato dd/mm/aaaa o elegirla en un calendario mensual que empieza en lunes.
    /// </summary>
    public partial class CampoFecha : UserControl
    {
        private static readonly CultureInfo Cultura = new CultureInfo("es-ES");
        private static readonly string[] Formatos = { "d/M/yyyy", "dd/MM/yyyy", "d-M-yyyy", "d.M.yyyy", "d/M/yy" };

        private DateTime _mesVisible = DateTime.Today;
        private bool _escribiendoTexto;

        public CampoFecha()
        {
            InitializeComponent();

            // Siempre bajo el campo y alineado a su izquierda, aunque Windows esté configurado
            // para abrir los menús hacia la izquierda (equipos táctiles para diestros).
            Desplegable.CustomPopupPlacementCallback = (popup, destino, desplazamiento) => new[]
            {
                new System.Windows.Controls.Primitives.CustomPopupPlacement(new Point(-8, destino.Height - 4),
                    System.Windows.Controls.Primitives.PopupPrimaryAxis.Horizontal)
            };
        }

        // ===== Propiedades =====

        public static readonly DependencyProperty FechaProperty =
            DependencyProperty.Register(nameof(Fecha), typeof(DateTime?), typeof(CampoFecha),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AlCambiarFecha));

        /// <summary>Fecha elegida, o null si el campo está vacío o el texto no es una fecha.</summary>
        public DateTime? Fecha
        {
            get => (DateTime?)GetValue(FechaProperty);
            set => SetValue(FechaProperty, value);
        }

        public static readonly DependencyProperty TieneErrorProperty =
            DependencyProperty.Register(nameof(TieneError), typeof(bool), typeof(CampoFecha),
                new PropertyMetadata(false, (d, e) => CampoAyudante.SetTieneError(((CampoFecha)d).CajaFecha, (bool)e.NewValue)));

        public bool TieneError
        {
            get => (bool)GetValue(TieneErrorProperty);
            set => SetValue(TieneErrorProperty, value);
        }

        /// <summary>Hay texto escrito pero no es una fecha válida.</summary>
        public bool TextoNoValido => !string.IsNullOrWhiteSpace(CajaFecha.Text) && Interpretar(CajaFecha.Text) == null;

        public bool EstaVacio => string.IsNullOrWhiteSpace(CajaFecha.Text);

        /// <summary>Se lanza cuando el usuario cambia la fecha (escribiendo o en el calendario).</summary>
        public event EventHandler FechaCambiada;

        public void Enfocar()
        {
            CajaFecha.Focus();
            CajaFecha.SelectAll();
        }

        public void Limpiar()
        {
            CajaFecha.Text = "";
            Fecha = null;
        }

        private static void AlCambiarFecha(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var campo = (CampoFecha)d;
            if (!campo._escribiendoTexto)
            {
                var fecha = (DateTime?)e.NewValue;
                campo.CajaFecha.Text = fecha?.ToString("dd/MM/yyyy") ?? "";
            }
            campo.FechaCambiada?.Invoke(campo, EventArgs.Empty);
        }

        private static DateTime? Interpretar(string texto)
        {
            return DateTime.TryParseExact(texto.Trim(), Formatos, Cultura, DateTimeStyles.None, out var fecha)
                ? fecha.Date
                : (DateTime?)null;
        }

        // ===== Escritura =====

        private void CajaFecha_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TieneError) TieneError = false;

            // Mientras se escribe, la fecha sigue al texto (null si todavía no es válida).
            _escribiendoTexto = true;
            Fecha = Interpretar(CajaFecha.Text);
            _escribiendoTexto = false;
        }

        // Al salir del campo, una fecha válida se reescribe con el formato completo.
        private void CajaFecha_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Fecha.HasValue)
                CajaFecha.Text = Fecha.Value.ToString("dd/MM/yyyy");
        }

        private void CajaFecha_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var tecla = e.Key == Key.System ? e.SystemKey : e.Key;
            if (tecla == Key.Down && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) || tecla == Key.F4)
            {
                BtnCalendario.IsChecked = true;
                e.Handled = true;
            }
        }

        // ===== Calendario desplegable =====

        private void BtnCalendario_Checked(object sender, RoutedEventArgs e)
        {
            var referencia = Fecha ?? DateTime.Today;
            _mesVisible = new DateTime(referencia.Year, referencia.Month, 1);
            DibujarMes();
            Desplegable.IsOpen = true;

            // Mientras está abierto, el botón no recibe clics: así un segundo clic lo cierra sin reabrirlo.
            BtnCalendario.IsHitTestVisible = false;
        }

        private void Desplegable_Closed(object sender, EventArgs e)
        {
            BtnCalendario.IsChecked = false;
            BtnCalendario.IsHitTestVisible = true;
        }

        private void MesAnterior_Click(object sender, RoutedEventArgs e)
        {
            _mesVisible = _mesVisible.AddMonths(-1);
            DibujarMes();
        }

        private void MesSiguiente_Click(object sender, RoutedEventArgs e)
        {
            _mesVisible = _mesVisible.AddMonths(1);
            DibujarMes();
        }

        private void Hoy_Click(object sender, RoutedEventArgs e) => Elegir(DateTime.Today);

        private void DibujarMes()
        {
            var nombreMes = Cultura.DateTimeFormat.GetMonthName(_mesVisible.Month);
            TxtMes.Text = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1) + " " + _mesVisible.Year;

            // Primer lunes de la cuadrícula (6 semanas).
            int desplazamiento = ((int)_mesVisible.DayOfWeek + 6) % 7;
            var inicio = _mesVisible.AddDays(-desplazamiento);

            Dias.Children.Clear();
            for (int i = 0; i < 42; i++)
            {
                var dia = inicio.AddDays(i);
                var boton = new Button
                {
                    Content = dia.Day.ToString(),
                    Style = (Style)FindResource("Calendario.Dia"),
                    ToolTip = dia.ToString("dddd, d 'de' MMMM 'de' yyyy", Cultura),
                    Tag = Fecha == dia ? "Seleccionado"
                        : dia == DateTime.Today ? "Hoy"
                        : dia.Month != _mesVisible.Month ? "Otro" : null
                };
                boton.Click += (s, e) => Elegir(dia);
                Dias.Children.Add(boton);
            }
        }

        private void Elegir(DateTime dia)
        {
            Fecha = dia;
            Desplegable.IsOpen = false;
            CajaFecha.Focus();
            CajaFecha.CaretIndex = CajaFecha.Text.Length;
        }
    }
}
