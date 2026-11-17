using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace LAB_IPO.Componentes
{
    /// <summary>
    /// Convierte una ruta de imagen (pack:// o archivo del disco) en una imagen
    /// lista para mostrar. Si se indica un ancho como parámetro, la decodifica a ese
    /// tamaño para que las miniaturas de los listados ocupen poca memoria.
    /// </summary>
    public class RutaImagenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Cargar(value as string, parameter);
        }

        public static BitmapImage Cargar(string ruta, object anchoDecodificado = null)
        {
            if (string.IsNullOrWhiteSpace(ruta)) return null;

            try
            {
                Uri uri = ruta.StartsWith("pack://", StringComparison.OrdinalIgnoreCase)
                    ? new Uri(ruta, UriKind.Absolute)
                    : new Uri(Path.GetFullPath(ruta), UriKind.Absolute);

                var imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.UriSource = uri;
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                if (anchoDecodificado != null && int.TryParse(anchoDecodificado.ToString(), out int ancho))
                    imagen.DecodePixelWidth = ancho;
                imagen.EndInit();
                imagen.Freeze();
                return imagen;
            }
            catch (Exception)
            {
                // Archivo inexistente o formato no válido: se muestra el hueco sin foto.
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
