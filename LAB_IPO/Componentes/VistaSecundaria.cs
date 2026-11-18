namespace LAB_IPO.Componentes
{
    /// <summary>
    /// Pantalla de consulta que se abre por encima del trabajo en curso (Ayuda, Acerca de).
    /// Recuerda la vista de la que se viene para poder volver a ella tal como estaba: por eso
    /// abrirla no pide confirmación aunque haya un formulario a medias.
    /// </summary>
    public class VistaSecundaria : VistaBase
    {
        private bool _volviendo;

        /// <summary>Vista de trabajo a la que se vuelve (nunca otra vista secundaria).</summary>
        public VistaBase Anterior { get; }

        public VistaSecundaria() : this(null)
        {
        }

        public VistaSecundaria(VistaBase anterior)
        {
            Anterior = anterior is VistaSecundaria secundaria ? secundaria.Anterior : anterior;
        }

        /// <summary>Vuelve a la vista anterior conservando sus datos, o al inicio si no la hay.</summary>
        protected void Volver(VistaBase siNoHayAnterior)
        {
            _volviendo = true;
            Navegar(Anterior ?? siNoHayAnterior);
            _volviendo = false;
        }

        /// <summary>
        /// Si desde aquí se va a otra sección, la vista anterior se abandona: se le pregunta a ella
        /// (por ejemplo, un formulario con cambios pide confirmación).
        /// </summary>
        public override bool PuedeSalir() => _volviendo || Anterior == null || Anterior.PuedeSalir();
    }
}
