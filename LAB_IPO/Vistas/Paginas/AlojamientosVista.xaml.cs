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
        /// <param name="mostrarBungalows">Abre directamente la pestaña de bungalows.</param>
        /// <param name="abrirFichaBungalow">Al volver de la galería (CU-13), reabre la ficha de destacarBungalow.</param>
        public AlojamientosVista(string mensajeGuardado, bool mostrarBungalows = false, Parcela destacar = null,
                                 Bungalow destacarBungalow = null, bool abrirFichaBungalow = false)
        {
            InitializeComponent();
            _vistaBungalows = new ListCollectionView(RepositorioAlojamientos.Bungalows);
            ListaBungalows.ItemsSource = _vistaBungalows;
            CmbFiltroCapacidad.ItemsSource = new[]
            {
                new OpcionFiltro { Texto = "Cualquier capacidad" },
                new OpcionFiltro { Texto = "Al menos 2 personas", Valor = 2 },
                new OpcionFiltro { Texto = "Al menos 3 personas", Valor = 3 },
                new OpcionFiltro { Texto = "Al menos 4 personas", Valor = 4 },
                new OpcionFiltro { Texto = "Al menos 6 personas", Valor = 6 }
            };
            CmbFiltroEstadoBungalow.ItemsSource = new[]
            {
                new OpcionFiltro { Texto = "Todos los estados" },
                new OpcionFiltro { Texto = "Libre", Valor = EstadoAlojamiento.Libre },
                new OpcionFiltro { Texto = "Ocupado", Valor = EstadoAlojamiento.Ocupado },
                new OpcionFiltro { Texto = "En reparación", Valor = EstadoAlojamiento.EnReparacion },
                new OpcionFiltro { Texto = "Pendiente de limpieza", Valor = EstadoAlojamiento.PendienteLimpieza }
            };
            CmbFiltroCapacidad.SelectedIndex = 0;
            CmbFiltroEstadoBungalow.SelectedIndex = 0;

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

            if (mostrarBungalows)
                TabBungalows.IsChecked = true;

            if (abrirFichaBungalow && destacarBungalow != null)
                Loaded += (s, e) =>
                {
                    if (ListaBungalows.ItemContainerGenerator.ContainerFromItem(destacarBungalow) is FrameworkElement tarjeta)
                        tarjeta.BringIntoView();
                    if (RepositorioAlojamientos.Bungalows.Contains(destacarBungalow))
                        Navegador?.AbrirPanelLateral(new FichaBungalow(destacarBungalow));
                };

            if (!string.IsNullOrEmpty(mensajeGuardado))
            {
                TxtAvisoGuardado.Text = mensajeGuardado;
                AvisoGuardado.Visibility = Visibility.Visible;
                // El alojamiento nuevo se añade al final: se baja hasta él para que se vea.
                Loaded += (s, e) =>
                {
                    if (mostrarBungalows)
                    {
                        // Tras editar se baja hasta su tarjeta; tras un alta, hasta el final.
                        if (destacarBungalow != null
                            && ListaBungalows.ItemContainerGenerator.ContainerFromItem(destacarBungalow) is FrameworkElement tarjeta)
                            tarjeta.BringIntoView();
                        else
                            PanelBungalows.ScrollToEnd();
                        return;
                    }
                    var ultima = destacar ?? RepositorioAlojamientos.Parcelas.LastOrDefault();
                    if (ultima != null) ListaParcelas.ScrollIntoView(ultima);
                };
            }
        }

        // ===== Pestañas Parcelas / Bungalows =====

        private bool MostrandoBungalows => TabBungalows?.IsChecked == true;

        private void Pestana_Checked(object sender, RoutedEventArgs e)
        {
            // Se dispara también mientras se construye la vista: espera a que exista todo.
            if (PanelBungalows == null || PanelParcelas == null || TxtAvisoEliminar == null) return;

            bool bungalows = MostrandoBungalows;
            PanelBungalows.Visibility = bungalows ? Visibility.Visible : Visibility.Collapsed;
            PanelParcelas.Visibility = bungalows ? Visibility.Collapsed : Visibility.Visible;

            // Los filtros y las tarifas son de parcelas; la consulta de bungalows llegará con el CU-10.
            GrupoFiltros.Visibility = bungalows ? Visibility.Collapsed : Visibility.Visible;
            GrupoFiltrosBungalows.Visibility = bungalows ? Visibility.Visible : Visibility.Collapsed;
            BtnTiposTarifas.Visibility = bungalows ? Visibility.Collapsed : Visibility.Visible;

            TxtNuevo.Text = bungalows ? "Nuevo bungalow" : "Nueva parcela";
            BtnNuevaParcela.ToolTip = bungalows
                ? "Registrar un bungalow nuevo en el complejo"
                : "Registrar una parcela nueva en el camping";
            TxtAvisoEliminar.Text = bungalows
                ? "Al eliminar un bungalow se solicitará confirmación antes de borrar sus datos."
                : "Al eliminar una parcela se solicitará confirmación antes de borrar sus datos.";

            AvisoGuardado.Visibility = Visibility.Collapsed;
            CerrarFicha();
            ActualizarRecuento();
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
            if (MostrandoBungalows)
            {
                int visiblesB = _vistaBungalows.Count;
                int totalB = RepositorioAlojamientos.Bungalows.Count;
                TxtRecuento.Text = visiblesB == 1 ? "1 bungalow encontrado" : $"{visiblesB} bungalows encontrados";
                TxtPaginacion.Text = $"Mostrando {visiblesB} de {totalB} bungalows";
                PanelSinBungalows.Visibility = visiblesB == 0 ? Visibility.Visible : Visibility.Collapsed;
                return;
            }
            PanelSinBungalows.Visibility = Visibility.Collapsed;

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
            // La ficha se abre en el panel lateral de la ventana; al cerrarse se quita el resaltado.
            Navegador?.AbrirPanelLateral(new FichaParcela(parcela), alCerrar: QuitarResaltado);
        }

        // Pulsar una fila o moverse con las flechas no abre la ficha:
        // la fila resaltada es siempre la de la ficha abierta (o ninguna).
        private void ListaParcelas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(ListaParcelas.SelectedItem, _parcelaEnFicha))
                ListaParcelas.SelectedItem = _parcelaEnFicha;
        }

        private void QuitarResaltado()
        {
            _parcelaEnFicha = null;
            ListaParcelas.SelectedItem = null;
        }

        private void CerrarFicha()
        {
            Navegador?.CerrarPanelLateral();
            QuitarResaltado();
        }

        // ===== Bungalows: filtros y ficha (CU-10) =====

        private readonly ListCollectionView _vistaBungalows;

        private void FiltrarBungalows_Click(object sender, RoutedEventArgs e) => AplicarFiltrosBungalows();

        private void BuscarBungalow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AplicarFiltrosBungalows();
                e.Handled = true;
            }
        }

        private void LimpiarBungalows_Click(object sender, RoutedEventArgs e)
        {
            TxtBuscarBungalow.Clear();
            CmbFiltroCapacidad.SelectedIndex = 0;
            CmbFiltroEstadoBungalow.SelectedIndex = 0;
            AplicarFiltrosBungalows();
        }

        private void AplicarFiltrosBungalows()
        {
            string texto = SinTildes(TxtBuscarBungalow.Text.Trim());
            var capacidad = (CmbFiltroCapacidad.SelectedItem as OpcionFiltro)?.Valor as int?;
            var estado = (CmbFiltroEstadoBungalow.SelectedItem as OpcionFiltro)?.Valor as EstadoAlojamiento?;

            bool sinFiltros = texto.Length == 0 && capacidad == null && estado == null;
            _vistaBungalows.Filter = sinFiltros ? null : new Predicate<object>(o =>
            {
                var b = (Bungalow)o;
                if (capacidad != null && b.CapacidadMaxima < capacidad) return false;
                if (estado != null && b.Estado != estado) return false;
                if (texto.Length > 0
                    && SinTildes(b.Nombre).IndexOf(texto, StringComparison.OrdinalIgnoreCase) < 0
                    && SinTildes(b.Descripcion).IndexOf(texto, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
                return true;
            });

            ActualizarRecuento();
        }

        private void VerBungalow_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Bungalow bungalow)) return;

            if (!RepositorioAlojamientos.Bungalows.Contains(bungalow))
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Bungalow no disponible",
                    "El bungalow seleccionado ya no existe. Actualiza el listado y vuelve a intentarlo.",
                    TipoDialogo.Aviso);
                return;
            }

            Navegador?.AbrirPanelLateral(new FichaBungalow(bungalow));
        }

        // ===== Disponibilidad (CU-14) =====

        private void Disponibilidad_Click(object sender, RoutedEventArgs e) => Navegar(new DisponibilidadVista());

        // ===== Modificar bungalow (CU-09) =====

        private void EditarBungalow_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Bungalow bungalow)) return;

            if (!RepositorioAlojamientos.Bungalows.Contains(bungalow))
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Bungalow no disponible",
                    "El bungalow seleccionado ya no existe. Actualiza el listado y vuelve a intentarlo.",
                    TipoDialogo.Aviso);
                return;
            }

            Navegar(new FormularioBungalowVista(bungalow));
        }

        // ===== Modificar parcela (CU-05) =====

        private void EditarParcela_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Parcela parcela)) return;

            if (!RepositorioAlojamientos.Parcelas.Contains(parcela))
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Parcela no disponible",
                    "La parcela seleccionada ya no existe. Actualiza el listado y vuelve a intentarlo.",
                    TipoDialogo.Aviso);
                return;
            }

            Navegar(new FormularioParcelaVista(parcela));
        }

        // ===== Alta (CU-03) =====

        // Abre el alta de parcela (CU-03) o de bungaló (CU-07) según la pestaña.
        // ===== Dar de baja (CU-04 parcela, CU-08 bungalow) =====

        private void EliminarParcela_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Parcela parcela)) return;

            string resumen = $"{parcela.Tipo?.Nombre} · {parcela.TamanoTexto} · {parcela.Ubicacion}\n"
                           + $"Servicios: {parcela.ServiciosTexto}\nEstado: {parcela.EstadoTexto}";
            DarDeBaja(parcela, $"Parcela {parcela.Identificador}", "la parcela", resumen,
                () => RepositorioAlojamientos.DarDeBajaParcela(parcela),
                RepositorioAlojamientos.Parcelas.Contains(parcela));
        }

        private void EliminarBungalow_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Bungalow bungalow)) return;

            string resumen = $"{bungalow.TamanoTexto} · {bungalow.CapacidadTexto} · {bungalow.PrecioTexto}/noche\n"
                           + $"Fotos: {bungalow.FotosTexto}\nEstado: {bungalow.EstadoTexto}";
            DarDeBaja(bungalow, bungalow.Nombre, "el bungalow", resumen,
                () => RepositorioAlojamientos.DarDeBajaBungalow(bungalow),
                RepositorioAlojamientos.Bungalows.Contains(bungalow));
        }

        /// <summary>
        /// Flujo común de baja: comprueba que exista y que se pueda dar de baja, muestra sus datos
        /// para confirmar (RF-36) y, si se confirma, lo da de baja e informa del resultado.
        /// </summary>
        private void DarDeBaja(object alojamiento, string nombre, string articulo, string resumen,
                               Action darDeBaja, bool existe)
        {
            if (!existe)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Alojamiento no disponible",
                    $"{nombre} ya no existe. Actualiza el listado y vuelve a intentarlo.", TipoDialogo.Aviso);
                return;
            }

            string motivo = RepositorioReservas.MotivoNoBaja(alojamiento);
            if (motivo != null)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, $"No se puede dar de baja {articulo}",
                    motivo + $"\n\nCancela o espera a que terminen sus reservas antes de dar{(alojamiento is Parcela ? "la" : "lo")} de baja. "
                           + "Sus datos se conservan.",
                    TipoDialogo.Aviso);
                return;
            }

            bool confirmar = DialogoMensaje.Confirmar(VentanaPropietaria,
                $"¿Dar de baja {articulo} {QuitarPrefijo(nombre)}?",
                resumen + $"\n\nDejará de aparecer en la gestión ordinaria del camping.",
                "Dar de baja", "Cancelar");
            if (!confirmar) return;

            try
            {
                darDeBaja();
            }
            catch (InvalidOperationException ex)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, $"No se ha podido dar de baja {articulo}",
                    ex.Message + " Sus datos se conservan.", TipoDialogo.Error);
                return;
            }

            CerrarFicha();
            AplicarFiltros();
            AplicarFiltrosBungalows();
            TxtAvisoGuardado.Text = $"{nombre} dado de baja correctamente.";
            if (alojamiento is Parcela) TxtAvisoGuardado.Text = $"{nombre} dada de baja correctamente.";
            AvisoGuardado.Visibility = Visibility.Visible;
        }

        // «Parcela P-03» -> «P-03» y «Bungalow 04» -> «04», para no repetir el tipo en la pregunta.
        private static string QuitarPrefijo(string nombre) =>
            nombre.StartsWith("Parcela ") ? nombre.Substring("Parcela ".Length) : nombre.Replace("Bungalow ", "");

        private void NuevaParcela_Click(object sender, RoutedEventArgs e)
        {
            if (MostrandoBungalows)
                Navegar(new FormularioBungalowVista());
            else
                Navegar(new FormularioParcelaVista());
        }
    }
}
