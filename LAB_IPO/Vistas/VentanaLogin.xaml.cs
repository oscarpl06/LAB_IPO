using System.Windows;
using System.Windows.Input;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Servicios;

namespace LAB_IPO.Vistas
{
    /// <summary>
    /// CU-01 · Iniciar sesión (PG1). Pide identificación y clave, las comprueba
    /// y, si son correctas, abre la ventana principal con los datos del usuario.
    /// </summary>
    public partial class VentanaLogin : VentanaBase
    {
        private const string MensajeCamposVacios = "Introduce tu usuario y tu clave para continuar.";
        private const string MensajeCredenciales = "Usuario o clave incorrectos. Inténtalo de nuevo.";

        public VentanaLogin()
        {
            InitializeComponent();
            InputBindings.Add(new KeyBinding(new ComandoSimple(MostrarAyuda), Key.F1, ModifierKeys.None));
        }

        private void Entrar_Click(object sender, RoutedEventArgs e)
        {
            string identificacion = TxtUsuario.Text.Trim();
            string clave = TxtClave.Password;

            bool faltaUsuario = identificacion.Length == 0;
            bool faltaClave = clave.Length == 0;
            if (faltaUsuario || faltaClave)
            {
                MostrarError(MensajeCamposVacios, faltaUsuario, faltaClave);
                (faltaUsuario ? (UIElement)TxtUsuario : TxtClave).Focus();
                return;
            }

            var usuario = RepositorioUsuarios.Autenticar(identificacion, clave);
            if (usuario == null)
            {
                MostrarError(MensajeCredenciales, true, true);
                TxtClave.Clear();
                TxtClave.Focus();
                return;
            }

            Sesion.Iniciar(usuario);

            var principal = new VentanaPrincipal();
            Application.Current.MainWindow = principal;
            principal.Show();
            Close();
        }

        private void MostrarError(string mensaje, bool marcarUsuario, bool marcarClave)
        {
            TxtError.Text = mensaje;
            PanelError.Visibility = Visibility.Visible;
            CampoAyudante.SetTieneError(TxtUsuario, marcarUsuario);
            CampoAyudante.SetTieneError(TxtClave, marcarClave);
        }

        // Al volver a escribir se retira el aviso, porque ya no se refiere a lo que hay en pantalla.
        private void Campo_Cambiado(object sender, RoutedEventArgs e)
        {
            if (PanelError.Visibility != Visibility.Visible) return;

            if (sender is DependencyObject campo && CampoAyudante.GetTieneError(campo))
                CampoAyudante.SetTieneError(campo, false);

            if (!CampoAyudante.GetTieneError(TxtUsuario) && !CampoAyudante.GetTieneError(TxtClave))
                PanelError.Visibility = Visibility.Hidden;
        }

        private void ClaveOlvidada_Click(object sender, RoutedEventArgs e)
        {
            DialogoMensaje.Mostrar(this, "Recuperar el acceso",
                "Por seguridad, la clave solo se puede restablecer desde la administración del camping. " +
                "Ponte en contacto con la administración para que te asignen una clave nueva.");
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MostrarAyuda()
        {
            DialogoMensaje.Mostrar(this, "Ayuda · Iniciar sesión",
                "1. Escribe tu identificación de usuario (por ejemplo, federico.g).\n" +
                "2. Escribe tu clave. Recuerda que distingue mayúsculas y minúsculas.\n" +
                "3. Pulsa «Entrar» o la tecla Intro.\n\n" +
                "Si los datos no son correctos verás un aviso en rojo bajo la clave. " +
                "Deja el cursor sobre un campo para ver qué debes escribir en él.");
        }
    }
}
