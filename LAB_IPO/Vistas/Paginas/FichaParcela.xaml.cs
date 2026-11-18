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

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as INavegador)?.CerrarPanelLateral();
        }
    }
}
