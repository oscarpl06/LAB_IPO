using System;
using System.Windows.Input;

namespace LAB_IPO.Componentes
{
    /// <summary>Comando mínimo para atajos de teclado (por ejemplo, F1 para la ayuda).</summary>
    public class ComandoSimple : ICommand
    {
        private readonly Action _accion;

        public ComandoSimple(Action accion)
        {
            _accion = accion ?? throw new ArgumentNullException(nameof(accion));
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _accion();
    }
}
