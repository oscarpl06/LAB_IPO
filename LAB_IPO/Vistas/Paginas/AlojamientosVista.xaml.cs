using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG3 · Listado de parcelas.
    /// CU-03: desde aquí se abre el alta de parcela.
    /// CU-06: filtros de búsqueda y ficha de la parcela en un panel lateral (RF-04, RF-05).
    /// </summary>
    public partial class AlojamientosVista : VistaBase
    {
        /// <summary>Opción de un desplegable de filtro: texto visible y valor (null = todos).</summary>
        private class OpcionFiltro
        {
            public string Texto { get; set; }
            public object Valor { get; set; }

            // El desplegable muestra la opción elegida con ToString().
            public override string ToString() => Texto;
        }

        private readonly ListCollectionView _vista;

        public override string OpcionMenu => "Alojamientos";

        public AlojamientosVista() : this(null)
        {
        }

        /// <param name="mensajeGuardado">Si se indica, muestra un aviso de confirmación sobre la tabla.</param>
        public AlojamientosVista(string mensajeGuardado)
        {
            InitializeComponent();

            // Vista propia de la colección, para que los filtros no afecten a otras pantallas.
            _vista = new ListCollectionView(RepositorioAlojamientos.Parcelas);
            ListaParcelas.ItemsSource = _vista;

            CmbFiltroTipo.ItemsSource = new[] { new OpcionFiltro { Texto = "Todos los tipos" } }
                .Concat(RepositorioAlojamientos.TiposParcela.Select(t => new OpcionFiltro { Texto = t.Nombre, Valor = t }))
                .ToList();
            CmbFiltroEstado.ItemsSource = new[]
            {
                new OpcionFiltro { Texto = "Todos los estados" },
                new OpcionFiltro { Texto = "Libre", Valor = EstadoAlojamiento.Libre },
                new OpcionFiltro { Texto = "Ocupada", Valor = EstadoAlojamiento.Ocupado },
                new OpcionFiltro { Texto = "En reparación", Valor = EstadoAlojamiento.EnReparacion },
                new OpcionFiltro { Texto = "Pendiente de limpieza", Valor = EstadoAlojamiento.PendienteLimpieza }
            };
            CmbFiltroTipo.SelectedIndex = 0;
            CmbFiltroEstado.SelectedIndex = 0;

            ActualizarRecuento();
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape && PanelFicha.Visibility == Visibility.Visible)
                {
                    CerrarFicha();
                    e.Handled = true;
                }
            };

            if (!string.IsNullOrEmpty(mensajeGuardado))
            {
                TxtAvisoGuardado.Text = mensajeGuardado;
                AvisoGuardado.Visibility = Visibility.Visible;
                // La parcela nueva se añade al final: se baja hasta ella para que se vea.
                Loaded += (s, e) =>
                {
                    var ultima = RepositorioAlojamientos.Parcelas.LastOrDefault();
                    if (ultima != null) ListaParcelas.ScrollIntoView(ultima);
                };
            }
        }

        // ===== Filtros =====

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
            CmbFiltroTipo.SelectedIndex = 0;
            CmbFiltroEstado.SelectedIndex = 0;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            string texto = SinTildes(TxtBuscar.Text.Trim());
            var tipo = (CmbFiltroTipo.SelectedItem as OpcionFiltro)?.Valor as TipoParcela;
            var estado = (CmbFiltroEstado.SelectedItem as OpcionFiltro)?.Valor as EstadoAlojamiento?;

            bool sinFiltros = texto.Length == 0 && tipo == null && estado == null;
            _vista.Filter = sinFiltros ? null : new Predicate<object>(o =>
            {
                var p = (Parcela)o;
                if (tipo != null && p.Tipo != tipo) return false;
                if (estado != null && p.Estado != estado) return false;
                if (texto.Length > 0
                    && SinTildes(p.Identificador).IndexOf(texto, StringComparison.OrdinalIgnoreCase) < 0
                    && SinTildes(p.Ubicacion).IndexOf(texto, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
                return true;
            });

            ActualizarRecuento();
        }

        private void ActualizarRecuento()
        {
            int visibles = _vista.Count;
            int total = RepositorioAlojamientos.Parcelas.Count;

            TxtRecuento.Text = visibles == 1 ? "1 parcela encontrada" : $"{visibles} parcelas encontradas";
            TxtPaginacion.Text = visibles == 0
                ? $"Mostrando 0 de {total} parcelas"
                : $"Mostrando {visibles} de {total} parcelas";

            bool vacio = visibles == 0;
            PanelSinResultados.Visibility = vacio ? Visibility.Visible : Visibility.Collapsed;
            TxtSinResultados.Text = total == 0
                ? "Todavía no hay parcelas registradas"
                : "No hay parcelas que coincidan con los filtros";
        }

        /// <summary>Quita las tildes para que "ubicacion" encuentre "Ubicación".</summary>
        private static string SinTildes(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;
            var sb = new StringBuilder();
            foreach (char c in texto.Normalize(NormalizationForm.FormD))
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // ===== Ficha de la parcela (CU-06) =====

        // Parcela cuya ficha está abierta. La ficha solo se abre con el botón "Ver".
        private Parcela _parcelaEnFicha;

        private void Ver_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Parcela parcela)) return;

            if (!RepositorioAlojamientos.Parcelas.Contains(parcela))
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Parcela no disponible",
                    "La parcela seleccionada ya no existe. Actualiza el listado y vuelve a intentarlo.",
                    TipoDialogo.Aviso);
                return;
            }

            _parcelaEnFicha = parcela;
            ListaParcelas.SelectedItem = parcela;   // resalta su fila
            MostrarFicha(parcela);
        }

        // Pulsar una fila o moverse con las flechas no abre la ficha:
        // la fila resaltada es siempre la de la ficha abierta (o ninguna).
        private void ListaParcelas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(ListaParcelas.SelectedItem, _parcelaEnFicha))
                ListaParcelas.SelectedItem = _parcelaEnFicha;
        }

        private void MostrarFicha(Parcela parcela)
        {
            bool yaVisible = PanelFicha.Visibility == Visibility.Visible;
            PanelFicha.DataContext = parcela;
            PanelFicha.Visibility = Visibility.Visible;

            if (!yaVisible)
            {
                var duracion = TimeSpan.FromMilliseconds(220);
                var suavizado = new CubicEase { EasingMode = EasingMode.EaseOut };
                DesplazamientoFicha.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty,
                    new DoubleAnimation(40, 0, duracion) { EasingFunction = suavizado });
                PanelFicha.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duracion));
            }
        }

        private void CerrarFicha_Click(object sender, RoutedEventArgs e) => CerrarFicha();

        private void CerrarFicha()
        {
            _parcelaEnFicha = null;
            ListaParcelas.SelectedItem = null;
            OcultarFicha();
        }

        private void OcultarFicha()
        {
            PanelFicha.Visibility = Visibility.Collapsed;
            PanelFicha.DataContext = null;
        }

        // ===== Alta (CU-03) =====

        private void NuevaParcela_Click(object sender, RoutedEventArgs e)
        {
            Navegar(new FormularioParcelaVista());
        }
    }
}
