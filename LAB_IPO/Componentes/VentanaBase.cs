using System.Windows;
using System.Windows.Input;

namespace LAB_IPO.Componentes
{
    /// <summary>
    /// Ventana base de la aplicación: aplica el marco propio "Ventana.Natural"
    /// (barra de título personalizada) y conecta sus botones de minimizar,
    /// maximizar, restaurar y cerrar. Todas las ventanas heredan de ella.
    /// </summary>
    public class VentanaBase : Window
    {
        public VentanaBase()
        {
            SetResourceReference(StyleProperty, "Ventana.Natural");

            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand,
                (s, e) => SystemCommands.MinimizeWindow(this)));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand,
                (s, e) => SystemCommands.MaximizeWindow(this)));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand,
                (s, e) => SystemCommands.RestoreWindow(this)));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand,
                (s, e) => SystemCommands.CloseWindow(this)));
        }
    }
}
