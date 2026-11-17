using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
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
    /// PG5 · CU-07 Dar de alta un bungaló (RF-06). Valida los datos, marca en rojo
    /// los campos con problemas y, si todo es correcto, registra el bungaló y vuelve a la pestaña de bungalows.
    /// </summary>
    public partial class FormularioBungalowVista : VistaBase
    {
        private static readonly CultureInfo Es = new CultureInfo("es-ES");

        private const int TamanoMaximo = 300;
        private const int CapacidadMaxima = 12;
        private const decimal PrecioMaximo = 1000;
        private const int EstanciaMaxima = 30;

        private readonly ObservableCollection<string> _fotos = new ObservableCollection<string>();
        private bool _salidaConfirmada;

        public override string OpcionMenu => "Alojamientos";

        public FormularioBungalowVista()
        {
            InitializeComponent();
            ListaFotos.ItemsSource = _fotos;
            _fotos.CollectionChanged += (s, e) => ActualizarGaleria();

            TxtNombre.Text = RepositorioAlojamientos.SiguienteNombreBungalow();
            Loaded += (s, e) => { TxtNombre.Focus(); TxtNombre.SelectAll(); };
        }

        // ===== Guardar =====

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            var errores = new List<string>();

            string nombre = string.Join(" ", TxtNombre.Text.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries));
            if (nombre.Length == 0)
                Marcar(TxtNombre, errores, "Indica el nombre del bungalow.");
            else if (RepositorioAlojamientos.ExisteBungalow(nombre))
                Marcar(TxtNombre, errores, $"Ya existe un bungalow llamado {nombre}.");

            int tamano = LeerEntero(TxtTamano, errores, obligatorio: true, maximo: TamanoMaximo,
                vacio: "Indica el tamaño del bungalow en m².",
                invalido: "El tamaño debe ser un número mayor que 0.",
                excesivo: $"El tamaño no puede superar {TamanoMaximo} m².");

            int capacidad = LeerEntero(TxtCapacidad, errores, obligatorio: true, maximo: CapacidadMaxima,
                vacio: "Indica la capacidad máxima del bungalow.",
                invalido: "La capacidad máxima debe ser un número entero mayor que 0.",
                excesivo: $"La capacidad máxima no puede superar {CapacidadMaxima} personas.");

            decimal precio = 0;
            string textoPrecio = TxtPrecio.Text.Trim().Replace('.', ',');
            if (textoPrecio.Length == 0)
                Marcar(TxtPrecio, errores, "Indica el precio por noche.");
            else if (!decimal.TryParse(textoPrecio, NumberStyles.Number, Es, out precio) || precio <= 0)
                Marcar(TxtPrecio, errores, "El precio por noche debe ser un importe mayor que 0.");
            else if (precio > PrecioMaximo)
                Marcar(TxtPrecio, errores, $"El precio por noche no puede superar {PrecioMaximo:0} €.");
            else if (decimal.Round(precio, 2) != precio)
                Marcar(TxtPrecio, errores, "El precio admite como mucho dos decimales.");

            int estancia = LeerEntero(TxtEstancia, errores, obligatorio: false, maximo: EstanciaMaxima,
                vacio: null,
                invalido: "La estancia mínima debe ser un número de noches mayor que 0.",
                excesivo: $"La estancia mínima no puede superar {EstanciaMaxima} noches.");

            if (errores.Count > 0)
            {
                TxtError.Text = string.Join("\n", errores);
                PanelError.Visibility = Visibility.Visible;
                PrimerCampoConError()?.Focus();
                return;
            }

            var bungalow = new Bungalow
            {
                Nombre = nombre,
                TamanoM2 = tamano,
                CapacidadMaxima = capacidad,
                PrecioNoche = precio,
                EstanciaMinima = estancia > 0 ? estancia : (int?)null,
                Menaje = ChkMenaje.IsChecked == true,
                Microondas = ChkMicroondas.IsChecked == true,
                RopaCamaToallas = ChkRopa.IsChecked == true,
                WiFi = ChkWifi.IsChecked == true,
                Descripcion = TxtDescripcion.Text.Trim(),
                Estado = EstadoAlojamiento.Libre,
                Fotos = _fotos.ToList()
            };
            RepositorioAlojamientos.AgregarBungalow(bungalow);

            _salidaConfirmada = true;
            Navegar(new AlojamientosVista($"{nombre} registrado correctamente.", mostrarBungalows: true));
        }

        /// <summary>Lee un entero positivo de un campo; devuelve 0 si está vacío o no es válido.</summary>
        private static int LeerEntero(TextBox campo, List<string> errores, bool obligatorio, int maximo,
                                      string vacio, string invalido, string excesivo)
        {
            string texto = campo.Text.Trim();
            if (texto.Length == 0)
            {
                if (obligatorio) Marcar(campo, errores, vacio);
                return 0;
            }
            if (!int.TryParse(texto, out int valor) || valor <= 0)
            {
                Marcar(campo, errores, invalido);
                return 0;
            }
            if (valor > maximo)
            {
                Marcar(campo, errores, excesivo);
                return 0;
            }
            return valor;
        }

        private static void Marcar(Control campo, List<string> errores, string mensaje)
        {
            CampoAyudante.SetTieneError(campo, true);
            errores.Add(mensaje);
        }

        private Control PrimerCampoConError() =>
            new Control[] { TxtNombre, TxtTamano, TxtCapacidad, TxtPrecio, TxtEstancia }
                .FirstOrDefault(CampoAyudante.GetTieneError);

        // Al corregir un campo se retira su marca; si ya no queda ninguna, se oculta el aviso.
        private void Campo_Cambiado(object sender, RoutedEventArgs e)
        {
            if (sender is Control campo && CampoAyudante.GetTieneError(campo))
                CampoAyudante.SetTieneError(campo, false);

            if (PanelError != null && PrimerCampoConError() == null)
                PanelError.Visibility = Visibility.Hidden;
        }

        private void SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void SoloPrecio(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(c => char.IsDigit(c) || c == ',' || c == '.');
        }

        // ===== Galería de fotos =====

        private void AnadirFotos_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new OpenFileDialog
            {
                Title = "Añadir fotos del bungalow",
                Filter = "Imágenes (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png",
                Multiselect = true
            };
            if (dialogo.ShowDialog(VentanaPropietaria) != true) return;

            var noValidas = new List<string>();
            foreach (string ruta in dialogo.FileNames)
            {
                if (_fotos.Contains(ruta)) continue;
                if (RutaImagenConverter.Cargar(ruta, 64) == null)
                    noValidas.Add(System.IO.Path.GetFileName(ruta));
                else
                    _fotos.Add(ruta);
            }

            if (noValidas.Count > 0)
                DialogoMensaje.Mostrar(VentanaPropietaria, "Algunas fotos no se han añadido",
                    "Estos archivos no son imágenes válidas: " + string.Join(", ", noValidas) + ".",
                    TipoDialogo.Aviso);
        }

        private void QuitarFoto_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is string ruta)
                _fotos.Remove(ruta);
        }

        private void ActualizarGaleria()
        {
            bool hayFotos = _fotos.Count > 0;
            ListaFotos.Visibility = hayFotos ? Visibility.Visible : Visibility.Collapsed;
            TxtSinFotos.Visibility = hayFotos ? Visibility.Collapsed : Visibility.Visible;
        }

        // ===== Cancelar y salida =====

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            if (!PuedeSalir()) return;
            _salidaConfirmada = true;
            Navegar(new AlojamientosVista(null, mostrarBungalows: true));
        }

        private bool HayCambios =>
            TxtNombre.Text.Trim() != RepositorioAlojamientos.SiguienteNombreBungalow()
            || new[] { TxtTamano, TxtCapacidad, TxtPrecio, TxtEstancia, TxtDescripcion }.Any(t => t.Text.Trim().Length > 0)
            || new[] { ChkMenaje, ChkMicroondas, ChkRopa, ChkWifi }.Any(c => c.IsChecked == true)
            || _fotos.Count > 0;

        /// <summary>Si hay datos sin guardar, pide confirmación antes de abandonar el formulario (RF-36).</summary>
        public override bool PuedeSalir()
        {
            if (_salidaConfirmada || !HayCambios) return true;

            _salidaConfirmada = DialogoMensaje.Confirmar(VentanaPropietaria,
                "¿Descartar el nuevo bungalow?",
                "Se perderán los datos y las fotos que has introducido en el formulario.",
                "Descartar", "Seguir editando");
            return _salidaConfirmada;
        }
    }
}
