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

        /// <summary>Precio por noche en las tres temporadas, para mostrarlo en la ficha.</summary>
        public string TarifasTexto => $"Baja {PrecioBaja:0} €  ·  Media {PrecioMedia:0} €  ·  Alta {PrecioAlta:0} €";

        public override string ToString() => Nombre;
    }
}
