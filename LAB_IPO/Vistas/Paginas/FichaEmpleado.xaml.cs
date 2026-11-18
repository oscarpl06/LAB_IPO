using System.Windows;
using System.Windows.Controls;
using LAB_IPO.Componentes;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// CU-22 · Ficha de consulta de un empleado temporal (RF-24). Se muestra en el panel
    /// lateral de la ventana principal: foto, contacto, idiomas, formación y disponibilidad.
    /// </summary>
    public partial class FichaEmpleado : UserControl
    {
        public FichaEmpleado(Empleado empleado)
        {
            InitializeComponent();
            DataContext = empleado;
            Foto.Nombre = empleado.NombreCompleto;
            Foto.Foto = RutaImagenConverter.Cargar(empleado.RutaFoto, 300);
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as INavegador)?.CerrarPanelLateral();
        }
    }
}
