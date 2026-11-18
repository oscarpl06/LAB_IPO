using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG13a · Listado de actividades y talleres.
    /// CU-24: desde aquí se crea una actividad.
    /// CU-27: búsqueda, filtros por área y público, y ficha de la actividad en el panel lateral (RF-29).
    /// Editar y Eliminar llegarán con sus casos de uso.
    /// </summary>
    public partial class ActividadesVista : VistaBase
    {
        private const string TodasLasZonas = "Todas las zonas";
        private const string TodosLosPublicos = "Todos";

        private readonly ListCollectionView _vista;

        public override string OpcionMenu => "Actividades";

        public ActividadesVista() : this(null, null)
        {
        }

        /// <param name="mensajeGuardado">Aviso de confirmación sobre la tabla (actividad creada).</param>
        /// <param name="destacar">Actividad que se resalta y se lleva a la vista.</param>
        public ActividadesVista(string mensajeGuardado, Actividad destacar)
        {
            InitializeComponent();

            _vista = new ListCollectionView(RepositorioActividades.Actividades);
            ListaActividades.ItemsSource = _vista;

            CmbFiltroArea.ItemsSource = new[] { TodasLasZonas }.Concat(RepositorioActividades.Areas).ToList();
            CmbFiltroArea.SelectedIndex = 0;
            CmbFiltroPublico.ItemsSource = new[] { TodosLosPublicos }.Concat(RepositorioActividades.Publicos).ToList();
            CmbFiltroPublico.SelectedIndex = 0;
            AplicarFiltros();

            if (!string.IsNullOrEmpty(mensajeGuardado))
            {
                TxtAvisoGuardado.Text = mensajeGuardado;
                AvisoGuardado.Visibility = Visibility.Visible;
            }

            if (destacar != null)
                Loaded += (s, e) => Dispatcher.BeginInvoke(new Action(() => Destacar(destacar)),
                    System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>Resalta la fila de la actividad y la lleva a la vista.</summary>
        private void Destacar(Actividad actividad)
        {
            if (!(ListaActividades.ItemContainerGenerator.ContainerFromItem(actividad) is ContentPresenter contenedor)) return;
            contenedor.ApplyTemplate();
            if (contenedor.ContentTemplate?.FindName("Fila", contenedor) is Border fila)
            {
                fila.Background = (Brush)FindResource("Pincel.Salvia100");
                fila.BringIntoView();
            }
        }

        // ===== Filtros (CU-27) =====

        private void Filtrar_Click(object sender, RoutedEventArgs e) => AplicarFiltros();

        private void Buscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AplicarFiltros();
                e.Handled = true;
            }
        }

        private void Limpiar_Click(object sender, RoutedEventArgs e)
        {
            TxtBuscar.Clear();
            CmbFiltroArea.SelectedIndex = 0;
            CmbFiltroPublico.SelectedIndex = 0;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            string texto = SinTildes(TxtBuscar.Text.Trim());
            string area = CmbFiltroArea.SelectedItem as string ?? TodasLasZonas;
            string publico = CmbFiltroPublico.SelectedItem as string ?? TodosLosPublicos;

            _vista.Filter = o =>
            {
                var a = (Actividad)o;
                if (area != TodasLasZonas && a.Area != area) return false;
                if (publico != TodosLosPublicos && a.Publico != publico) return false;
                return texto.Length == 0
                       || SinTildes(a.Nombre).IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0;
            };

            int visibles = _vista.Count;
            int total = RepositorioActividades.Actividades.Count;
            bool sinFiltros = texto.Length == 0 && area == TodasLasZonas && publico == TodosLosPublicos;
            TxtRecuento.Text = sinFiltros
                ? (total == 1 ? "1 actividad programada" : $"{total} actividades programadas")
                : (visibles == 1 ? "1 actividad encontrada" : $"{visibles} actividades encontradas");

            // Sin actividades: se distingue si no hay ninguna o si los filtros las ocultan.
            TxtSinActividades.Text = total == 0
                ? "Todavía no hay actividades programadas"
                : "No hay actividades que coincidan con la búsqueda";
            TxtSinActividadesAyuda.Text = total == 0
                ? "Pulsa «Nueva actividad» para crear la primera."
                : "Prueba con otro nombre, área o público.";
            BtnQuitarFiltros.Visibility = total == 0 ? Visibility.Collapsed : Visibility.Visible;
            PanelSinActividades.Visibility = visibles == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static string SinTildes(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;
            var sb = new StringBuilder();
            foreach (char c in texto.Normalize(NormalizationForm.FormD))
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // ===== Ficha de la actividad (CU-27) =====

        private void Ver_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Actividad actividad)) return;

            if (!RepositorioActividades.Actividades.Contains(actividad))
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Actividad no disponible",
                    "La actividad seleccionada ya no existe. Actualiza el listado y vuelve a intentarlo.",
                    TipoDialogo.Aviso);
                return;
            }

            var ficha = new FichaActividad(actividad);
            ficha.MonitoresCambiados += (s, a) => _vista.Refresh();   // CU-28: columna Monitor/es
            Navegador?.AbrirPanelLateral(ficha);
        }

        // ===== Modificar (CU-26) y dar de baja (CU-25) =====

        private bool Existe(Actividad actividad)
        {
            if (RepositorioActividades.Actividades.Contains(actividad)) return true;
            DialogoMensaje.Mostrar(VentanaPropietaria, "Actividad no disponible",
                "La actividad seleccionada ya no existe. Actualiza el listado y vuelve a intentarlo.", TipoDialogo.Aviso);
            return false;
        }

        private void Editar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Actividad actividad && Existe(actividad))
                Navegar(new FormularioActividadVista(actividad));
        }

        private void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Actividad a) || !Existe(a)) return;

            string detalle = $"{a.DiasTexto} · {a.HorarioTexto} · {a.Area} · {a.Publico}\n"
                           + $"Cupo: {a.CupoTexto} · Precio: {a.PrecioTexto}\n"
                           + $"Monitores: {a.MonitoresTexto}\n\n"
                           + "Dejará de aparecer en la programación del camping.";
            if (!DialogoMensaje.Confirmar(VentanaPropietaria, $"¿Dar de baja «{a.Nombre}»?", detalle,
                    "Dar de baja", "Cancelar"))
                return;

            try
            {
                RepositorioActividades.DarDeBaja(a);
            }
            catch (InvalidOperationException ex)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No se ha podido dar de baja la actividad",
                    ex.Message + " Sus datos se conservan.", TipoDialogo.Error);
                return;
            }

            Navegador?.CerrarPanelLateral();
            AplicarFiltros();
            TxtAvisoGuardado.Text = $"Actividad «{a.Nombre}» dada de baja correctamente.";
            AvisoGuardado.Visibility = Visibility.Visible;
        }

        // ===== Alta (CU-24) =====

        private void NuevaActividad_Click(object sender, RoutedEventArgs e)
        {
            Navegar(new FormularioActividadVista());
        }
    }
}
