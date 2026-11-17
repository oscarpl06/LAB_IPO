namespace LAB_IPO.Modelos
{
    /// <summary>
    /// Categoría de parcela con su precio por noche en cada temporada (PG7).
    /// </summary>
    public class TipoParcela
    {
        public string Nombre { get; set; }
        public decimal PrecioBaja { get; set; }
        public decimal PrecioMedia { get; set; }
        public decimal PrecioAlta { get; set; }

        public override string ToString() => Nombre;
    }
}
