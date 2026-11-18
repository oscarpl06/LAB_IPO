using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG10 · CU-17 Consultar reservas (RF-19). Listado con búsqueda y filtros por estado
    /// y fechas; «Ver» abre la ficha de la reserva en el panel lateral.
    /// Editar y Cancelar llegarán con sus casos de uso.
    /// </summary>
    public partial class ReservasVista : VistaBase
    {
        private const string TodosLosEstados = "Todos los estados";

        private static readonly (string Texto, EstadoReserva? Estado)[] Estados =
        {
            (TodosLosEstados, null),
            ("Pendiente", EstadoReserva.Pendiente),
            ("Confirmada", EstadoReserva.Confirmada),
            ("En estancia", EstadoReserva.EnEstancia),
            ("Cancelada", EstadoReserva.Cancelada)
        };

        private readonly ListCollectionView _vista;

        public override string OpcionMenu => "Reservas";

        public ReservasVista() : this(null, null)
        {
        }

        /// <param name="mensajeGuardado">Aviso de confirmación sobre la tabla (reserva creada).</param>
        /// <param name="destacar">Reserva que se resalta y se lleva a la vista.</param>
        public ReservasVista(string mensajeGuardado, Reserva destacar)
        {
            InitializeComponent();

            _vista = new ListCollectionView(RepositorioReservas.Reservas);
            _vista.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(Reserva.Entrada),
                System.ComponentModel.ListSortDirection.Ascending));
            ListaReservas.ItemsSource = _vista;

            CmbFiltroEstado.ItemsSource = Estados.Select(e => e.Texto).ToList();
            CmbFiltroEstado.SelectedIndex = 0;

            FechaDesde.FechaCambiada += (s, e) => QuitarErrorFiltro();
            FechaHasta.FechaCambiada += (s, e) => QuitarErrorFiltro();

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

        /// <summary>Resalta la fila de la reserva y la lleva a la vista.</summary>
        private void Destacar(Reserva reserva)
        {
            if (!(ListaReservas.ItemContainerGenerator.ContainerFromItem(reserva) is ContentPresenter contenedor)) return;
            contenedor.ApplyTemplate();
            if (contenedor.ContentTemplate?.FindName("Fila", contenedor) is Border fila)
            {
                fila.Background = (Brush)FindResource("Pincel.Salvia100");
                fila.BringIntoView();
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
            CmbFiltroEstado.SelectedIndex = 0;
            FechaDesde.Limpiar();
            FechaHasta.Limpiar();
            QuitarErrorFiltro();
            AplicarFiltros();
        }

        private void QuitarErrorFiltro()
        {
            FechaDesde.TieneError = false;
            FechaHasta.TieneError = false;
            PanelErrorFiltro.Visibility = Visibility.Collapsed;
        }

        private void MostrarErrorFiltro(CampoFecha campo, string mensaje)
        {
            campo.TieneError = true;
            TxtErrorFiltro.Text = mensaje;
            PanelErrorFiltro.Visibility = Visibility.Visible;
            campo.Enfocar();
        }

        private void AplicarFiltros()
        {
            QuitarErrorFiltro();
            if (FechaDesde.TextoNoValido)
            {
                MostrarErrorFiltro(FechaDesde, "La fecha «Desde» no es válida (dd/mm/aaaa).");
                return;
            }
            if (FechaHasta.TextoNoValido)
            {
                MostrarErrorFiltro(FechaHasta, "La fecha «Hasta» no es válida (dd/mm/aaaa).");
                return;
            }
            if (FechaDesde.Fecha.HasValue && FechaHasta.Fecha.HasValue && FechaHasta.Fecha < FechaDesde.Fecha)
            {
                MostrarErrorFiltro(FechaHasta, "La fecha «Hasta» no puede ser anterior a la fecha «Desde».");
                return;
            }

            string texto = SinTildes(TxtBuscar.Text.Trim());
            string soloCifras = new string(texto.Where(char.IsDigit).ToArray());
            var estado = Estados[Math.Max(0, CmbFiltroEstado.SelectedIndex)].Estado;
            var desde = FechaDesde.Fecha ?? DateTime.MinValue;
            var hasta = FechaHasta.Fecha ?? DateTime.MaxValue.Date;

            _vista.Filter = o =>
            {
                var r = (Reserva)o;
                if (estado.HasValue && r.Estado != estado.Value) return false;

                // Estancias que tienen alguna noche dentro del intervalo.
                if ((FechaDesde.Fecha.HasValue || FechaHasta.Fecha.HasValue) && !r.CoincideCon(desde, hasta))
                    return false;

                if (texto.Length > 0)
                {
                    bool coincide =
                        SinTildes(r.Titular).IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                        || (r.Localizador ?? "").IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                        // El teléfono se compara sin espacios: "600111" encuentra "600 111 222".
                        || (soloCifras.Length >= 3 && new string((r.Telefonos ?? "").Where(char.IsDigit).ToArray())
                                .Contains(soloCifras));
                    if (!coincide) return false;
                }
                return true;
            };

            var visibles = _vista.Cast<Reserva>().ToList();
            int pendientes = visibles.Count(r => r.PendienteDeLlegada);
            bool sinFiltros = texto.Length == 0 && !estado.HasValue && FechaDesde.EstaVacio && FechaHasta.EstaVacio;

            string recuento = visibles.Count == 1
                ? (sinFiltros ? "1 reserva registrada" : "1 reserva encontrada")
                : (sinFiltros ? $"{visibles.Count} reservas registradas" : $"{visibles.Count} reservas encontradas");
            TxtRecuento.Text = visibles.Count == 0
                ? recuento
                : recuento + "  ·  " + (pendientes == 1 ? "1 pendiente de llegada" : $"{pendientes} pendientes de llegada");

            // Sin reservas: se distingue si no hay ninguna o si los filtros las ocultan.
            bool hayReservas = RepositorioReservas.Reservas.Count > 0;
            TxtSinResultados.Text = hayReservas
                ? "No hay reservas que coincidan con la búsqueda"
                : "Todavía no hay reservas registradas";
            TxtSinResultadosAyuda.Text = hayReservas
                ? "Prueba con otro nombre, estado o intervalo de fechas."
                : "Pulsa «Nueva reserva» para registrar la primera.";
            BtnQuitarFiltros.Visibility = hayReservas ? Visibility.Visible : Visibility.Collapsed;
            PanelSinResultados.Visibility = visibles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
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

        // ===== Ficha de la reserva =====

        private void Ver_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Reserva reserva)) return;

            if (!RepositorioReservas.Reservas.Contains(reserva))
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Reserva no disponible",
                    "La reserva seleccionada ya no existe. Actualiza el listado y vuelve a intentarlo.",
                    TipoDialogo.Aviso);
                return;
            }

            Navegador?.AbrirPanelLateral(new FichaReserva(reserva));
        }

        // ===== Modificar (CU-18) y cancelar (CU-19) =====

        private void Editar_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Reserva reserva)) return;
            if (!RepositorioReservas.Reservas.Contains(reserva))
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Reserva no disponible",
                    "La reserva seleccionada ya no existe. Actualiza el listado y vuelve a intentarlo.", TipoDialogo.Aviso);
                return;
            }
            Navegar(new FormularioReservaVista(reserva));
        }

        private void CancelarReserva_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Reserva r)) return;
            if (!RepositorioReservas.Reservas.Contains(r) || !r.PuedeCancelarse)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No se puede cancelar la reserva",
                    RepositorioReservas.Reservas.Contains(r) ? r.AyudaCancelar + "." : "La reserva ya no existe.",
                    TipoDialogo.Aviso);
                return;
            }

            bool confirmar = DialogoMensaje.Confirmar(VentanaPropietaria,
                $"¿Cancelar la reserva {r.Localizador}?",
                $"{r.NombreAlojamiento}, del {r.Entrada:dd/MM/yyyy} al {r.Salida:dd/MM/yyyy} ({r.NochesTexto}), "
                + $"{r.Ocupantes} {(r.Ocupantes == 1 ? "ocupante" : "ocupantes")}, a nombre de {r.Titular}.\n\n"
                + "El alojamiento volverá a quedar libre en esas fechas.",
                "Cancelar reserva", "Volver");
            if (!confirmar) return;

            try
            {
                RepositorioReservas.Cancelar(r);
            }
            catch (InvalidOperationException ex)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No se puede cancelar la reserva", ex.Message, TipoDialogo.Error);
                return;
            }

            Navegador?.CerrarPanelLateral();
            Navegar(new ReservasVista($"Reserva {r.Localizador} cancelada. {r.NombreAlojamiento} vuelve a estar libre en esas fechas.", r));
        }

        // ===== Alta (CU-16) =====

        private void NuevaReserva_Click(object sender, RoutedEventArgs e)
        {
            Navegar(new FormularioReservaVista());
        }
    }
}
