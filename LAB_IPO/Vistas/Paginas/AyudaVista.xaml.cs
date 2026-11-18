using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using LAB_IPO.Componentes;
using LAB_IPO.Datos;

namespace LAB_IPO.Vistas.Paginas
{
    /// <summary>
    /// PG14 · CU-29 Centro de ayuda (RF-34). Índice por temas con búsqueda y, a la derecha,
    /// los pasos del tema elegido. Con F1 se abre directamente el tema de la pantalla actual;
    /// la ayuda contextual de cada elemento son sus tooltips (RF-35).
    /// </summary>
    public partial class AyudaVista : VistaSecundaria
    {
        private TemaAyuda _temaActual;

        public override string OpcionMenu => "Ayuda";

        public AyudaVista() : this(null)
        {
        }

        /// <param name="anterior">Pantalla desde la que se abre; su tema se muestra primero.</param>
        public AyudaVista(VistaBase anterior) : base(anterior)
        {
            InitializeComponent();

            BtnVolver.Visibility = Anterior != null ? Visibility.Visible : Visibility.Collapsed;
            BtnVolver.ToolTip = "Volver a la pantalla en la que estabas, tal como la dejaste";
            if (Anterior != null) TxtVolver.Text = $"Volver a {Anterior.OpcionMenu}";

            string clave = TemaDe(Anterior);
            var tema = TemasAyuda.Todos.FirstOrDefault(t => t.Clave == clave) ?? TemasAyuda.Todos[0];
            DibujarIndice(TemasAyuda.Todos.ToList());
            Mostrar(tema);
        }

        /// <summary>Tema de ayuda que corresponde a cada pantalla (ayuda sensible al contexto).</summary>
        private static string TemaDe(VistaBase vista)
        {
            switch (vista)
            {
                case FormularioReservaVista _: return TemasAyuda.CrearReserva;
                case ReservasVista _: return TemasAyuda.ModificarReserva;
                case DisponibilidadVista _: return TemasAyuda.EstadosDisponibilidad;
                case GaleriaBungalowVista _: return TemasAyuda.GaleriaFotos;
                case AlojamientosVista _:
                case FormularioParcelaVista _:
                case FormularioBungalowVista _: return TemasAyuda.ParcelasBungalows;
                case PersonalVista _:
                case FormularioEmpleadoVista _: return TemasAyuda.AltaBajaPersonal;
                case ActividadesVista _:
                case FormularioActividadVista _: return TemasAyuda.HorariosCupos;
                default: return TemasAyuda.Sesion;
            }
        }

        // ===== Índice =====

        private void DibujarIndice(System.Collections.Generic.List<TemaAyuda> temas)
        {
            Indice.Children.Clear();
            foreach (var grupo in temas.GroupBy(t => t.Grupo))
            {
                Indice.Children.Add(new TextBlock
                {
                    Text = grupo.Key,
                    Style = (Style)FindResource("Texto.Etiqueta"),
                    Margin = new Thickness(4, Indice.Children.Count == 0 ? 4 : 14, 0, 6)
                });
                foreach (var tema in grupo)
                {
                    var opcion = new RadioButton
                    {
                        Content = tema.Titulo,
                        Tag = tema,
                        GroupName = "TemasAyuda",
                        Style = (Style)FindResource("Tema"),
                        IsChecked = tema == _temaActual,
                        ToolTip = tema.Encabezado
                    };
                    opcion.Checked += (s, e) => Mostrar((TemaAyuda)((FrameworkElement)s).Tag);
                    Indice.Children.Add(opcion);
                }
            }
            TxtSinTemas.Visibility = temas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Mostrar(TemaAyuda tema)
        {
            if (tema == null) return;
            _temaActual = tema;
            TxtGrupo.Text = tema.Grupo.ToUpper();
            TxtEncabezado.Text = tema.Encabezado;
            TxtIntroduccion.Text = tema.Introduccion;
            ListaPasos.ItemsSource = tema.Pasos.Select((p, i) => new { Numero = i + 1, Texto = p }).ToList();

            foreach (var opcion in Indice.Children.OfType<RadioButton>())
                if (opcion.Tag == tema && opcion.IsChecked != true) opcion.IsChecked = true;
        }

        // ===== Búsqueda =====

        private void Buscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = SinTildes(TxtBuscar.Text.Trim());
            var temas = TemasAyuda.Todos
                .Where(t => texto.Length == 0
                            || SinTildes(t.Titulo + " " + t.Grupo + " " + t.Encabezado + " " + string.Join(" ", t.Pasos))
                                .IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            DibujarIndice(temas);
            // Si el tema que se ve ya no está en la lista, se muestra el primero que coincida.
            if (temas.Count > 0 && !temas.Contains(_temaActual)) Mostrar(temas[0]);
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

        // ===== Pie =====

        private void Volver_Click(object sender, RoutedEventArgs e) => Volver(new InicioVista());

        private void Soporte_Click(object sender, RoutedEventArgs e)
        {
            DialogoMensaje.Mostrar(VentanaPropietaria, "Contactar con soporte",
                "Soporte técnico de Camping Timoteo\n\n"
                + "Teléfono: 900 123 456 (gratuito)\n"
                + "Correo: soporte@campingtimoteo.es\n"
                + "Horario: de lunes a viernes, de 9:00 a 18:00\n\n"
                + "Indica tu usuario y la pantalla en la que estabas para que podamos ayudarte antes.");
        }
    }
}
