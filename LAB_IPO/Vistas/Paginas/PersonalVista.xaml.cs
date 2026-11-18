using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;
using LAB_IPO.Modelos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG11 · Listado de personal y monitores.
    /// CU-20: desde aquí se da de alta un empleado.
    /// CU-22: búsqueda, filtros por idioma y estado, y ficha del empleado en el panel lateral (RF-24).
    /// Editar y Dar de baja llegarán con sus casos de uso.
    /// </summary>
    public partial class PersonalVista : VistaBase
    {
        private const string TodosLosIdiomas = "Todos";
        private static readonly string[] Estados = { "Activo", "De baja", "Todos" };

        private readonly ListCollectionView _vista;

        public override string OpcionMenu => "Personal";

        public PersonalVista() : this(null)
        {
        }

        /// <param name="mensajeGuardado">Si se indica, muestra un aviso de confirmación sobre la tabla.</param>
        public PersonalVista(string mensajeGuardado)
        {
            InitializeComponent();

            _vista = new ListCollectionView(RepositorioPersonal.Empleados);
            ListaEmpleados.ItemsSource = _vista;

            CmbFiltroIdioma.ItemsSource = new[] { TodosLosIdiomas }.Concat(IdiomasRegistrados()).ToList();
            CmbFiltroIdioma.SelectedIndex = 0;
            CmbFiltroEstado.ItemsSource = Estados;
            CmbFiltroEstado.SelectedIndex = 0;   // por defecto, solo los empleados en activo
            AplicarFiltros();

            if (!string.IsNullOrEmpty(mensajeGuardado))
            {
                TxtAvisoGuardado.Text = mensajeGuardado;
                AvisoGuardado.Visibility = Visibility.Visible;
                // El empleado nuevo se añade al final: se baja hasta él para que se vea.
                Loaded += (s, e) => DesplazamientoEmpleados.ScrollToEnd();
            }
        }

        /// <summary>Idiomas distintos que aparecen en las fichas, sin el nivel (Inglés B2 → Inglés).</summary>
        private static string[] IdiomasRegistrados() =>
            RepositorioPersonal.Empleados
                .SelectMany(e => (e.Idiomas ?? "").Split(','))
                .Select(NombreIdioma)
                .Where(i => i.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(i => i, StringComparer.Create(new CultureInfo("es-ES"), true))
                .ToArray();

        private static string NombreIdioma(string idioma) =>
            Regex.Replace(idioma.Trim(), @"\s+[ABC][12]$", "", RegexOptions.IgnoreCase).Trim();

        // ===== Filtros (CU-22) =====

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
            CmbFiltroIdioma.SelectedIndex = 0;
            CmbFiltroEstado.SelectedIndex = 0;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            string texto = SinTildes(TxtBuscar.Text.Trim());
            string soloCifras = new string(texto.Where(char.IsDigit).ToArray());
            string idioma = CmbFiltroIdioma.SelectedItem as string;
            string estado = CmbFiltroEstado.SelectedItem as string ?? "Activo";

            _vista.Filter = o =>
            {
                var emp = (Empleado)o;
                if (estado == "Activo" && !emp.Activo) return false;
                if (estado == "De baja" && emp.Activo) return false;

                if (idioma != null && idioma != TodosLosIdiomas
                    && !(emp.Idiomas ?? "").Split(',').Any(i =>
                        string.Equals(NombreIdioma(i), idioma, StringComparison.OrdinalIgnoreCase)))
                    return false;

                if (texto.Length > 0)
                {
                    bool coincide =
                        SinTildes(emp.NombreCompleto).IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                        || (emp.Correos ?? "").IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                        // El teléfono se compara sin espacios: "600111" encuentra "600 111 222".
                        || (soloCifras.Length > 0 && new string((emp.Telefonos ?? "").Where(char.IsDigit).ToArray())
                                .Contains(soloCifras));
                    if (!coincide) return false;
                }
                return true;
            };

            int visibles = _vista.Count;
            int activos = RepositorioPersonal.Empleados.Count(e => e.Activo);
            TxtRecuento.Text = visibles == activos && estado == "Activo" && texto.Length == 0 && idioma == TodosLosIdiomas
                ? (activos == 1 ? "1 empleado registrado" : $"{activos} empleados registrados")
                : (visibles == 1 ? "1 empleado encontrado" : $"{visibles} empleados encontrados");
            PanelSinResultados.Visibility = visibles == 0 ? Visibility.Visible : Visibility.Collapsed;
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

        // ===== Ficha del empleado (CU-22) =====

        private void Ver_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Empleado empleado)) return;

            if (!RepositorioPersonal.Empleados.Contains(empleado))
            {
                DialogoMensaje.Mostrar(VentanaPropietaria, "Empleado no disponible",
                    "El empleado seleccionado ya no existe. Actualiza el listado y vuelve a intentarlo.",
                    TipoDialogo.Aviso);
                return;
            }

            Navegador?.AbrirPanelLateral(new FichaEmpleado(empleado));
        }

        // ===== Alta (CU-20) =====

        private void NuevoEmpleado_Click(object sender, RoutedEventArgs e)
        {
            Navegar(new FormularioEmpleadoVista());
        }
    }
}
