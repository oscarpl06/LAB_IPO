using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG4 · CU-03 Dar de alta una parcela (RF-01). Valida los datos, marca en rojo
    /// los campos con problemas y, si todo es correcto, registra la parcela y vuelve al listado.
    /// </summary>
    public partial class FormularioParcelaVista : VistaBase
    {
        private const int TamanoMaximo = 1000;

        private readonly string _identificadorSugerido;
        private string _rutaFoto;
        private bool _salidaConfirmada;

        /// <summary>Parcela que se está modificando (CU-05), o null en un alta (CU-03).</summary>
        private readonly Parcela _original;
        private bool EsEdicion => _original != null;

        public override string OpcionMenu => "Alojamientos";

        /// <summary>CU-03 · Formulario vacío para dar de alta una parcela.</summary>
        public FormularioParcelaVista()
        {
            InitializeComponent();

            CmbTipo.ItemsSource = RepositorioAlojamientos.TiposParcela;
            CmbUbicacion.ItemsSource = RepositorioAlojamientos.Ubicaciones;

            _identificadorSugerido = RepositorioAlojamientos.SiguienteIdentificadorParcela();
            TxtIdentificador.Text = _identificadorSugerido;

            Loaded += (s, e) =>
            {
                if (EsEdicion) { CmbTipo.Focus(); return; }
                TxtIdentificador.Focus();
                TxtIdentificador.SelectAll();
            };
        }

        /// <summary>
        /// CU-05 · Mismo formulario con los datos actuales de la parcela (RF-03).
        /// El identificador se muestra pero no se puede cambiar.
        /// </summary>
        public FormularioParcelaVista(Parcela parcela) : this()
        {
            _original = parcela ?? throw new ArgumentNullException(nameof(parcela));

            TxtTitulo.Text = $"Editar parcela {parcela.Identificador}";
            TxtSubtitulo.Text = "Modifica el tipo, el tamaño, la ubicación, los servicios o la foto de la parcela";
            TxtGuardar.Text = "Guardar cambios";
            BtnGuardar.ToolTip = "Guardar los cambios de la parcela";
            BtnCancelarFormulario.ToolTip = "Volver al listado sin guardar los cambios";

            TxtIdentificador.Text = parcela.Identificador;
            TxtIdentificador.IsEnabled = false;
            TxtIdentificador.ToolTip = "El identificador de una parcela no se puede cambiar";
            ToolTipService.SetShowOnDisabled(TxtIdentificador, true);

            CmbTipo.SelectedItem = parcela.Tipo;
            TxtTamano.Text = parcela.TamanoM2.ToString();
            CmbUbicacion.SelectedItem = parcela.Ubicacion;
            ChkAgua.IsChecked = parcela.TomaAgua;
            ChkLuz.IsChecked = parcela.TomaLuz;
            ChkDesague.IsChecked = parcela.Desague;
            ChkSetos.IsChecked = parcela.DelimitadaSetos;
            ChkSombra.IsChecked = parcela.ZonaSombra;

            _rutaFoto = parcela.RutaFoto;
            PincelFoto.ImageSource = RutaImagenConverter.Cargar(_rutaFoto, 600);
            BtnQuitarFoto.IsEnabled = _rutaFoto != null;
        }

        // ===== Guardar =====

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            var errores = new List<string>();
            string identificador = TxtIdentificador.Text.Trim().ToUpperInvariant();

            if (EsEdicion)
                identificador = _original.Identificador;   // bloqueado: no se valida
            else if (identificador.Length == 0)
                Marcar(TxtIdentificador, errores, "Indica el identificador de la parcela.");
            else if (!Regex.IsMatch(identificador, @"^P-\d{1,3}$"))
                Marcar(TxtIdentificador, errores, "El identificador debe tener el formato P-número (ej. P-22).");
            else if (RepositorioAlojamientos.ExisteParcela(identificador))
                Marcar(TxtIdentificador, errores, $"Ya existe una parcela con el identificador {identificador}.");

            if (CmbTipo.SelectedItem == null)
                Marcar(CmbTipo, errores, "Selecciona el tipo de parcela.");

            int tamano = 0;
            string textoTamano = TxtTamano.Text.Trim();
            if (textoTamano.Length == 0)
                Marcar(TxtTamano, errores, "Indica el tamaño de la parcela en m².");
            else if (!int.TryParse(textoTamano, out tamano) || tamano <= 0)
                Marcar(TxtTamano, errores, "El tamaño debe ser un número mayor que 0.");
            else if (tamano > TamanoMaximo)
                Marcar(TxtTamano, errores, $"El tamaño no puede superar {TamanoMaximo} m².");

            if (CmbUbicacion.SelectedItem == null)
                Marcar(CmbUbicacion, errores, "Selecciona la ubicación de la parcela.");

            if (errores.Count > 0)
            {
                TxtError.Text = string.Join("\n", errores);
                PanelError.Visibility = Visibility.Visible;
                PrimerCampoConError()?.Focus();
                return;
            }

            if (EsEdicion)
            {
                GuardarCambios(tamano);
                return;
            }

            var parcela = new Parcela
            {
                Identificador = identificador,
                Tipo = (TipoParcela)CmbTipo.SelectedItem,
                TamanoM2 = tamano,
                Ubicacion = (string)CmbUbicacion.SelectedItem,
                TomaAgua = ChkAgua.IsChecked == true,
                TomaLuz = ChkLuz.IsChecked == true,
                Desague = ChkDesague.IsChecked == true,
                DelimitadaSetos = ChkSetos.IsChecked == true,
                ZonaSombra = ChkSombra.IsChecked == true,
                Estado = EstadoAlojamiento.Libre,
                RutaFoto = _rutaFoto
            };
            RepositorioAlojamientos.AgregarParcela(parcela);

            _salidaConfirmada = true;
            Navegar(new AlojamientosVista($"Parcela {identificador} registrada correctamente."));
        }

        /// <summary>CU-05 · Pide confirmación y aplica los cambios sobre la parcela existente (RF-36).</summary>
        private void GuardarCambios(int tamano)
        {
            if (!HayCambios)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No hay cambios que guardar",
                    $"No has modificado ningún dato de la parcela {_original.Identificador}.");
                return;
            }

            bool confirmar = DialogoMensaje.Confirmar(VentanaPropietaria,
                $"¿Guardar los cambios de la parcela {_original.Identificador}?",
                "Se actualizarán sus datos con los valores del formulario.",
                "Guardar", "Seguir editando");
            if (!confirmar) return;

            _original.Tipo = (TipoParcela)CmbTipo.SelectedItem;
            _original.TamanoM2 = tamano;
            _original.Ubicacion = (string)CmbUbicacion.SelectedItem;
            _original.TomaAgua = ChkAgua.IsChecked == true;
            _original.TomaLuz = ChkLuz.IsChecked == true;
            _original.Desague = ChkDesague.IsChecked == true;
            _original.DelimitadaSetos = ChkSetos.IsChecked == true;
            _original.ZonaSombra = ChkSombra.IsChecked == true;
            _original.RutaFoto = _rutaFoto;

            _salidaConfirmada = true;
            Navegar(new AlojamientosVista($"Cambios de la parcela {_original.Identificador} guardados correctamente.",
                destacar: _original));
        }

        private static void Marcar(Control campo, List<string> errores, string mensaje)
        {
            CampoAyudante.SetTieneError(campo, true);
            errores.Add(mensaje);
        }

        private Control PrimerCampoConError() =>
            new Control[] { TxtIdentificador, CmbTipo, TxtTamano, CmbUbicacion }
                .FirstOrDefault(CampoAyudante.GetTieneError);

        // Al corregir un campo se retira su marca; si ya no queda ninguna, se oculta el aviso.
        private void Campo_Cambiado(object sender, RoutedEventArgs e)
        {
            if (sender is Control campo && CampoAyudante.GetTieneError(campo))
                CampoAyudante.SetTieneError(campo, false);

            if (PrimerCampoConError() == null)
                PanelError.Visibility = Visibility.Hidden;
        }

        private void SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        // ===== Foto =====

        private void SeleccionarFoto_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new OpenFileDialog
            {
                Title = "Seleccionar foto de la parcela",
                Filter = "Imágenes (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png"
            };
            if (dialogo.ShowDialog(VentanaPropietaria) != true) return;

            var imagen = RutaImagenConverter.Cargar(dialogo.FileName, 600);
            if (imagen == null)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No se puede abrir la imagen",
                    "El archivo elegido no es una imagen válida. Prueba con otra foto en formato JPG o PNG.",
                    TipoDialogo.Error);
                return;
            }

            _rutaFoto = dialogo.FileName;
            PincelFoto.ImageSource = imagen;
            BtnQuitarFoto.IsEnabled = true;
        }

        private void QuitarFoto_Click(object sender, RoutedEventArgs e)
        {
            _rutaFoto = null;
            PincelFoto.ImageSource = null;
            BtnQuitarFoto.IsEnabled = false;
        }

        // ===== Cancelar y salida =====

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            if (!PuedeSalir()) return;
            _salidaConfirmada = true;
            Navegar(new AlojamientosVista());
        }

        private bool HayCambios => EsEdicion ? HayCambiosEnEdicion : HayDatosEnAlta;

        private bool HayDatosEnAlta =>
            TxtIdentificador.Text.Trim() != _identificadorSugerido
            || CmbTipo.SelectedItem != null
            || TxtTamano.Text.Trim().Length > 0
            || CmbUbicacion.SelectedItem != null
            || new[] { ChkAgua, ChkLuz, ChkDesague, ChkSetos, ChkSombra }.Any(c => c.IsChecked == true)
            || _rutaFoto != null;

        /// <summary>True si algún campo difiere de los datos actuales de la parcela.</summary>
        private bool HayCambiosEnEdicion =>
            !ReferenceEquals(CmbTipo.SelectedItem, _original.Tipo)
            || TxtTamano.Text.Trim() != _original.TamanoM2.ToString()
            || (string)CmbUbicacion.SelectedItem != _original.Ubicacion
            || (ChkAgua.IsChecked == true) != _original.TomaAgua
            || (ChkLuz.IsChecked == true) != _original.TomaLuz
            || (ChkDesague.IsChecked == true) != _original.Desague
            || (ChkSetos.IsChecked == true) != _original.DelimitadaSetos
            || (ChkSombra.IsChecked == true) != _original.ZonaSombra
            || _rutaFoto != _original.RutaFoto;

        /// <summary>Si hay datos sin guardar, pide confirmación antes de abandonar el formulario (RF-36).</summary>
        public override bool PuedeSalir()
        {
            if (_salidaConfirmada || !HayCambios) return true;

            _salidaConfirmada = EsEdicion
                ? DialogoMensaje.Confirmar(VentanaPropietaria,
                    $"¿Descartar los cambios de la parcela {_original.Identificador}?",
                    "Los datos de la parcela se quedarán como estaban.",
                    "Descartar", "Seguir editando")
                : DialogoMensaje.Confirmar(VentanaPropietaria,
                    "¿Descartar la nueva parcela?",
                    "Se perderán los datos que has introducido en el formulario.",
                    "Descartar", "Seguir editando");
            return _salidaConfirmada;
        }
    }
}
