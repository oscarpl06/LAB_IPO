using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LAB_IPO.Componentes
{
    /// <summary>
    /// Foto de perfil circular. Si no hay foto, muestra las iniciales del nombre.
    /// Se reutilizará en la cabecera, en la ficha de empleados y en los monitores.
    /// </summary>
    public partial class AvatarUsuario : UserControl
    {
        public static readonly DependencyProperty NombreProperty =
            DependencyProperty.Register(nameof(Nombre), typeof(string), typeof(AvatarUsuario),
                new PropertyMetadata(string.Empty, (d, e) => ((AvatarUsuario)d).Actualizar()));

        public static readonly DependencyProperty FotoProperty =
            DependencyProperty.Register(nameof(Foto), typeof(ImageSource), typeof(AvatarUsuario),
                new PropertyMetadata(null, (d, e) => ((AvatarUsuario)d).Actualizar()));

        public string Nombre
        {
            get => (string)GetValue(NombreProperty);
            set => SetValue(NombreProperty, value);
        }

        public ImageSource Foto
        {
            get => (ImageSource)GetValue(FotoProperty);
            set => SetValue(FotoProperty, value);
        }

        public AvatarUsuario()
        {
            InitializeComponent();
            SizeChanged += (s, e) => TxtIniciales.FontSize = System.Math.Max(9, ActualHeight * 0.38);
        }

        private void Actualizar()
        {
            TxtIniciales.Text = CalcularIniciales(Nombre);
            PincelFoto.ImageSource = Foto;
            CirculoFoto.Visibility = Foto != null ? Visibility.Visible : Visibility.Collapsed;
        }

        private static string CalcularIniciales(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return string.Empty;
            var partes = nombre.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(partes.Take(2).Select(p => char.ToUpperInvariant(p[0])));
        }
    }
}
