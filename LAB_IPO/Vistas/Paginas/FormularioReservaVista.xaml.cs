using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG09 · CU-16 Crear una reserva (RF-15 a RF-18). Valida los datos, comprueba que el
    /// alojamiento esté libre (y, en los bungalows, la capacidad y la estancia mínima),
    /// pide confirmación y registra la reserva.
    /// </summary>
    public partial class FormularioReservaVista : VistaBase
    {
        private static readonly Regex PatronCorreo = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");

        private static readonly string[] HorasLlegada =
            { "12:00 - 14:00", "14:00 - 16:00", "16:00 - 18:00", "18:00 - 20:00", "Después de las 20:00" };
        private static readonly string[] HorasSalida =
            { "Antes de 10:00", "Antes de 11:00", "Antes de 12:00" };

        private readonly bool _desdeDisponibilidad;

        /// <summary>Reserva que se está modificando (CU-18), o null en un alta (CU-16).</summary>
        private readonly Reserva _original;
        private bool EsEdicion => _original != null;
        private bool EnEstancia => EsEdicion && _original.Estado == EstadoReserva.EnEstancia;
        private bool _salidaConfirmada;
        private bool _cargando;

        public override string OpcionMenu => "Reservas";

        /// <summary>Formulario vacío (menú Reservas).</summary>
        public FormularioReservaVista()
        {
            _cargando = true;
            InitializeComponent();

            CmbTipo.ItemsSource = new[] { "Bungalow", "Parcela" };
            CmbHoraLlegada.ItemsSource = HorasLlegada;
            CmbHoraLlegada.SelectedItem = "16:00 - 18:00";
            CmbHoraSalida.ItemsSource = HorasSalida;
            CmbHoraSalida.SelectedItem = "Antes de 12:00";

            FechaEntrada.FechaCambiada += Fecha_Cambiada;
            FechaSalida.FechaCambiada += Fecha_Cambiada;

            CargarAlojamientos();
            ActualizarOcupantes();
            MarcarPendiente();
            _cargando = false;

            Loaded += (s, e) =>
            {
                if (_desdeDisponibilidad) TxtAdultos.Focus();
                else FechaEntrada.Enfocar();
            };
        }

        /// <summary>
        /// Desde Disponibilidad: el alojamiento y la entrada ya vienen elegidos; la salida
        /// se propone según la estancia mínima del bungalow (o una noche).
        /// </summary>
        public FormularioReservaVista(object alojamiento, DateTime entrada) : this()
        {
            _desdeDisponibilidad = true;
            _cargando = true;

            CmbTipo.SelectedItem = alojamiento is Bungalow ? "Bungalow" : "Parcela";
            CargarAlojamientos();
            CmbAlojamiento.SelectedItem = ((IEnumerable<OpcionAlojamiento>)CmbAlojamiento.ItemsSource)
                .FirstOrDefault(o => o.Alojamiento == alojamiento);

            int noches = Math.Max(1, (alojamiento as Bungalow)?.EstanciaMinima ?? 1);
            FechaEntrada.Fecha = entrada.Date;
            FechaSalida.Fecha = entrada.Date.AddDays(noches);

            MarcarPendiente();
            _cargando = false;
        }

        /// <summary>
        /// CU-18 · El mismo formulario con los datos actuales de la reserva. Si el cliente ya está
        /// alojado, la entrada y el alojamiento quedan bloqueados.
        /// </summary>
        public FormularioReservaVista(Reserva reserva) : this()
        {
            _original = reserva ?? throw new ArgumentNullException(nameof(reserva));
            _cargando = true;

            TxtTitulo.Text = $"Editar reserva {reserva.Localizador}";
            TxtSubtitulo.Text = EnEstancia
                ? "El cliente ya está alojado: puedes cambiar la salida, los ocupantes, el contacto y las solicitudes."
                : "Modifica las fechas, el alojamiento, los ocupantes o los datos de contacto y vuelve a comprobar la disponibilidad.";
            TxtGuardar.Text = "Guardar cambios";
            BtnGuardar.ToolTip = "Comprobar la disponibilidad y guardar los cambios de la reserva";
            BtnCancelarFormulario.ToolTip = "Volver al listado sin guardar los cambios";

            CmbTipo.SelectedItem = reserva.Alojamiento is Bungalow ? "Bungalow" : "Parcela";
            CargarAlojamientos();
            CmbAlojamiento.SelectedItem = ((IEnumerable<OpcionAlojamiento>)CmbAlojamiento.ItemsSource)
                .FirstOrDefault(o => o.Alojamiento == reserva.Alojamiento);
            FechaEntrada.Fecha = reserva.Entrada.Date;
            FechaSalida.Fecha = reserva.Salida.Date;
            TxtAdultos.Text = reserva.Adultos.ToString();
            TxtNinos.Text = reserva.Ninos > 0 ? reserva.Ninos.ToString() : "";
            if (reserva.HoraLlegada != null) CmbHoraLlegada.SelectedItem = reserva.HoraLlegada;
            if (reserva.HoraSalida != null) CmbHoraSalida.SelectedItem = reserva.HoraSalida;
            TxtTitular.Text = reserva.Titular;
            TxtTelefonos.Text = reserva.Telefonos;
            TxtCorreo.Text = reserva.Correo ?? "";
            TxtDocumento.Text = reserva.Documento ?? "";
            TxtSolicitudes.Text = reserva.Solicitudes ?? "";
            ActualizarOcupantes();

            if (EnEstancia)
            {
                const string bloqueado = "El cliente ya está alojado: no se puede cambiar";
                FechaEntrada.IsEnabled = CmbTipo.IsEnabled = CmbAlojamiento.IsEnabled = false;
                FechaEntrada.ToolTip = bloqueado + " la entrada";
                CmbTipo.ToolTip = CmbAlojamiento.ToolTip = bloqueado + " el alojamiento";
                ToolTipService.SetShowOnDisabled(FechaEntrada, true);
                ToolTipService.SetShowOnDisabled(CmbTipo, true);
                ToolTipService.SetShowOnDisabled(CmbAlojamiento, true);
            }

            MarcarPendiente();
            _cargando = false;
        }

        /// <summary>Elemento del desplegable de alojamientos (la plantilla muestra ToString()).</summary>
        public class OpcionAlojamiento
        {
            public object Alojamiento { get; set; }
            public string Texto { get; set; }
            public override string ToString() => Texto;
        }

        private object AlojamientoElegido => (CmbAlojamiento.SelectedItem as OpcionAlojamiento)?.Alojamiento;

        // ===== Tipo y alojamiento =====

        private void CmbTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized) return;
            CargarAlojamientos();
            Campo_Cambiado(sender, e);
        }

        private void CargarAlojamientos()
        {
            IEnumerable<OpcionAlojamiento> opciones;
            if ((string)CmbTipo.SelectedItem == "Bungalow")
                opciones = RepositorioAlojamientos.Bungalows.Select(b => new OpcionAlojamiento
                {
                    Alojamiento = b,
                    Texto = $"{b.Nombre}  ·  {b.CapacidadMaxima} pers.  ·  {b.PrecioTexto}/noche"
                });
            else if ((string)CmbTipo.SelectedItem == "Parcela")
                opciones = RepositorioAlojamientos.Parcelas.Select(p => new OpcionAlojamiento
                {
                    Alojamiento = p,
                    Texto = $"Parcela {p.Identificador}  ·  {p.Tipo?.Nombre}  ·  {p.TamanoTexto}"
                });
            else
                opciones = Enumerable.Empty<OpcionAlojamiento>();

            CmbAlojamiento.ItemsSource = opciones.ToList();
            CmbAlojamiento.SelectedItem = null;
            CmbAlojamiento.IsEnabled = CmbTipo.SelectedItem != null;
            CmbAlojamiento.ToolTip = CmbTipo.SelectedItem == null
                ? "Elige primero el tipo de alojamiento"
                : "Parcela o bungalow concreto que se reserva";
            ToolTipService.SetShowOnDisabled(CmbAlojamiento, true);
        }

        // ===== Ocupantes =====

        private void Ocupantes_Cambiado(object sender, TextChangedEventArgs e)
        {
            ActualizarOcupantes();
            Campo_Cambiado(sender, e);
        }

        private void ActualizarOcupantes()
        {
            bool hayAdultos = int.TryParse(TxtAdultos.Text, out int adultos);
            bool hayNinos = int.TryParse(TxtNinos.Text, out int ninos);
            TxtOcupantes.Text = hayAdultos || hayNinos ? (adultos + ninos).ToString() : "";
        }

        private int Adultos => int.TryParse(TxtAdultos.Text, out int n) ? n : 0;
        private int Ninos => int.TryParse(TxtNinos.Text, out int n) ? n : 0;

        // ===== Comprobación de disponibilidad =====

        private void VerDisponibilidad_Click(object sender, RoutedEventArgs e)
        {
            var errores = new List<string>();
            ValidarEstancia(errores);
            if (MostrarErrores(errores)) return;

            ComprobarDisponibilidad();
        }

        /// <summary>Comprueba la disponibilidad y la muestra en el aviso. Devuelve true si se puede reservar.</summary>
        private bool ComprobarDisponibilidad()
        {
            var entrada = FechaEntrada.Fecha.Value;
            var salida = FechaSalida.Fecha.Value;
            string motivo = RepositorioReservas.MotivoNoDisponible(AlojamientoElegido, entrada, salida, Adultos + Ninos,
                                                                  excluir: _original);

            if (motivo == null)
            {
                int noches = (int)(salida - entrada).TotalDays;
                MostrarAviso(
                    $"Disponible: {Alojamientos.Nombre(AlojamientoElegido)} está libre del {entrada:dd/MM/yyyy} "
                    + $"al {salida:dd/MM/yyyy} ({noches} {(noches == 1 ? "noche" : "noches")}) "
                    + $"para {Adultos + Ninos} {(Adultos + Ninos == 1 ? "ocupante" : "ocupantes")}.",
                    "Pincel.Salvia100", "Pincel.Salvia200", "Pincel.Bosque800", "Icono.Correcto");
                return true;
            }

            MostrarAviso("No disponible: " + motivo,
                "Pincel.ErrorFondo", "Pincel.Error", "Pincel.Error", "Icono.Error");
            MostrarAlternativas(entrada, salida);
            return false;
        }

        /// <summary>
        /// Ofrece los alojamientos del mismo tipo que sí están libres en esas fechas para esos
        /// ocupantes (capacidad y estancia mínima incluidas), o avisa de que no queda ninguno.
        /// </summary>
        private void MostrarAlternativas(DateTime entrada, DateTime salida)
        {
            int ocupantes = Adultos + Ninos;
            var alternativas = ((IEnumerable<OpcionAlojamiento>)CmbAlojamiento.ItemsSource)
                .Where(o => o.Alojamiento != AlojamientoElegido
                            && RepositorioReservas.MotivoNoDisponible(o.Alojamiento, entrada, salida, ocupantes,
                                                                      excluir: _original) == null)
                .ToList();

            bool bungalow = AlojamientoElegido is Bungalow;
            string fechas = $"del {entrada:dd/MM/yyyy} al {salida:dd/MM/yyyy}";
            string personas = $"{ocupantes} {(ocupantes == 1 ? "ocupante" : "ocupantes")}";

            if (alternativas.Count == 0)
            {
                TxtAlternativas.Text = bungalow
                    ? $"No hay ningún otro bungalow disponible {fechas} para {personas}."
                    : $"No hay ninguna otra parcela disponible {fechas} para {personas}.";
                ListaAlternativas.ItemsSource = null;
            }
            else
            {
                string tipo = bungalow
                    ? (alternativas.Count == 1 ? "Este bungalow sí está libre" : "Estos bungalows sí están libres")
                    : (alternativas.Count == 1 ? "Esta parcela sí está libre" : "Estas parcelas sí están libres");
                TxtAlternativas.Text = $"{tipo} {fechas} para {personas}:";
                ListaAlternativas.ItemsSource = alternativas;
            }
            PanelAlternativas.Visibility = Visibility.Visible;
        }

        // Elige la alternativa pulsada y comprueba de nuevo (debería salir disponible).
        private void Alternativa_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is OpcionAlojamiento opcion)) return;
            CmbAlojamiento.SelectedItem = opcion;
            ComprobarDisponibilidad();
            PanelError.Visibility = Visibility.Hidden;
            CmbAlojamiento.Focus();
        }

        private void MarcarPendiente()
        {
            MostrarAviso("Disponibilidad pendiente de comprobar. Los campos con * son obligatorios.",
                "Pincel.Beige100", "Pincel.Borde", "Pincel.TextoSecundario", "Icono.Info");
        }

        private void MostrarAviso(string texto, string fondo, string borde, string color, string icono)
        {
            PanelAlternativas.Visibility = Visibility.Collapsed;
            AvisoDisponibilidad.Background = (Brush)FindResource(fondo);
            AvisoDisponibilidad.BorderBrush = (Brush)FindResource(borde);
            TxtDisponibilidad.Text = texto;
            TxtDisponibilidad.Foreground = (Brush)FindResource(color);
            IconoDisponibilidad.Text = (string)FindResource(icono);
            IconoDisponibilidad.Foreground = (Brush)FindResource(color == "Pincel.TextoSecundario" ? "Pincel.Info" : color);
        }

        // ===== Validación =====

        /// <summary>Fechas, alojamiento y ocupantes: lo necesario para comprobar la disponibilidad.</summary>
        private void ValidarEstancia(List<string> errores)
        {
            if (FechaEntrada.EstaVacio)
                Marcar(FechaEntrada, errores, "Indica la fecha de entrada.");
            else if (FechaEntrada.TextoNoValido)
                Marcar(FechaEntrada, errores, "La fecha de entrada no es válida (dd/mm/aaaa).");
            else if (FechaEntrada.Fecha < DateTime.Today && !(EsEdicion && FechaEntrada.Fecha == _original.Entrada.Date))
                Marcar(FechaEntrada, errores, "La fecha de entrada no puede ser anterior a hoy.");

            if (FechaSalida.EstaVacio)
                Marcar(FechaSalida, errores, "Indica la fecha de salida.");
            else if (FechaSalida.TextoNoValido)
                Marcar(FechaSalida, errores, "La fecha de salida no es válida (dd/mm/aaaa).");
            else if (FechaEntrada.Fecha.HasValue && FechaSalida.Fecha <= FechaEntrada.Fecha)
                Marcar(FechaSalida, errores, "La fecha de salida debe ser posterior a la de entrada.");
            else if (EnEstancia && FechaSalida.Fecha <= DateTime.Today)
                Marcar(FechaSalida, errores, "La salida debe ser posterior a hoy: el cliente sigue alojado.");

            if (CmbTipo.SelectedItem == null)
                Marcar(CmbTipo, errores, "Selecciona el tipo de alojamiento.");
            else if (CmbAlojamiento.SelectedItem == null)
                Marcar(CmbAlojamiento, errores, "Selecciona el alojamiento que se reserva.");

            if (TxtAdultos.Text.Trim().Length == 0)
                Marcar(TxtAdultos, errores, "Indica el número de adultos.");
            else if (Adultos < 1)
                Marcar(TxtAdultos, errores, "La reserva debe incluir al menos un adulto.");
        }

        private void ValidarTitular(List<string> errores)
        {
            string titular = Normalizar(TxtTitular.Text);
            if (titular.Length == 0)
                Marcar(TxtTitular, errores, "Indica el nombre de la persona titular.");
            else if (titular.Length < 3 || titular.Any(char.IsDigit))
                Marcar(TxtTitular, errores, "Revisa el nombre de la persona titular: escribe su nombre y apellidos.");

            var telefonos = Partes(TxtTelefonos.Text);
            if (telefonos.Count == 0)
                Marcar(TxtTelefonos, errores, "Indica al menos un teléfono de contacto.");
            else if (telefonos.Any(t => !TelefonoValido(t)))
                Marcar(TxtTelefonos, errores, "Revisa el teléfono: debe tener 9 cifras (puede llevar el prefijo +34).");

            string correo = TxtCorreo.Text.Trim();
            if (correo.Length > 0 && !PatronCorreo.IsMatch(correo))
                Marcar(TxtCorreo, errores, "Revisa el e-mail: debe tener el formato nombre@dominio.es.");
        }

        private bool MostrarErrores(List<string> errores)
        {
            if (errores.Count == 0) return false;
            TxtError.Text = string.Join("\n", errores);
            PanelError.Visibility = Visibility.Visible;
            PrimerCampoConError()?.Focus();
            return true;
        }

        private static void Marcar(Control campo, List<string> errores, string mensaje)
        {
            if (campo is CampoFecha fecha) fecha.TieneError = true;
            else CampoAyudante.SetTieneError(campo, true);
            errores.Add(mensaje);
        }

        private static bool TieneError(Control campo) =>
            campo is CampoFecha fecha ? fecha.TieneError : CampoAyudante.GetTieneError(campo);

        private Control PrimerCampoConError() =>
            new Control[] { FechaEntrada, FechaSalida, CmbTipo, CmbAlojamiento, TxtAdultos, TxtTitular, TxtTelefonos, TxtCorreo }
                .FirstOrDefault(TieneError);

        private void Fecha_Cambiada(object sender, EventArgs e) => Campo_Cambiado(sender, null);

        // Al corregir un campo se retira su marca; si cambia la estancia, hay que volver a comprobarla.
        private void Campo_Cambiado(object sender, RoutedEventArgs e)
        {
            if (sender is CampoFecha fecha) fecha.TieneError = false;
            else if (sender is Control campo && CampoAyudante.GetTieneError(campo))
                CampoAyudante.SetTieneError(campo, false);

            if (sender == CmbTipo) CampoAyudante.SetTieneError(CmbAlojamiento, false);

            if (PrimerCampoConError() == null)
                PanelError.Visibility = Visibility.Hidden;

            if (!_cargando && (sender is CampoFecha || sender == CmbTipo || sender == CmbAlojamiento
                               || sender == TxtAdultos || sender == TxtNinos))
                MarcarPendiente();
        }

        private void SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private static string Normalizar(string texto) => Regex.Replace((texto ?? "").Trim(), @"\s+", " ");

        private static List<string> Partes(string texto) =>
            (texto ?? "").Split(',').Select(Normalizar).Where(p => p.Length > 0).ToList();

        /// <summary>Teléfono español: 9 cifras, admite espacios, guiones y el prefijo +34.</summary>
        private static bool TelefonoValido(string telefono)
        {
            string cifras = Regex.Replace(telefono, @"[\s\-]", "");
            if (cifras.StartsWith("+34")) cifras = cifras.Substring(3);
            return Regex.IsMatch(cifras, @"^[6789]\d{8}$");
        }

        // ===== Guardar =====

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            var errores = new List<string>();
            ValidarEstancia(errores);
            ValidarTitular(errores);
            if (MostrarErrores(errores)) return;

            if (!ComprobarDisponibilidad())
            {
                TxtError.Text = (EsEdicion ? "No se pueden guardar los cambios: " : "No se puede crear la reserva: ")
                                + TxtDisponibilidad.Text.Replace("No disponible: ", "");
                PanelError.Visibility = Visibility.Visible;
                return;
            }

            if (EsEdicion)
            {
                GuardarCambios();
                return;
            }

            var alojamiento = AlojamientoElegido;
            var entrada = FechaEntrada.Fecha.Value;
            var salida = FechaSalida.Fecha.Value;
            int noches = (int)(salida - entrada).TotalDays;
            string titular = Normalizar(TxtTitular.Text);

            bool confirmar = DialogoMensaje.Confirmar(VentanaPropietaria,
                "¿Guardar la reserva?",
                $"{Alojamientos.Nombre(alojamiento)}, del {entrada:dd/MM/yyyy} al {salida:dd/MM/yyyy} "
                + $"({noches} {(noches == 1 ? "noche" : "noches")}), para {Adultos + Ninos} "
                + $"{(Adultos + Ninos == 1 ? "ocupante" : "ocupantes")}, a nombre de {titular}.",
                "Guardar reserva", "Seguir editando");
            if (!confirmar) return;

            var reserva = new Reserva
            {
                Localizador = RepositorioReservas.SiguienteLocalizador(),
                Titular = titular,
                Alojamiento = alojamiento,
                Entrada = entrada,
                Salida = salida,
                HoraLlegada = (string)CmbHoraLlegada.SelectedItem,
                HoraSalida = (string)CmbHoraSalida.SelectedItem,
                Telefonos = string.Join(", ", Partes(TxtTelefonos.Text)),
                Correo = TxtCorreo.Text.Trim(),
                Documento = TxtDocumento.Text.Trim(),
                Adultos = Adultos,
                Ninos = Ninos,
                Solicitudes = TxtSolicitudes.Text.Trim()
            };
            RepositorioReservas.Agregar(reserva);

            _salidaConfirmada = true;
            Navegar(new ReservasVista(
                $"Reserva {reserva.Localizador} creada para {reserva.NombreAlojamiento}, "
                + $"del {entrada:dd/MM/yyyy} al {salida:dd/MM/yyyy}.", reserva));
        }

        /// <summary>Datos del formulario como reserva (sin localizador ni estado).</summary>
        private Reserva LeerFormulario() => new Reserva
        {
            Titular = Normalizar(TxtTitular.Text),
            Alojamiento = AlojamientoElegido,
            Entrada = FechaEntrada.Fecha.Value,
            Salida = FechaSalida.Fecha.Value,
            HoraLlegada = (string)CmbHoraLlegada.SelectedItem,
            HoraSalida = (string)CmbHoraSalida.SelectedItem,
            Telefonos = string.Join(", ", Partes(TxtTelefonos.Text)),
            Correo = TxtCorreo.Text.Trim(),
            Documento = TxtDocumento.Text.Trim(),
            Adultos = Adultos,
            Ninos = Ninos,
            Solicitudes = TxtSolicitudes.Text.Trim()
        };

        private bool HayCambiosEnEdicion
        {
            get
            {
                if (!FechaEntrada.Fecha.HasValue || !FechaSalida.Fecha.HasValue) return true;
                var c = LeerFormulario();
                var o = _original;
                return c.Alojamiento != o.Alojamiento || c.Entrada != o.Entrada.Date || c.Salida != o.Salida.Date
                       || c.HoraLlegada != o.HoraLlegada || c.HoraSalida != o.HoraSalida
                       || c.Titular != o.Titular || c.Telefonos != string.Join(", ", Partes(o.Telefonos))
                       || c.Correo != (o.Correo ?? "") || c.Documento != (o.Documento ?? "")
                       || c.Adultos != o.Adultos || c.Ninos != o.Ninos || c.Solicitudes != (o.Solicitudes ?? "");
            }
        }

        /// <summary>CU-18 · Pide confirmación y aplica los cambios sobre la reserva existente (RF-36).</summary>
        private void GuardarCambios()
        {
            if (!HayCambiosEnEdicion)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No hay cambios que guardar",
                    $"No has modificado ningún dato de la reserva {_original.Localizador}.");
                return;
            }

            var cambios = LeerFormulario();
            int noches = (int)(cambios.Salida - cambios.Entrada).TotalDays;
            if (!DialogoMensaje.Confirmar(VentanaPropietaria,
                    $"¿Guardar los cambios de la reserva {_original.Localizador}?",
                    $"{Alojamientos.Nombre(cambios.Alojamiento)}, del {cambios.Entrada:dd/MM/yyyy} al "
                    + $"{cambios.Salida:dd/MM/yyyy} ({noches} {(noches == 1 ? "noche" : "noches")}), para "
                    + $"{cambios.Ocupantes} {(cambios.Ocupantes == 1 ? "ocupante" : "ocupantes")}, a nombre de {cambios.Titular}.",
                    "Guardar cambios", "Seguir editando"))
                return;

            try
            {
                RepositorioReservas.Modificar(_original, cambios);
            }
            catch (InvalidOperationException ex)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No se han podido guardar los cambios",
                    ex.Message + " La reserva se conserva como estaba.", TipoDialogo.Error);
                return;
            }

            _salidaConfirmada = true;
            Navegar(new ReservasVista($"Cambios de la reserva {_original.Localizador} guardados correctamente.", _original));
        }

        // ===== Cancelar y salida =====

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            if (!PuedeSalir()) return;
            _salidaConfirmada = true;
            Navegar(new ReservasVista());
        }

        private bool HayDatos =>
            (!_desdeDisponibilidad && (!FechaEntrada.EstaVacio || !FechaSalida.EstaVacio
                                       || CmbTipo.SelectedItem != null))
            || new[] { TxtAdultos, TxtNinos, TxtTitular, TxtTelefonos, TxtCorreo, TxtDocumento, TxtSolicitudes }
                .Any(t => t.Text.Trim().Length > 0);

        /// <summary>Si hay datos sin guardar, pide confirmación antes de abandonar el formulario (RF-36).</summary>
        public override bool PuedeSalir()
        {
            if (_salidaConfirmada || !(EsEdicion ? HayCambiosEnEdicion : HayDatos)) return true;

            _salidaConfirmada = EsEdicion
                ? DialogoMensaje.Confirmar(VentanaPropietaria,
                    $"¿Descartar los cambios de la reserva {_original.Localizador}?",
                    "La reserva se quedará como estaba.",
                    "Descartar", "Seguir editando")
                : DialogoMensaje.Confirmar(VentanaPropietaria,
                    "¿Descartar la nueva reserva?",
                    "Se perderán los datos que has introducido en el formulario.",
                    "Descartar", "Seguir editando");
            return _salidaConfirmada;
        }
    }
}
