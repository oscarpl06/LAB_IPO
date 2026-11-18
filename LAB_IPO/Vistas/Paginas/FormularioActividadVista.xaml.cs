using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG13b · CU-24 Dar de alta una actividad (RF-26). Valida los datos, marca en rojo los
    /// campos con problemas y, si todo es correcto, registra la actividad y vuelve al listado.
    /// </summary>
    public partial class FormularioActividadVista : VistaBase
    {
        private static readonly CultureInfo Es = new CultureInfo("es-ES");
        private static readonly TimeSpan PrimeraHora = new TimeSpan(7, 0, 0);
        private static readonly TimeSpan UltimaHora = new TimeSpan(23, 0, 0);
        private const int CupoMaximoPermitido = 100;
        private const decimal PrecioMaximo = 9999;

        private readonly Dictionary<DayOfWeek, ToggleButton> _dias = new Dictionary<DayOfWeek, ToggleButton>();
        private bool _salidaConfirmada;

        public override string OpcionMenu => "Actividades";

        public FormularioActividadVista()
        {
            InitializeComponent();

            CmbPublico.ItemsSource = RepositorioActividades.Publicos;
            CmbArea.ItemsSource = RepositorioActividades.Areas;
            CmbUnidadPrecio.ItemsSource = RepositorioActividades.UnidadesPrecio;
            CmbUnidadPrecio.SelectedIndex = 0;
            CmbUnidadPrecio.IsEnabled = false;   // se activa al escribir un precio

            foreach (var dia in Actividad.Semana)
            {
                string nombre = Es.DateTimeFormat.GetDayName(dia);
                var boton = new ToggleButton
                {
                    Content = dia == DayOfWeek.Wednesday ? "X" : nombre.Substring(0, 1).ToUpper(),
                    Style = (Style)FindResource("Boton.Dia"),
                    ToolTip = char.ToUpper(nombre[0]) + nombre.Substring(1)
                };
                System.Windows.Automation.AutomationProperties.SetName(boton, nombre);
                boton.Checked += (s, e) => Dias_Cambiados();
                boton.Unchecked += (s, e) => Dias_Cambiados();
                _dias[dia] = boton;
                PanelDias.Children.Add(boton);
            }

            Loaded += (s, e) => TxtNombre.Focus();
        }

        // ===== Días =====

        private void MarcarDias(params DayOfWeek[] dias)
        {
            foreach (var par in _dias) par.Value.IsChecked = dias.Contains(par.Key);
            Dias_Cambiados();
        }

        private void DiasLaborables_Click(object sender, RoutedEventArgs e) =>
            MarcarDias(DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday);

        private void FinDeSemana_Click(object sender, RoutedEventArgs e) =>
            MarcarDias(DayOfWeek.Saturday, DayOfWeek.Sunday);

        private void TodosLosDias_Click(object sender, RoutedEventArgs e) => MarcarDias(Actividad.Semana);

        private List<DayOfWeek> DiasElegidos => _dias.Where(p => p.Value.IsChecked == true).Select(p => p.Key).ToList();

        private void Dias_Cambiados()
        {
            if (DiasElegidos.Count > 0)
                foreach (var boton in _dias.Values) CampoAyudante.SetTieneError(boton, false);
            ActualizarPanelError();
        }

        // ===== Precio =====

        private void Precio_Cambiado(object sender, TextChangedEventArgs e)
        {
            CmbUnidadPrecio.IsEnabled = TxtPrecio.Text.Trim().Length > 0;
            Campo_Cambiado(sender, e);
        }

        private void SoloImporte(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(c => char.IsDigit(c) || c == ',' || c == '.');
        }

        private void SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        // ===== Guardar =====

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            var errores = new List<string>();

            string nombre = Regex.Replace(TxtNombre.Text.Trim(), @"\s+", " ");
            if (nombre.Length == 0)
                Marcar(TxtNombre, errores, "Indica el nombre de la actividad.");
            else if (nombre.Length < 3)
                Marcar(TxtNombre, errores, "El nombre de la actividad debe tener al menos 3 caracteres.");
            else if (RepositorioActividades.Existe(nombre))
                Marcar(TxtNombre, errores, $"Ya existe una actividad llamada «{nombre}».");

            if (CmbPublico.SelectedItem == null)
                Marcar(CmbPublico, errores, "Selecciona el público de la actividad.");

            var dias = DiasElegidos;
            if (dias.Count == 0)
            {
                foreach (var boton in _dias.Values) CampoAyudante.SetTieneError(boton, true);
                errores.Add("Marca al menos un día de la semana.");
            }

            var inicio = LeerHora(TxtHoraInicio, "de inicio", errores);
            var fin = LeerHora(TxtHoraFin, "de fin", errores);
            if (inicio.HasValue && fin.HasValue && fin <= inicio)
                Marcar(TxtHoraFin, errores, "La hora de fin debe ser posterior a la hora de inicio.");

            int? cupoMinimo = null;
            int cupoMaximo = 0;
            if (TxtCupoMaximo.Text.Trim().Length == 0)
                Marcar(TxtCupoMaximo, errores, "Indica el cupo máximo de participantes.");
            else if (!int.TryParse(TxtCupoMaximo.Text, out cupoMaximo) || cupoMaximo <= 0)
                Marcar(TxtCupoMaximo, errores, "El cupo máximo debe ser un número mayor que cero.");
            else if (cupoMaximo > CupoMaximoPermitido)
                Marcar(TxtCupoMaximo, errores, $"El cupo máximo no puede superar {CupoMaximoPermitido} participantes.");

            if (TxtCupoMinimo.Text.Trim().Length > 0)
            {
                if (!int.TryParse(TxtCupoMinimo.Text, out int minimo) || minimo <= 0)
                    Marcar(TxtCupoMinimo, errores, "El cupo mínimo debe ser un número mayor que cero.");
                else if (cupoMaximo > 0 && minimo > cupoMaximo)
                    Marcar(TxtCupoMinimo, errores, "El cupo mínimo no puede ser mayor que el cupo máximo.");
                else
                    cupoMinimo = minimo;
            }

            decimal? precio = null;
            string textoPrecio = TxtPrecio.Text.Trim().Replace('.', ',');
            if (textoPrecio.Length > 0)
            {
                if (!decimal.TryParse(textoPrecio, NumberStyles.Number, Es, out decimal valor) || valor < 0
                    || Regex.IsMatch(textoPrecio, @",\d{3,}"))
                    Marcar(TxtPrecio, errores, "El precio debe ser un importe en euros, con dos decimales como máximo (ej. 8,50).");
                else if (valor > PrecioMaximo)
                    Marcar(TxtPrecio, errores, $"El precio no puede superar {PrecioMaximo} €.");
                else
                    precio = valor;
            }

            if (CmbArea.SelectedItem == null)
                Marcar(CmbArea, errores, "Selecciona el área del camping donde se realiza la actividad.");

            if (errores.Count > 0)
            {
                // Con muchos errores se resumen para no tapar el formulario.
                TxtError.Text = errores.Count <= 4
                    ? string.Join("\n", errores)
                    : string.Join("\n", errores.Take(3))
                      + $"\n… y {errores.Count - 3} más: revisa los campos marcados en rojo.";
                ActualizarPanelError(forzarVisible: true);
                PrimerCampoConError()?.Focus();
                return;
            }

            var actividad = new Actividad
            {
                Nombre = nombre,
                Publico = (string)CmbPublico.SelectedItem,
                Descripcion = TxtDescripcion.Text.Trim(),
                Dias = dias,
                HoraInicio = inicio.Value,
                HoraFin = fin.Value,
                CupoMinimo = cupoMinimo,
                CupoMaximo = cupoMaximo,
                Precio = precio > 0 ? precio : null,
                UnidadPrecio = precio > 0 ? (string)CmbUnidadPrecio.SelectedItem : null,
                Area = (string)CmbArea.SelectedItem,
                Materiales = TxtMateriales.Text.Trim(),
                Notas = TxtNotas.Text.Trim()
            };
            RepositorioActividades.Agregar(actividad);

            _salidaConfirmada = true;
            Navegar(new ActividadesVista($"Actividad «{actividad.Nombre}» creada correctamente. "
                                         + "Ya puedes asignarle sus monitores.", actividad));
        }

        /// <summary>Lee una hora hh:mm (admite 9:30) dentro del horario del camping.</summary>
        private TimeSpan? LeerHora(TextBox caja, string cual, List<string> errores)
        {
            string texto = caja.Text.Trim();
            if (texto.Length == 0)
            {
                Marcar(caja, errores, $"Indica la hora {cual}.");
                return null;
            }
            if (!Regex.IsMatch(texto, @"^\d{1,2}:\d{2}$")
                || !TimeSpan.TryParseExact(texto, new[] { @"h\:mm", @"hh\:mm" }, CultureInfo.InvariantCulture, out var hora))
            {
                Marcar(caja, errores, $"La hora {cual} no es válida: usa el formato hh:mm (ej. 10:00).");
                return null;
            }
            if (hora < PrimeraHora || hora > UltimaHora)
            {
                Marcar(caja, errores, $"La hora {cual} debe estar entre las 07:00 y las 23:00.");
                return null;
            }
            return hora;
        }

        private static void Marcar(Control campo, List<string> errores, string mensaje)
        {
            CampoAyudante.SetTieneError(campo, true);
            errores.Add(mensaje);
        }

        private Control PrimerCampoConError() =>
            new Control[] { TxtNombre, CmbPublico }
                .Concat(_dias.Values)
                .Concat(new Control[] { TxtHoraInicio, TxtHoraFin, TxtCupoMinimo, TxtCupoMaximo, TxtPrecio, CmbArea })
                .FirstOrDefault(CampoAyudante.GetTieneError);

        // Al corregir un campo se retira su marca; si ya no queda ninguna, se oculta el aviso.
        private void Campo_Cambiado(object sender, RoutedEventArgs e)
        {
            if (sender is Control campo && CampoAyudante.GetTieneError(campo))
                CampoAyudante.SetTieneError(campo, false);
            ActualizarPanelError();
        }

        // El aviso de errores ocupa el sitio de la nota sobre los monitores.
        private void ActualizarPanelError(bool forzarVisible = false)
        {
            bool hayErrores = forzarVisible || (PanelError.Visibility == Visibility.Visible && PrimerCampoConError() != null);
            PanelError.Visibility = hayErrores ? Visibility.Visible : Visibility.Hidden;
            NotaMonitores.Visibility = hayErrores ? Visibility.Hidden : Visibility.Visible;
        }

        // ===== Cancelar y salida =====

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            if (!PuedeSalir()) return;
            _salidaConfirmada = true;
            Navegar(new ActividadesVista());
        }

        private bool HayDatos =>
            new[] { TxtNombre, TxtDescripcion, TxtHoraInicio, TxtHoraFin, TxtCupoMinimo, TxtCupoMaximo,
                    TxtPrecio, TxtMateriales, TxtNotas }.Any(t => t.Text.Trim().Length > 0)
            || CmbPublico.SelectedItem != null || CmbArea.SelectedItem != null || DiasElegidos.Count > 0;

        /// <summary>Si hay datos sin guardar, pide confirmación antes de abandonar el formulario (RF-36).</summary>
        public override bool PuedeSalir()
        {
            if (_salidaConfirmada || !HayDatos) return true;

            _salidaConfirmada = DialogoMensaje.Confirmar(VentanaPropietaria,
                "¿Descartar la nueva actividad?",
                "Se perderán los datos que has introducido en el formulario.",
                "Descartar", "Seguir editando");
            return _salidaConfirmada;
        }
    }
}
