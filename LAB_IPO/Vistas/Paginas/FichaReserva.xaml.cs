using System.Windows;
using System.Windows.Controls;
using LAB_IPO.Componentes;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// CU-17 · Ficha de consulta de una reserva (RF-19): estancia, titular, solicitudes
    /// y el alojamiento con su foto. Se muestra en el panel lateral de la ventana principal.
    /// </summary>
    public partial class FichaReserva : UserControl
    {
        public FichaReserva(Reserva reserva)
        {
            InitializeComponent();
            DataContext = reserva;
            FotoAlojamiento.ImageSource = RutaImagenConverter.Cargar(reserva.FotoAlojamiento, 900);
        }

        // CU-18 · Abre el formulario en modo edición; al navegar, la ventana cierra el panel lateral.
        private void Editar_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Reserva reserva)
                (Window.GetWindow(this) as INavegador)?.Navegar(new FormularioReservaVista(reserva));
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as INavegador)?.CerrarPanelLateral();
        }
    }
}
