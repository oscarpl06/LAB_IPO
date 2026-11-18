using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG6 · Galería de un bungalow.
    /// CU-13: ver las fotos (foto grande, flechas y miniaturas; galería vacía si no hay ninguna).
    /// CU-11: añadir una o varias fotos JPG/PNG, validando cada archivo.
    /// CU-12: eliminar la foto seleccionada tras confirmarlo.
    /// Los cambios se aplican al momento sobre el bungalow.
    /// </summary>
    public partial class GaleriaBungalowVista : VistaBase
    {
        private readonly Bungalow _bungalow;
        private int _indice;

        public override string OpcionMenu => "Alojamientos";

        public GaleriaBungalowVista(Bungalow bungalow)
        {
            InitializeComponent();
            _bungalow = bungalow ?? throw new ArgumentNullException(nameof(bungalow));
            TxtTitulo.Text = $"Galería del {bungalow.Nombre}";
            Mostrar(0);
            Loaded += (s, e) => Focus();
        }

        /// <summary>Elemento de la tira de miniaturas.</summary>
        public class Miniatura
        {
            public string Ruta { get; set; }
            public int Numero { get; set; }
            public string Nombre { get; set; }
            public bool Seleccionada { get; set; }
        }

        private List<string> Fotos => _bungalow.Fotos;

        private static string NombreArchivo(string ruta) =>
            string.IsNullOrEmpty(ruta) ? "" : Path.GetFileName(ruta.Replace('/', Path.DirectorySeparatorChar));

        // ===== Visualización (CU-13) =====

        private void Mostrar(int indice)
        {
            bool hayFotos = Fotos.Count > 0;
            _indice = hayFotos ? Math.Max(0, Math.Min(indice, Fotos.Count - 1)) : 0;

            FotoGrande.ImageSource = hayFotos ? RutaImagenConverter.Cargar(Fotos[_indice], 1400) : null;
            PanelVacio.Visibility = hayFotos ? Visibility.Collapsed : Visibility.Visible;
            ChipPortada.Visibility = hayFotos && _indice == 0 ? Visibility.Visible : Visibility.Collapsed;

            BtnAnterior.Visibility = BtnSiguiente.Visibility = Fotos.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            BtnAnterior.IsEnabled = _indice > 0;
            BtnSiguiente.IsEnabled = _indice < Fotos.Count - 1;

            TxtPosicion.Text = hayFotos ? $"Foto {_indice + 1} de {Fotos.Count}" : "Sin fotos";
            TxtArchivo.Text = hayFotos ? NombreArchivo(Fotos[_indice]) : "La galería está vacía.";
            TxtArchivo.ToolTip = hayFotos ? Fotos[_indice] : null;

            BtnEliminarFoto.IsEnabled = hayFotos;
            BtnEliminarFoto.ToolTip = hayFotos
                ? "Quitar de la galería la foto que se ve en grande"
                : "No hay ninguna foto que eliminar";

            TxtMiniaturas.Text = hayFotos ? $"Miniaturas ({Fotos.Count})" : "Miniaturas";
            Miniaturas.ItemsSource = Fotos.Select((ruta, i) => new Miniatura
            {
                Ruta = ruta, Numero = i + 1, Nombre = $"Foto {i + 1}: {NombreArchivo(ruta)}", Seleccionada = i == _indice
            }).ToList();
        }

        private void Miniatura_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Miniatura m && m.Numero - 1 != _indice)
                Mostrar(m.Numero - 1);
        }

        private void Anterior_Click(object sender, RoutedEventArgs e) => Mostrar(_indice - 1);
        private void Siguiente_Click(object sender, RoutedEventArgs e) => Mostrar(_indice + 1);

        // Las flechas del teclado pasan de foto (salvo si el foco está en un campo de texto).
        private void Vista_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox) return;
            if (e.Key == Key.Left && _indice > 0) { Mostrar(_indice - 1); e.Handled = true; }
            else if (e.Key == Key.Right && _indice < Fotos.Count - 1) { Mostrar(_indice + 1); e.Handled = true; }
        }

        // ===== Añadir fotos (CU-11) =====

        private void AnadirFoto_Click(object sender, RoutedEventArgs e)
        {
            if (!ExisteBungalow()) return;

            var dialogo = new OpenFileDialog
            {
                Title = $"Añadir fotos al {_bungalow.Nombre}",
                Filter = "Imágenes (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png",
                Multiselect = true
            };
            if (dialogo.ShowDialog(VentanaPropietaria) != true) return;

            var noValidas = new List<string>();
            var repetidas = new List<string>();
            int primeraNueva = -1;
            foreach (string ruta in dialogo.FileNames)
            {
                string extension = Path.GetExtension(ruta).ToLowerInvariant();
                if (Fotos.Any(f => string.Equals(f, ruta, StringComparison.OrdinalIgnoreCase)))
                    repetidas.Add(NombreArchivo(ruta));
                else if ((extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                         || RutaImagenConverter.Cargar(ruta, 64) == null)
                    noValidas.Add(NombreArchivo(ruta));
                else
                {
                    if (primeraNueva < 0) primeraNueva = Fotos.Count;
                    Fotos.Add(ruta);
                }
            }

            int anadidas = dialogo.FileNames.Length - noValidas.Count - repetidas.Count;
            if (anadidas > 0) Mostrar(primeraNueva);

            var partes = new List<string>();
            if (anadidas > 0)
                partes.Add(anadidas == 1 ? "Se ha añadido 1 foto a la galería." : $"Se han añadido {anadidas} fotos a la galería.");
            if (repetidas.Count > 0)
                partes.Add("Ya estaban en la galería: " + string.Join(", ", repetidas) + ".");
            if (noValidas.Count > 0)
                partes.Add("No se han podido añadir porque no son imágenes JPG o PNG válidas: "
                           + string.Join(", ", noValidas) + ".");
            MostrarResultado(string.Join(" ", partes), correcto: anadidas > 0 && noValidas.Count == 0);
        }

        // ===== Eliminar foto (CU-12) =====

        private void EliminarFoto_Click(object sender, RoutedEventArgs e)
        {
            if (!ExisteBungalow() || Fotos.Count == 0) return;

            string ruta = Fotos[_indice];
            bool confirmar = DialogoMensaje.Confirmar(VentanaPropietaria,
                "¿Eliminar esta foto?",
                $"Foto {_indice + 1} de {Fotos.Count}: {NombreArchivo(ruta)}\n\n"
                + (_indice == 0 && Fotos.Count > 1
                    ? "Es la foto de portada: la siguiente pasará a ser la portada del bungalow."
                    : "Dejará de aparecer en la galería del bungalow."),
                "Eliminar foto", "Cancelar");
            if (!confirmar) return;

            if (!Fotos.Contains(ruta))
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Foto no disponible",
                    "La foto ya no está en la galería del bungalow.", TipoDialogo.Aviso);
                Mostrar(_indice);
                return;
            }

            Fotos.Remove(ruta);
            Mostrar(_indice);
            MostrarResultado($"Se ha eliminado «{NombreArchivo(ruta)}» de la galería.", correcto: true);
        }

        private bool ExisteBungalow()
        {
            if (RepositorioAlojamientos.Bungalows.Contains(_bungalow)) return true;
            DialogoMensaje.Mostrar(VentanaPropietaria, "Bungalow no disponible",
                $"{_bungalow.Nombre} ya no existe. Vuelve al listado de alojamientos.", TipoDialogo.Aviso);
            return false;
        }

        private void MostrarResultado(string texto, bool correcto)
        {
            AvisoResultado.Background = (Brush)FindResource(correcto ? "Pincel.Salvia100" : "Pincel.AvisoFondo");
            IconoResultado.Text = (string)FindResource(correcto ? "Icono.Correcto" : "Icono.Aviso");
            IconoResultado.Foreground = (Brush)FindResource(correcto ? "Pincel.Bosque700" : "Pincel.Aviso");
            TxtResultado.Text = texto;
            TxtResultado.Foreground = (Brush)FindResource(correcto ? "Pincel.Bosque800" : "Pincel.TextoPrincipal");
            AvisoResultado.Visibility = Visibility.Visible;
        }

        // ===== Volver =====

        private void Volver_Click(object sender, RoutedEventArgs e)
        {
            Navegar(new AlojamientosVista(null, mostrarBungalows: true, destacarBungalow: _bungalow,
                                          abrirFichaBungalow: true));
        }
    }
}
