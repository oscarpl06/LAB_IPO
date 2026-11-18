using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// CU-27 · Ficha de consulta de una actividad (RF-29), en el panel lateral.
    /// CU-28 · Desde la sección de monitores se pasa a un modo de gestión con casillas para
    /// asignar o quitar monitores (RF-30); al guardar se vuelve a la consulta actualizada.
    /// </summary>
    public partial class FichaActividad : UserControl
    {
        private readonly Actividad _actividad;
        private List<OpcionMonitor> _candidatos = new List<OpcionMonitor>();

        /// <summary>Se lanza tras guardar los monitores, para que el listado se actualice.</summary>
        public event EventHandler MonitoresCambiados;

        public FichaActividad(Actividad actividad)
        {
            InitializeComponent();
            _actividad = actividad;
            DataContext = actividad;
            MostrarMonitores();
        }

        /// <summary>Empleado que se puede marcar como monitor de la actividad.</summary>
        public class OpcionMonitor : INotifyPropertyChanged
        {
            private bool _seleccionado;

            public Empleado Empleado { get; set; }

            public bool Seleccionado
            {
                get => _seleccionado;
                set
                {
                    if (_seleccionado == value) return;
                    _seleccionado = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Seleccionado)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ayuda)));
                }
            }

            public string Detalle => Empleado.Activo
                ? $"{Empleado.Formacion} · {Empleado.Disponibilidad}"
                : "De baja · se quitará si lo desmarcas";

            public string Ayuda => Seleccionado
                ? $"Desmarca para quitar a {Empleado.NombreCompleto} de la actividad"
                : $"Marca para asignar a {Empleado.NombreCompleto} a la actividad";

            public event PropertyChangedEventHandler PropertyChanged;
        }

        // ===== Consulta =====

        private void MostrarMonitores()
        {
            ListaMonitores.ItemsSource = null;
            ListaMonitores.ItemsSource = _actividad.Monitores;
            AvisoSinMonitores.Visibility = _actividad.TieneMonitores ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as INavegador)?.CerrarPanelLateral();
        }

        // ===== Gestión de monitores (CU-28) =====

        private void GestionarMonitores_Click(object sender, RoutedEventArgs e)
        {
            if (!RepositorioActividades.Actividades.Contains(_actividad))
            {
                DialogoMensaje.Mostrar(Window.GetWindow(this), "Actividad no disponible",
                    "La actividad ya no existe. Cierra la ficha y actualiza el listado.", TipoDialogo.Aviso);
                return;
            }

            // Empleados en activo y, si alguno de los monitores actuales ya está de baja, también él.
            _candidatos = RepositorioPersonal.Empleados
                .Where(emp => emp.Activo || _actividad.Monitores.Contains(emp))
                .OrderByDescending(emp => _actividad.Monitores.Contains(emp))
                .ThenBy(emp => emp.NombreCompleto)
                .Select(emp => new OpcionMonitor { Empleado = emp, Seleccionado = _actividad.Monitores.Contains(emp) })
                .ToList();
            ListaCandidatos.ItemsSource = _candidatos;
            ActualizarRecuento();

            AvisoMonitoresGuardados.Visibility = Visibility.Collapsed;
            CambiarModo(gestion: true);
        }

        private void CambiarModo(bool gestion)
        {
            VistaConsulta.Visibility = gestion ? Visibility.Collapsed : Visibility.Visible;
            VistaGestion.Visibility = gestion ? Visibility.Visible : Visibility.Collapsed;
            BtnEditarActividad.Visibility = gestion ? Visibility.Collapsed : Visibility.Visible;
            PieGestion.Visibility = gestion ? Visibility.Visible : Visibility.Collapsed;
            TxtRotulo.Text = gestion ? "MONITORES DE LA ACTIVIDAD" : "FICHA DE LA ACTIVIDAD";
        }

        private void Candidato_Cambiado(object sender, RoutedEventArgs e) => ActualizarRecuento();

        private void QuitarTodos_Click(object sender, RoutedEventArgs e)
        {
            foreach (var c in _candidatos) c.Seleccionado = false;
            ActualizarRecuento();
        }

        private void ActualizarRecuento()
        {
            int n = _candidatos.Count(c => c.Seleccionado);
            TxtSeleccionados.Text = n == 0 ? "Ningún monitor seleccionado"
                : n == 1 ? "1 monitor seleccionado" : $"{n} monitores seleccionados";
        }

        private void CancelarMonitores_Click(object sender, RoutedEventArgs e) => CambiarModo(gestion: false);

        private void GuardarMonitores_Click(object sender, RoutedEventArgs e)
        {
            var elegidos = _candidatos.Where(c => c.Seleccionado).Select(c => c.Empleado).ToList();
            var ventana = Window.GetWindow(this);

            // Si algún empleado ya no existe, no se registra esa asignación.
            var inexistentes = elegidos.Where(emp => !RepositorioPersonal.Empleados.Contains(emp)).ToList();
            if (inexistentes.Count > 0)
            {
                DialogoMensaje.Mostrar(ventana, "Empleado no disponible",
                    $"{string.Join(", ", inexistentes.Select(i => i.NombreCompleto))} ya no "
                    + (inexistentes.Count == 1 ? "existe" : "existen")
                    + " en el registro de personal y no se puede asignar. Revisa la selección.", TipoDialogo.Aviso);
                foreach (var c in _candidatos.Where(c => inexistentes.Contains(c.Empleado))) c.Seleccionado = false;
                ActualizarRecuento();
                return;
            }

            var anteriores = _actividad.Monitores;
            bool sinCambios = elegidos.Count == anteriores.Count && elegidos.All(anteriores.Contains);
            if (sinCambios)
            {
                CambiarModo(gestion: false);
                return;
            }

            if (elegidos.Count == 0 && !DialogoMensaje.Confirmar(ventana,
                    "¿Dejar la actividad sin monitores?",
                    $"«{_actividad.Nombre}» se quedará sin ningún monitor responsable.",
                    "Quitar monitores", "Seguir eligiendo"))
                return;

            int añadidos = elegidos.Count(emp => !anteriores.Contains(emp));
            int quitados = anteriores.Count(emp => !elegidos.Contains(emp));

            // Se conserva el orden: primero los que ya estaban, después los nuevos.
            _actividad.Monitores = anteriores.Where(elegidos.Contains)
                .Concat(elegidos.Where(emp => !anteriores.Contains(emp)))
                .ToList();

            MostrarMonitores();
            TxtMonitoresGuardados.Text = Resumen(añadidos, quitados);
            AvisoMonitoresGuardados.Visibility = Visibility.Visible;
            CambiarModo(gestion: false);
            Dispatcher.BeginInvoke(new Action(() => VistaConsulta.ScrollToEnd()),
                System.Windows.Threading.DispatcherPriority.Loaded);

            MonitoresCambiados?.Invoke(this, EventArgs.Empty);
        }

        private static string Resumen(int añadidos, int quitados)
        {
            var partes = new List<string>();
            if (añadidos > 0) partes.Add(añadidos == 1 ? "1 monitor asignado" : $"{añadidos} monitores asignados");
            if (quitados > 0) partes.Add(quitados == 1 ? "1 monitor quitado" : $"{quitados} monitores quitados");
            return "Monitores actualizados: " + string.Join(" y ", partes) + ".";
        }
    }
}
