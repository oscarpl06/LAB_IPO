using System.Windows;
using System.Windows.Controls;

namespace LAB_IPO.Componentes
{
    /// <summary>
    /// Contrato entre la ventana principal y las vistas que muestra en su zona de contenido.
    /// </summary>
    public interface INavegador
    {
        /// <summary>Muestra otra vista y marca su opción del menú lateral.</summary>
        void Navegar(VistaBase vista);
    }

    /// <summary>
    /// Base de las pantallas que se muestran dentro de la ventana principal (PG2 en adelante).
    /// Cada vista indica qué opción del menú lateral le corresponde.
    /// </summary>
    public class VistaBase : UserControl
    {
        /// <summary>Texto de la opción del menú lateral que se marca al mostrar esta vista.</summary>
        public virtual string OpcionMenu => "Inicio";

        /// <summary>
        /// Se consulta antes de abandonar la vista. Los formularios lo usan para
        /// pedir confirmación si hay cambios sin guardar.
        /// </summary>
        public virtual bool PuedeSalir() => true;

        protected void Navegar(VistaBase vista)
        {
            (Window.GetWindow(this) as INavegador)?.Navegar(vista);
        }

        protected Window VentanaPropietaria => Window.GetWindow(this);
    }
}
