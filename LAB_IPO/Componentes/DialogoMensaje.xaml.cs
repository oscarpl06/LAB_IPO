using System.Windows;
using System.Windows.Media;

namespace LAB_IPO.Componentes
{
    public enum TipoDialogo
    {
        Informacion,
        Aviso,
        Error,
        Pregunta
    }

    /// <summary>
    /// Cuadro de diálogo reutilizable con el estilo de la aplicación.
    /// Sustituye a MessageBox para avisos, ayudas y confirmaciones.
    /// </summary>
    public partial class DialogoMensaje : VentanaBase
    {
        private DialogoMensaje()
        {
            InitializeComponent();
        }

        /// <summary>Muestra un mensaje con un único botón "Aceptar".</summary>
        public static void Mostrar(Window propietario, string titulo, string mensaje,
                                   TipoDialogo tipo = TipoDialogo.Informacion)
        {
            Crear(propietario, titulo, mensaje, tipo, "Aceptar", null).ShowDialog();
        }

        /// <summary>
        /// Pide confirmación al usuario. Devuelve true solo si pulsa el botón de confirmar.
        /// </summary>
        public static bool Confirmar(Window propietario, string titulo, string mensaje,
                                     string textoConfirmar = "Aceptar", string textoCancelar = "Cancelar")
        {
            return Crear(propietario, titulo, mensaje, TipoDialogo.Pregunta, textoConfirmar, textoCancelar)
                       .ShowDialog() == true;
        }

        private static DialogoMensaje Crear(Window propietario, string titulo, string mensaje,
                                            TipoDialogo tipo, string textoAceptar, string textoCancelar)
        {
            var dialogo = new DialogoMensaje();
            if (propietario != null && propietario.IsVisible)
                dialogo.Owner = propietario;
            else
                dialogo.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            dialogo.TxtTitulo.Text = titulo;
            dialogo.TxtMensaje.Text = mensaje;
            dialogo.BtnAceptar.Content = textoAceptar;

            if (textoCancelar != null)
            {
                dialogo.BtnCancelar.Content = textoCancelar;
                dialogo.BtnCancelar.Visibility = Visibility.Visible;
            }

            dialogo.AplicarTipo(tipo);
            return dialogo;
        }

        private void AplicarTipo(TipoDialogo tipo)
        {
            string icono, primerPlano, fondo;
            switch (tipo)
            {
                case TipoDialogo.Aviso:
                    icono = "Icono.Aviso"; primerPlano = "Pincel.Aviso"; fondo = "Pincel.AvisoFondo";
                    break;
                case TipoDialogo.Error:
                    icono = "Icono.Error"; primerPlano = "Pincel.Error"; fondo = "Pincel.ErrorFondo";
                    break;
                case TipoDialogo.Pregunta:
                    icono = "Icono.Pregunta"; primerPlano = "Pincel.Bosque700"; fondo = "Pincel.Salvia100";
                    break;
                default:
                    icono = "Icono.Info"; primerPlano = "Pincel.Info"; fondo = "Pincel.InfoFondo";
                    break;
            }

            TxtIcono.Text = (string)FindResource(icono);
            TxtIcono.Foreground = (Brush)FindResource(primerPlano);
            FondoIcono.Background = (Brush)FindResource(fondo);
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
