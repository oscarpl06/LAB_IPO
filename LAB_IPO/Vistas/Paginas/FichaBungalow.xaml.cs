using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using LAB_IPO.Componentes;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// CU-10 · Ficha de consulta de un bungalow (RF-09). Se muestra en el panel lateral
    /// de la ventana principal. Las miniaturas cambian la foto que se ve en grande.
    /// </summary>
    public partial class FichaBungalow : UserControl
    {
        public FichaBungalow(Bungalow bungalow)
        {
            InitializeComponent();
            DataContext = bungalow;

            FotoGrande.ImageSource = RutaImagenConverter.Cargar(bungalow.Portada, 900);

            // Con una sola foto no hace falta la tira de miniaturas.
            Miniaturas.ItemsSource = bungalow.Fotos.Count > 1 ? bungalow.Fotos.ToList() : null;
            Miniaturas.Visibility = bungalow.Fotos.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

            // Marca la primera miniatura cuando ya se han generado.
            Loaded += (s, e) =>
            {
                if (Miniaturas.ItemContainerGenerator.ContainerFromIndex(0) is ContentPresenter cp
                    && VisualTreeHelperBuscar<ToggleButton>(cp) is ToggleButton primera)
                    primera.IsChecked = true;
            };
        }

        private void Miniatura_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is string ruta)
                FotoGrande.ImageSource = RutaImagenConverter.Cargar(ruta, 900);
        }

        // Abre el formulario en modo edición; al navegar, la ventana cierra el panel lateral.
        private void Editar_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Bungalow bungalow)
                (Window.GetWindow(this) as INavegador)?.Navegar(new FormularioBungalowVista(bungalow));
        }

        // CU-13 · Abre la galería del bungalow (PG6); al navegar, la ventana cierra el panel lateral.
        private void Galeria_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Bungalow bungalow)
                (Window.GetWindow(this) as INavegador)?.Navegar(new GaleriaBungalowVista(bungalow));
        }

        private void FotoGrande_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) =>
            Galeria_Click(sender, e);

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as INavegador)?.CerrarPanelLateral();
        }

        private static T VisualTreeHelperBuscar<T>(DependencyObject raiz) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(raiz); i++)
            {
                var hijo = System.Windows.Media.VisualTreeHelper.GetChild(raiz, i);
                if (hijo is T encontrado) return encontrado;
                var profundo = VisualTreeHelperBuscar<T>(hijo);
                if (profundo != null) return profundo;
            }
            return null;
        }
    }
}
