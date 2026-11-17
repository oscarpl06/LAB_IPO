using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using LAB_IPO.Componentes;
using LAB_IPO.Servicios;

namespace LAB_IPO.Vistas
{
    /// <summary>
    /// PG2 · Ventana principal. Tras iniciar sesión muestra los datos del usuario,
    /// su foto de perfil y la fecha de su último acceso (RF-32). Desde aquí se
    /// cierra la sesión o se sale de la aplicación con confirmación (CU-02, RF-33).
    /// </summary>
    public partial class VentanaPrincipal : VentanaBase
    {
        private static readonly CultureInfo Es = new CultureInfo("es-ES");

        public VentanaPrincipal()
        {
            InitializeComponent();
            MostrarUsuario();
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

            TxtBienvenida.Text = $"Bienvenido, {usuario.Nombre}. Este es el resumen de hoy del camping.";
        }

        // Se activa al cerrar sesión, para no pedir la confirmación de salida.
        private bool _cerrandoSesion;

        /// <summary>Cierra la sesión y vuelve a la ventana de login sin cerrar la aplicación.</summary>
        private void CerrarSesion_Click(object sender, RoutedEventArgs e)
        {
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
