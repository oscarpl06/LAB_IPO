using System.Windows;
using System.Windows.Controls;
using LAB_IPO.Componentes;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// CU-27 · Ficha de consulta de una actividad (RF-29): horario, cupos, destinatarios,
    /// precio, área, descripción, materiales y monitores. Se muestra en el panel lateral.
    /// </summary>
    public partial class FichaActividad : UserControl
    {
        public FichaActividad(Actividad actividad)
        {
            InitializeComponent();
            DataContext = actividad;
            AvisoSinMonitores.Visibility = actividad.TieneMonitores ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as INavegador)?.CerrarPanelLateral();
        }
    }
}
