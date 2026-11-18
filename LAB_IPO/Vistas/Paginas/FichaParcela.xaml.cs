using System.Windows;
using System.Windows.Controls;
using LAB_IPO.Componentes;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// CU-06 · Ficha de consulta de una parcela. Se muestra en el panel lateral
    /// de la ventana principal, a toda la altura de su borde derecho.
    /// </summary>
    public partial class FichaParcela : UserControl
    {
        public FichaParcela(Parcela parcela)
        {
            InitializeComponent();
            DataContext = parcela;
        }

        // Abre el formulario en modo edición; al navegar, la ventana cierra el panel lateral.
        private void Editar_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Parcela parcela)
                (Window.GetWindow(this) as INavegador)?.Navegar(new FormularioParcelaVista(parcela));
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as INavegador)?.CerrarPanelLateral();
        }
    }
}
