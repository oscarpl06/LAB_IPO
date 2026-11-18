using System.Windows;
using LAB_IPO.Componentes;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG15 · Acerca de: autores, fecha, versión y uso académico del prototipo.
    /// «Volver a la aplicación» regresa a la pantalla en la que se estaba.
    /// </summary>
    public partial class AcercaDeVista : VistaSecundaria
    {
        public override string OpcionMenu => "Acerca de";

        public AcercaDeVista() : this(null)
        {
        }

        public AcercaDeVista(VistaBase anterior) : base(anterior)
        {
            InitializeComponent();
        }

        private void Volver_Click(object sender, RoutedEventArgs e) => Volver(new InicioVista());
    }
}
