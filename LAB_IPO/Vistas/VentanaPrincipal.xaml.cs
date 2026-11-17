using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using LAB_IPO.Componentes;
using LAB_IPO.Servicios;

namespace LAB_IPO.Vistas
{
    /// <summary>
    /// PG2 · Ventana principal. Tras iniciar sesión muestra los datos del usuario,
    /// su foto de perfil y la fecha de su último acceso (RF-32).
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

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
