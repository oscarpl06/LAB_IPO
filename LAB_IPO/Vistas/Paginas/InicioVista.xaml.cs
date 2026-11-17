using LAB_IPO.Componentes;
using LAB_IPO.Servicios;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>PG2 · Pantalla de inicio de la ventana principal.</summary>
    public partial class InicioVista : VistaBase
    {
        public InicioVista()
        {
            InitializeComponent();
            var nombre = Sesion.UsuarioActual?.Nombre ?? string.Empty;
            TxtBienvenida.Text = $"Bienvenido, {nombre}. Este es el resumen de hoy del camping.";
        }
    }
}
