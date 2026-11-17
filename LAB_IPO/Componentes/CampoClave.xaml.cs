using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace LAB_IPO.Componentes
{
    /// <summary>
    /// Campo de clave reutilizable con botón de ojo para mostrar u ocultar lo escrito.
    /// El texto de ejemplo y el estado de error se indican con CampoAyudante,
    /// igual que en el resto de campos.
    /// </summary>
    public partial class CampoClave : UserControl
    {
        private bool _sincronizando;

        /// <summary>Se lanza cada vez que cambia la clave, esté visible u oculta.</summary>
        public event RoutedEventHandler PasswordChanged;

        public CampoClave()
        {
            InitializeComponent();
        }

        public string Password
        {
            get => CajaClave.Password;
            set => CajaClave.Password = value ?? string.Empty;
        }

        public bool ClaveVisible => BtnVerClave.IsChecked == true;

        public void Clear()
        {
            CajaClave.Clear();
        }

        /// <summary>Pone el foco en la caja que se esté viendo, con el cursor al final.</summary>
        public void Enfocar()
        {
            if (ClaveVisible)
            {
                CajaClaveVisible.Focus();
                CajaClaveVisible.CaretIndex = CajaClaveVisible.Text.Length;
            }
            else
            {
                CajaClave.Focus();
                MoverCursorAlFinal(CajaClave);
            }
        }

        private void CajaClave_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_sincronizando) return;
            _sincronizando = true;
            CajaClaveVisible.Text = CajaClave.Password;
            _sincronizando = false;
            PasswordChanged?.Invoke(this, e);
        }

        private void CajaClaveVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_sincronizando) return;
            _sincronizando = true;
            CajaClave.Password = CajaClaveVisible.Text;
            _sincronizando = false;
            PasswordChanged?.Invoke(this, e);
        }

        private void BtnVerClave_Cambiado(object sender, RoutedEventArgs e)
        {
            bool mostrar = ClaveVisible;
            CajaClaveVisible.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
            CajaClave.Visibility = mostrar ? Visibility.Collapsed : Visibility.Visible;
            BtnVerClave.ToolTip = mostrar ? "Ocultar la clave" : "Mostrar la clave";
            Enfocar();
        }

        // PasswordBox no permite colocar el cursor desde fuera; su método Select es interno.
        private static void MoverCursorAlFinal(PasswordBox caja)
        {
            typeof(PasswordBox)
                .GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(caja, new object[] { caja.Password.Length, 0 });
        }
    }
}
