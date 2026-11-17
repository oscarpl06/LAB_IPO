using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LAB_IPO.Componentes;
using LAB_IPO.Servicios;
using LAB_IPO.Vistas.Paginas;

namespace LAB_IPO.Vistas
{
    /// <summary>
    /// PG2 · Ventana principal. Tras iniciar sesión muestra los datos del usuario,
    /// su foto de perfil y la fecha de su último acceso (RF-32). Desde aquí se
    /// cierra la sesión o se sale de la aplicación con confirmación (CU-02, RF-33).
    /// El menú lateral cambia la vista que se muestra en la zona de contenido.
    /// </summary>
    public partial class VentanaPrincipal : VentanaBase, INavegador
    {
        private static readonly CultureInfo Es = new CultureInfo("es-ES");

        public VentanaPrincipal()
        {
            InitializeComponent();
            MostrarUsuario();
            Navegar(new InicioVista());
        }

        private void MostrarUsuario()
        {
            var usuario = Sesion.UsuarioActual;
            if (usuario == null) return;

            RunNombre.Text = usuario.NombreCompleto;
            RunRol.Text = usuario.Rol;
            Avatar.Nombre = usuario.NombreCompleto;
            if (!string.IsNullOrEmpty(usuario.RutaFoto))
                Avatar.Foto = new BitmapImage(new Uri(usuario.RutaFoto, UriKind.RelativeOrAbsolute));

            TxtUltimoAcceso.Text = Sesion.AccesoAnterior.HasValue
                ? "Último acceso: " + Sesion.AccesoAnterior.Value.ToString("dd/MM/yyyy  ·  HH:mm", Es)
                : "Primer acceso a la aplicación";
        }

        // ===== Navegación =====

        private VistaBase VistaActual => Contenido.Content as VistaBase;

        public void Navegar(VistaBase vista)
        {
            if (VistaActual != null && !VistaActual.PuedeSalir())
            {
                MarcarMenu(VistaActual.OpcionMenu);
                return;
            }

            Contenido.Content = vista;
            MarcarMenu(vista.OpcionMenu);
        }

        // Evita que marcar el menú desde el código vuelva a navegar.
        private bool _marcandoMenu;

        private void MarcarMenu(string opcion)
        {
            var boton = new[] { MenuInicio, MenuAlojamientos }
                .FirstOrDefault(b => (string)b.Content == opcion);
            if (boton == null || boton.IsChecked == true) return;

            _marcandoMenu = true;
            boton.IsChecked = true;
            _marcandoMenu = false;
        }

        private void Menu_Checked(object sender, RoutedEventArgs e)
        {
            if (_marcandoMenu || Contenido == null) return;

            var opcion = (string)((RadioButton)sender).Content;
            switch (opcion)
            {
                case "Alojamientos": Navegar(new AlojamientosVista()); break;
                default: Navegar(new InicioVista()); break;
            }
        }

        // Pulsar la opción ya marcada desde un formulario vuelve a su listado.
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var opcion = (string)((RadioButton)sender).Content;
            if (VistaActual is FormularioParcelaVista && opcion == "Alojamientos")
                Navegar(new AlojamientosVista());
        }

        // ===== Sesión y cierre =====

        // Se activa al cerrar sesión, para no pedir la confirmación de salida.
        private bool _cerrandoSesion;

        /// <summary>Cierra la sesión y vuelve a la ventana de login sin cerrar la aplicación.</summary>
        private void CerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            if (VistaActual != null && !VistaActual.PuedeSalir()) return;

            bool confirmar = DialogoMensaje.Confirmar(this,
                "¿Cerrar sesión?",
                "Volverás a la pantalla de acceso y tendrás que introducir de nuevo tu usuario y tu clave.",
                "Cerrar sesión", "Cancelar");
            if (!confirmar) return;

            Sesion.Cerrar();

            var login = new VentanaLogin();
            Application.Current.MainWindow = login;
            login.Show();

            _cerrandoSesion = true;
            Close();
        }

        /// <summary>
        /// CU-02 · Cerrar la aplicación. Al cerrar la ventana principal (X, Alt+F4)
        /// se pide confirmación, porque los datos no se guardan al salir (RF-36).
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_cerrandoSesion || e.Cancel) return;

            bool salir = DialogoMensaje.Confirmar(this,
                "¿Salir de Camping Timoteo?",
                "Se cerrará la aplicación y se perderán los cambios que no se hayan guardado.",
                "Salir", "Cancelar");

            if (!salir)
                e.Cancel = true;
            else
                Sesion.Cerrar();
        }
    }
}
