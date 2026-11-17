using System.Windows;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG3 · Listado de parcelas. Desde aquí se abre el alta de parcela (CU-03).
    /// Filtros, Ver, Editar, Eliminar, Bungalows y Tipos y tarifas llegarán con sus casos de uso.
    /// </summary>
    public partial class AlojamientosVista : VistaBase
    {
        public override string OpcionMenu => "Alojamientos";

        public AlojamientosVista() : this(null)
        {
        }

        /// <param name="mensajeGuardado">Si se indica, muestra un aviso de confirmación sobre la tabla.</param>
        public AlojamientosVista(string mensajeGuardado)
        {
            InitializeComponent();
            ListaParcelas.ItemsSource = RepositorioAlojamientos.Parcelas;

            int total = RepositorioAlojamientos.Parcelas.Count;
            TxtRecuento.Text = total == 1 ? "1 parcela encontrada" : $"{total} parcelas encontradas";
            TxtPaginacion.Text = total == 0
                ? "No hay parcelas registradas"
                : $"Mostrando 1-{total} de {total} parcelas";

            if (!string.IsNullOrEmpty(mensajeGuardado))
            {
                TxtAvisoGuardado.Text = mensajeGuardado;
                AvisoGuardado.Visibility = Visibility.Visible;
                // La parcela nueva se añade al final: se baja hasta ella para que se vea.
                Loaded += (s, e) => DesplazamientoParcelas.ScrollToEnd();
            }
        }

        private void NuevaParcela_Click(object sender, RoutedEventArgs e)
        {
            Navegar(new FormularioParcelaVista());
        }
    }
}
