using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG12 · CU-20 Dar de alta un empleado temporal (RF-22). Valida los datos, marca en rojo
    /// los campos con problemas y, si todo es correcto, registra al empleado y vuelve al listado.
    /// </summary>
    public partial class FormularioEmpleadoVista : VistaBase
    {
        private static readonly Regex PatronCorreo = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");

        private string _rutaFoto;
        private bool _salidaConfirmada;

        /// <summary>Empleado que se está modificando (CU-23), o null en un alta (CU-20).</summary>
        private readonly Empleado _original;
        private bool EsEdicion => _original != null;

        public override string OpcionMenu => "Personal";

        public FormularioEmpleadoVista()
        {
            InitializeComponent();
            CmbDisponibilidad.ItemsSource = RepositorioPersonal.OpcionesDisponibilidad;
            CmbDisponibilidad.SelectedIndex = 0;
            Loaded += (s, e) => TxtNombre.Focus();
        }

        /// <summary>CU-23 · El mismo formulario con los datos actuales del empleado, foto incluida.</summary>
        public FormularioEmpleadoVista(Empleado empleado) : this()
        {
            _original = empleado ?? throw new ArgumentNullException(nameof(empleado));

            TxtTitulo.Text = $"Editar a {empleado.NombreCompleto}";
            TxtSubtitulo.Text = "Modifica los datos personales, las competencias, la disponibilidad o la foto del empleado.";
            TxtGuardar.Text = "Guardar cambios";
            BtnGuardar.ToolTip = "Guardar los cambios del empleado";
            BtnCancelarFormulario.ToolTip = "Volver al listado sin guardar los cambios";

            TxtNombre.Text = empleado.Nombre;
            TxtApellidos.Text = empleado.Apellidos;
            TxtTelefonos.Text = empleado.Telefonos;
            TxtCorreos.Text = empleado.Correos;
            TxtIdiomas.Text = empleado.Idiomas ?? "";
            TxtFormacion.Text = empleado.Formacion ?? "";
            CmbDisponibilidad.SelectedItem = empleado.Disponibilidad;
            TxtDiasNoDisponibles.Text = empleado.DiasNoDisponibles ?? "";
            TxtRestricciones.Text = empleado.Restricciones ?? "";

            _rutaFoto = empleado.RutaFoto;
            VistaPreviaFoto.Nombre = empleado.NombreCompleto;
            VistaPreviaFoto.Foto = RutaImagenConverter.Cargar(_rutaFoto, 300);
            BtnQuitarFoto.IsEnabled = _rutaFoto != null;
        }

        // ===== Guardar =====

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            var errores = new List<string>();

            string nombre = Normalizar(TxtNombre.Text);
            string apellidos = Normalizar(TxtApellidos.Text);
            if (nombre.Length == 0) Marcar(TxtNombre, errores, "Indica el nombre del empleado.");
            if (apellidos.Length == 0) Marcar(TxtApellidos, errores, "Indica los apellidos del empleado.");

            var telefonos = Partes(TxtTelefonos.Text);
            if (telefonos.Count == 0)
                Marcar(TxtTelefonos, errores, "Indica al menos un teléfono de contacto.");
            else if (telefonos.Any(t => !TelefonoValido(t)))
                Marcar(TxtTelefonos, errores, "Revisa el teléfono: debe tener 9 cifras (puede llevar el prefijo +34).");

            var correos = Partes(TxtCorreos.Text);
            if (correos.Count == 0)
                Marcar(TxtCorreos, errores, "Indica al menos un correo electrónico.");
            else if (correos.Any(c => !PatronCorreo.IsMatch(c)))
                Marcar(TxtCorreos, errores, "Revisa el correo: debe tener el formato nombre@dominio.es.");
            else if (correos.FirstOrDefault(c => RepositorioPersonal.ExisteCorreo(c, _original)) is string repetido)
                Marcar(TxtCorreos, errores, $"Ya hay un empleado con el correo {repetido}.");

            if (errores.Count > 0)
            {
                TxtError.Text = string.Join("\n", errores);
                MostrarError(true);
                PrimerCampoConError()?.Focus();
                return;
            }

            if (EsEdicion)
            {
                GuardarCambios(nombre, apellidos, telefonos, correos);
                return;
            }

            var empleado = new Empleado
            {
                Nombre = nombre,
                Apellidos = apellidos,
                Telefonos = string.Join(", ", telefonos),
                Correos = string.Join(", ", correos),
                Idiomas = Normalizar(TxtIdiomas.Text),
                Formacion = TxtFormacion.Text.Trim(),
                Disponibilidad = (string)CmbDisponibilidad.SelectedItem ?? "Completa",
                DiasNoDisponibles = Normalizar(TxtDiasNoDisponibles.Text),
                Restricciones = TxtRestricciones.Text.Trim(),
                RutaFoto = _rutaFoto
            };
            RepositorioPersonal.AgregarEmpleado(empleado);

            _salidaConfirmada = true;
            Navegar(new PersonalVista($"{empleado.NombreCompleto} dado de alta correctamente."));
        }

        /// <summary>CU-23 · Pide confirmación y aplica los cambios sobre el empleado existente (RF-36).</summary>
        private void GuardarCambios(string nombre, string apellidos, List<string> telefonos, List<string> correos)
        {
            if (!HayCambiosEnEdicion)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No hay cambios que guardar",
                    $"No has modificado ningún dato de {_original.NombreCompleto}.");
                return;
            }

            if (!DialogoMensaje.Confirmar(VentanaPropietaria,
                    $"¿Guardar los cambios de {_original.NombreCompleto}?",
                    "Se actualizará su ficha con los datos del formulario.",
                    "Guardar", "Seguir editando"))
                return;

            if (!RepositorioPersonal.Empleados.Contains(_original))
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No se han podido guardar los cambios",
                    "El empleado ya no existe en el registro de personal.", TipoDialogo.Error);
                return;
            }

            _original.Nombre = nombre;
            _original.Apellidos = apellidos;
            _original.Telefonos = string.Join(", ", telefonos);
            _original.Correos = string.Join(", ", correos);
            _original.Idiomas = Normalizar(TxtIdiomas.Text);
            _original.Formacion = TxtFormacion.Text.Trim();
            _original.Disponibilidad = (string)CmbDisponibilidad.SelectedItem ?? "Completa";
            _original.DiasNoDisponibles = Normalizar(TxtDiasNoDisponibles.Text);
            _original.Restricciones = TxtRestricciones.Text.Trim();
            _original.RutaFoto = _rutaFoto;

            _salidaConfirmada = true;
            Navegar(new PersonalVista($"Cambios de {_original.NombreCompleto} guardados correctamente.", _original));
        }

        private bool HayCambiosEnEdicion =>
            Normalizar(TxtNombre.Text) != (_original.Nombre ?? "")
            || Normalizar(TxtApellidos.Text) != (_original.Apellidos ?? "")
            || string.Join(", ", Partes(TxtTelefonos.Text)) != string.Join(", ", Partes(_original.Telefonos))
            || string.Join(", ", Partes(TxtCorreos.Text)) != string.Join(", ", Partes(_original.Correos))
            || Normalizar(TxtIdiomas.Text) != Normalizar(_original.Idiomas)
            || TxtFormacion.Text.Trim() != (_original.Formacion ?? "").Trim()
            || (string)CmbDisponibilidad.SelectedItem != _original.Disponibilidad
            || Normalizar(TxtDiasNoDisponibles.Text) != Normalizar(_original.DiasNoDisponibles)
            || TxtRestricciones.Text.Trim() != (_original.Restricciones ?? "").Trim()
            || _rutaFoto != _original.RutaFoto;

        private static string Normalizar(string texto) =>
            string.Join(" ", (texto ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

        private static List<string> Partes(string texto) =>
            (texto ?? "").Split(',').Select(Normalizar).Where(p => p.Length > 0).ToList();

        /// <summary>Teléfono español: 9 cifras, admite espacios, guiones y el prefijo +34.</summary>
        private static bool TelefonoValido(string telefono)
        {
            string cifras = Regex.Replace(telefono, @"[\s\-]", "");
            if (cifras.StartsWith("+34")) cifras = cifras.Substring(3);
            return Regex.IsMatch(cifras, @"^[6789]\d{8}$");
        }

        private static void Marcar(Control campo, List<string> errores, string mensaje)
        {
            CampoAyudante.SetTieneError(campo, true);
            errores.Add(mensaje);
        }

        private Control PrimerCampoConError() =>
            new Control[] { TxtNombre, TxtApellidos, TxtTelefonos, TxtCorreos }
                .FirstOrDefault(CampoAyudante.GetTieneError);

        /// <summary>El aviso de error ocupa el sitio de la nota de campos obligatorios.</summary>
        private void MostrarError(bool mostrar)
        {
            PanelError.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
            TxtNotaObligatorios.Visibility = mostrar ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Campo_Cambiado(object sender, RoutedEventArgs e)
        {
            if (sender is Control campo && CampoAyudante.GetTieneError(campo))
                CampoAyudante.SetTieneError(campo, false);

            if (PanelError != null && PrimerCampoConError() == null)
                MostrarError(false);
        }

        // El nombre alimenta las iniciales de la vista previa cuando no hay foto.
        private void Nombre_Cambiado(object sender, RoutedEventArgs e)
        {
            Campo_Cambiado(sender, e);
            if (VistaPreviaFoto != null)
                VistaPreviaFoto.Nombre = $"{TxtNombre.Text.Trim()} {TxtApellidos.Text.Trim()}".Trim();
        }

        // ===== Foto =====

        private void SeleccionarFoto_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new OpenFileDialog
            {
                Title = "Seleccionar foto del empleado",
                Filter = "Imágenes (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png"
            };
            if (dialogo.ShowDialog(VentanaPropietaria) != true) return;

            var imagen = RutaImagenConverter.Cargar(dialogo.FileName, 300);
            if (imagen == null)
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "No se puede abrir la imagen",
                    "El archivo elegido no es una imagen válida. Prueba con otra foto en formato JPG o PNG.",
                    TipoDialogo.Error);
                return;
            }

            _rutaFoto = dialogo.FileName;
            VistaPreviaFoto.Foto = imagen;
            BtnQuitarFoto.IsEnabled = true;
        }

        private void QuitarFoto_Click(object sender, RoutedEventArgs e)
        {
            _rutaFoto = null;
            VistaPreviaFoto.Foto = null;
            BtnQuitarFoto.IsEnabled = false;
        }

        // ===== Cancelar y salida =====

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            if (!PuedeSalir()) return;
            _salidaConfirmada = true;
            Navegar(new PersonalVista());
        }

        private bool HayCambios =>
            new[] { TxtNombre, TxtApellidos, TxtTelefonos, TxtCorreos, TxtIdiomas, TxtFormacion,
                    TxtDiasNoDisponibles, TxtRestricciones }.Any(t => t.Text.Trim().Length > 0)
            || CmbDisponibilidad.SelectedIndex > 0
            || _rutaFoto != null;

        /// <summary>Si hay datos sin guardar, pide confirmación antes de abandonar el formulario (RF-36).</summary>
        public override bool PuedeSalir()
        {
            if (_salidaConfirmada || !(EsEdicion ? HayCambiosEnEdicion : HayCambios)) return true;

            _salidaConfirmada = EsEdicion
                ? DialogoMensaje.Confirmar(VentanaPropietaria,
                    $"¿Descartar los cambios de {_original.NombreCompleto}?",
                    "Su ficha se quedará como estaba.",
                    "Descartar", "Seguir editando")
                : DialogoMensaje.Confirmar(VentanaPropietaria,
                    "¿Descartar el nuevo empleado?",
                    "Se perderán los datos que has introducido en la ficha.",
                    "Descartar", "Seguir editando");
            return _salidaConfirmada;
        }
    }
}
