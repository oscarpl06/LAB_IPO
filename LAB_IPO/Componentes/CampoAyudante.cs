using System.Windows;
using System.Windows.Controls;

namespace LAB_IPO.Componentes
{
    /// <summary>
    /// Propiedades adjuntas para los campos de formulario de la aplicación:
    /// texto de ejemplo (placeholder), estado de error y, en las claves,
    /// si el usuario ya ha escrito algo (PasswordBox no expone su contenido para enlazarlo).
    /// </summary>
    public static class CampoAyudante
    {
        public static readonly DependencyProperty TextoEjemploProperty =
            DependencyProperty.RegisterAttached("TextoEjemplo", typeof(string), typeof(CampoAyudante),
                new PropertyMetadata(string.Empty));

        public static string GetTextoEjemplo(DependencyObject d) => (string)d.GetValue(TextoEjemploProperty);
        public static void SetTextoEjemplo(DependencyObject d, string valor) => d.SetValue(TextoEjemploProperty, valor);

        public static readonly DependencyProperty TieneErrorProperty =
            DependencyProperty.RegisterAttached("TieneError", typeof(bool), typeof(CampoAyudante),
                new PropertyMetadata(false));

        public static bool GetTieneError(DependencyObject d) => (bool)d.GetValue(TieneErrorProperty);
        public static void SetTieneError(DependencyObject d, bool valor) => d.SetValue(TieneErrorProperty, valor);

        // Se activa desde el estilo de PasswordBox para seguir sus cambios.
        public static readonly DependencyProperty VigilarClaveProperty =
            DependencyProperty.RegisterAttached("VigilarClave", typeof(bool), typeof(CampoAyudante),
                new PropertyMetadata(false, AlCambiarVigilarClave));

        public static bool GetVigilarClave(DependencyObject d) => (bool)d.GetValue(VigilarClaveProperty);
        public static void SetVigilarClave(DependencyObject d, bool valor) => d.SetValue(VigilarClaveProperty, valor);

        private static readonly DependencyPropertyKey TieneTextoKey =
            DependencyProperty.RegisterAttachedReadOnly("TieneTexto", typeof(bool), typeof(CampoAyudante),
                new PropertyMetadata(false));

        public static readonly DependencyProperty TieneTextoProperty = TieneTextoKey.DependencyProperty;

        public static bool GetTieneTexto(DependencyObject d) => (bool)d.GetValue(TieneTextoProperty);

        private static void AlCambiarVigilarClave(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is PasswordBox clave)) return;

            clave.PasswordChanged -= AlCambiarClave;
            if ((bool)e.NewValue)
            {
                clave.PasswordChanged += AlCambiarClave;
                clave.SetValue(TieneTextoKey, clave.SecurePassword.Length > 0);
            }
        }

        private static void AlCambiarClave(object sender, RoutedEventArgs e)
        {
            var clave = (PasswordBox)sender;
            clave.SetValue(TieneTextoKey, clave.SecurePassword.Length > 0);
        }
    }
}
