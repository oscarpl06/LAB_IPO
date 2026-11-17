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

        public override string OpcionMenu => "Alojamientos";

        public FormularioParcelaVista()
        {
            InitializeComponent();

            CmbTipo.ItemsSource = RepositorioAlojamientos.TiposParcela;
            CmbUbicacion.ItemsSource = RepositorioAlojamientos.Ubicaciones;

            _identificadorSugerido = RepositorioAlojamientos.SiguienteIdentificadorParcela();
            TxtIdentificador.Text = _identificadorSugerido;

            Loaded += (s, e) => { TxtIdentificador.Focus(); TxtIdentificador.SelectAll(); };
        }

        // ===== Guardar =====

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            var errores = new List<string>();
            string identificador = TxtIdentificador.Text.Trim().ToUpperInvariant();

            if (identificador.Length == 0)
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

        private bool HayCambios =>
            TxtIdentificador.Text.Trim() != _identificadorSugerido
            || CmbTipo.SelectedItem != null
            || TxtTamano.Text.Trim().Length > 0
            || CmbUbicacion.SelectedItem != null
            || new[] { ChkAgua, ChkLuz, ChkDesague, ChkSetos, ChkSombra }.Any(c => c.IsChecked == true)
            || _rutaFoto != null;

        /// <summary>Si hay datos sin guardar, pide confirmación antes de abandonar el formulario (RF-36).</summary>
        public override bool PuedeSalir()
        {
            if (_salidaConfirmada || !HayCambios) return true;

            _salidaConfirmada = DialogoMensaje.Confirmar(VentanaPropietaria,
                "¿Descartar la nueva parcela?",
                "Se perderán los datos que has introducido en el formulario.",
                "Descartar", "Seguir editando");
            return _salidaConfirmada;
        }
    }
}
