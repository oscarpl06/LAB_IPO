using System.Linq;
using System.Windows;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG11 · Listado de personal y monitores. Desde aquí se da de alta un empleado (CU-20).
    /// Filtros, Ver, Editar y Dar de baja llegarán con sus casos de uso.
    /// </summary>
    public partial class PersonalVista : VistaBase
    {
        public override string OpcionMenu => "Personal";

        public PersonalVista() : this(null)
        {
        }

        /// <param name="mensajeGuardado">Si se indica, muestra un aviso de confirmación sobre la tabla.</param>
        public PersonalVista(string mensajeGuardado)
        {
            InitializeComponent();

            var activos = RepositorioPersonal.Empleados.Where(e => e.Activo).ToList();
            ListaEmpleados.ItemsSource = activos;
            TxtRecuento.Text = activos.Count == 1 ? "1 empleado registrado" : $"{activos.Count} empleados registrados";

            if (!string.IsNullOrEmpty(mensajeGuardado))
            {
                TxtAvisoGuardado.Text = mensajeGuardado;
                AvisoGuardado.Visibility = Visibility.Visible;
                // El empleado nuevo se añade al final: se baja hasta él para que se vea.
                Loaded += (s, e) => DesplazamientoEmpleados.ScrollToEnd();
            }
        }

        private void NuevoEmpleado_Click(object sender, RoutedEventArgs e)
        {
            Navegar(new FormularioEmpleadoVista());
        }
    }
}
