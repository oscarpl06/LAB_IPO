using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG13a · Listado básico de actividades y talleres. CU-24: desde aquí se crea una actividad.
    /// Los filtros y la consulta llegarán con el CU-27; Editar y Eliminar, con sus casos de uso.
    /// </summary>
    public partial class ActividadesVista : VistaBase
    {
        public override string OpcionMenu => "Actividades";

        public ActividadesVista() : this(null, null)
        {
        }

        /// <param name="mensajeGuardado">Aviso de confirmación sobre la tabla (actividad creada).</param>
        /// <param name="destacar">Actividad que se resalta y se lleva a la vista.</param>
        public ActividadesVista(string mensajeGuardado, Actividad destacar)
        {
            InitializeComponent();

            ListaActividades.ItemsSource = RepositorioActividades.Actividades;
            int total = RepositorioActividades.Actividades.Count;
            TxtRecuento.Text = total == 1 ? "1 actividad programada" : $"{total} actividades programadas";
            PanelSinActividades.Visibility = total == 0 ? Visibility.Visible : Visibility.Collapsed;

            if (!string.IsNullOrEmpty(mensajeGuardado))
            {
                TxtAvisoGuardado.Text = mensajeGuardado;
                AvisoGuardado.Visibility = Visibility.Visible;
            }

            if (destacar != null)
                Loaded += (s, e) => Dispatcher.BeginInvoke(new Action(() => Destacar(destacar)),
                    System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>Resalta la fila de la actividad y la lleva a la vista.</summary>
        private void Destacar(Actividad actividad)
        {
            if (!(ListaActividades.ItemContainerGenerator.ContainerFromItem(actividad) is ContentPresenter contenedor)) return;
            contenedor.ApplyTemplate();
            if (contenedor.ContentTemplate?.FindName("Fila", contenedor) is Border fila)
            {
                fila.Background = (Brush)FindResource("Pincel.Salvia100");
                fila.BringIntoView();
            }
        }

        private void NuevaActividad_Click(object sender, RoutedEventArgs e)
        {
            Navegar(new FormularioActividadVista());
        }
    }
}
