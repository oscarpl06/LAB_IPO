using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using LAB_IPO.Modelos;

namespace LAB_IPO.Datos
{
    /// <summary>
    /// Tipos de parcela, ubicaciones y parcelas de prueba, en memoria.
    /// Los datos coinciden con los bocetos PG3 y PG7.
    /// </summary>
    public static class RepositorioAlojamientos
    {
        private const string RutaFotos = "pack://application:,,,/LAB_IPO;component/Recursos/Fotos/Parcelas/";

        public static ObservableCollection<TipoParcela> TiposParcela { get; } = new ObservableCollection<TipoParcela>
        {
            new TipoParcela { Nombre = "Pequeña",      PrecioBaja = 18, PrecioMedia = 22, PrecioAlta = 28 },
            new TipoParcela { Nombre = "Mediana",      PrecioBaja = 22, PrecioMedia = 27, PrecioAlta = 34 },
            new TipoParcela { Nombre = "Deluxe",       PrecioBaja = 30, PrecioMedia = 38, PrecioAlta = 48 },
            new TipoParcela { Nombre = "Autocaravana", PrecioBaja = 26, PrecioMedia = 32, PrecioAlta = 40 }
        };

        /// <summary>Zonas de referencia para situar una parcela dentro del camping.</summary>
        public static string[] Ubicaciones { get; } =
        {
            "Cerca de aseos",
            "Cerca de fregaderos",
            "Junto a la piscina",
            "Cerca del bar-restaurante",
            "Cerca de la entrada",
            "Zona de sombra"
        };

        public static ObservableCollection<Parcela> Parcelas { get; } = new ObservableCollection<Parcela>
        {
            new Parcela
            {
                Identificador = "P-03", Tipo = Tipo("Mediana"), TamanoM2 = 80, Ubicacion = "Cerca de aseos",
                TomaAgua = true, TomaLuz = true, Desague = true,
                Estado = EstadoAlojamiento.Libre, RutaFoto = RutaFotos + "parcela-p03.jpg"
            },
            new Parcela
            {
                Identificador = "P-06", Tipo = Tipo("Deluxe"), TamanoM2 = 120, Ubicacion = "Junto a la piscina",
                TomaAgua = true, TomaLuz = true, Desague = true, DelimitadaSetos = true,
                Estado = EstadoAlojamiento.Ocupado, RutaFoto = RutaFotos + "parcela-p06.jpg"
            },
            new Parcela
            {
                Identificador = "P-12", Tipo = Tipo("Pequeña"), TamanoM2 = 50, Ubicacion = "Cerca de la entrada",
                TomaAgua = true, TomaLuz = true,
                Estado = EstadoAlojamiento.Libre, RutaFoto = RutaFotos + "parcela-p12.jpg"
            },
            new Parcela
            {
                Identificador = "P-15", Tipo = Tipo("Autocaravana"), TamanoM2 = 100, Ubicacion = "Cerca del bar-restaurante",
                TomaAgua = true, TomaLuz = true, Desague = true,
                Estado = EstadoAlojamiento.EnReparacion, RutaFoto = RutaFotos + "parcela-p15.jpg"
            },
            new Parcela
            {
                Identificador = "P-21", Tipo = Tipo("Mediana"), TamanoM2 = 75, Ubicacion = "Zona de sombra",
                TomaAgua = true, TomaLuz = true, ZonaSombra = true,
                Estado = EstadoAlojamiento.PendienteLimpieza, RutaFoto = RutaFotos + "parcela-p21.jpg"
            }
        };

        private static TipoParcela Tipo(string nombre) => TiposParcela.First(t => t.Nombre == nombre);

        public static bool ExisteParcela(string identificador) =>
            Parcelas.Any(p => string.Equals(p.Identificador, identificador, StringComparison.OrdinalIgnoreCase));

        /// <summary>Primer identificador libre con el formato P-NN, para sugerirlo en el formulario.</summary>
        public static string SiguienteIdentificadorParcela()
        {
            int maximo = Parcelas
                .Select(p => Regex.Match(p.Identificador ?? "", @"^P-(\d+)$", RegexOptions.IgnoreCase))
                .Where(m => m.Success)
                .Select(m => int.Parse(m.Groups[1].Value))
                .DefaultIfEmpty(0)
                .Max();
            return $"P-{maximo + 1:00}";
        }

        public static void AgregarParcela(Parcela parcela)
        {
            if (parcela == null) throw new ArgumentNullException(nameof(parcela));
            if (ExisteParcela(parcela.Identificador))
                throw new InvalidOperationException($"Ya existe una parcela con el identificador {parcela.Identificador}.");
            Parcelas.Add(parcela);
        }

        // ===== Bungalows =====

        private const string RutaFotosBungalows = "pack://application:,,,/LAB_IPO;component/Recursos/Fotos/Bungalows/";

        public static ObservableCollection<Bungalow> Bungalows { get; } = new ObservableCollection<Bungalow>
        {
            new Bungalow
            {
                Nombre = "Bungalow 01", TamanoM2 = 35, CapacidadMaxima = 2, PrecioNoche = 55, EstanciaMinima = 2,
                Menaje = true, Microondas = true, WiFi = true,
                Descripcion = "Bungaló para parejas junto a la pradera, con porche de madera y vistas al bosque.",
                Estado = EstadoAlojamiento.Ocupado,
                Fotos = { RutaFotosBungalows + "bungalow-01.jpg" }
            },
            new Bungalow
            {
                Nombre = "Bungalow 02", TamanoM2 = 40, CapacidadMaxima = 3, PrecioNoche = 60,
                Menaje = true, Microondas = true, RopaCamaToallas = true,
                Descripcion = "Cabaña de tejado a dos aguas con salón, dormitorio y altillo, a dos pasos de la zona de juegos.",
                Estado = EstadoAlojamiento.Libre,
                Fotos = { RutaFotosBungalows + "bungalow-02.jpg" }
            },
            new Bungalow
            {
                Nombre = "Bungalow 04", TamanoM2 = 45, CapacidadMaxima = 4, PrecioNoche = 65, EstanciaMinima = 2,
                Menaje = true, Microondas = true, RopaCamaToallas = true, WiFi = true,
                Descripcion = "Bungaló familiar con dos dormitorios, cocina equipada y terraza a la sombra de los pinos.",
                Estado = EstadoAlojamiento.Ocupado,
                Fotos = { RutaFotosBungalows + "bungalow-04.jpg", RutaFotosBungalows + "bungalow-04-dormitorio.jpg" }
            },
            new Bungalow
            {
                Nombre = "Bungalow 07", TamanoM2 = 55, CapacidadMaxima = 6, PrecioNoche = 85, EstanciaMinima = 3,
                Menaje = true, Microondas = true, RopaCamaToallas = true, WiFi = true,
                Descripcion = "El más amplio del complejo: tres dormitorios, salón con chimenea y vistas al río.",
                Estado = EstadoAlojamiento.PendienteLimpieza,
                Fotos = { RutaFotosBungalows + "bungalow-07.jpg", RutaFotosBungalows + "bungalow-07-dormitorio.jpg" }
            }
        };

        public static bool ExisteBungalow(string nombre) =>
            Bungalows.Any(b => string.Equals(b.Nombre?.Trim(), nombre?.Trim(), StringComparison.OrdinalIgnoreCase));

        /// <summary>Siguiente nombre libre con el formato "Bungalow NN", para sugerirlo en el formulario.</summary>
        public static string SiguienteNombreBungalow()
        {
            int maximo = Bungalows
                .Select(b => Regex.Match(b.Nombre ?? "", @"^Bungalow (\d+)$", RegexOptions.IgnoreCase))
                .Where(m => m.Success)
                .Select(m => int.Parse(m.Groups[1].Value))
                .DefaultIfEmpty(0)
                .Max();
            return $"Bungalow {maximo + 1:00}";
        }

        public static void AgregarBungalow(Bungalow bungalow)
        {
            if (bungalow == null) throw new ArgumentNullException(nameof(bungalow));
            if (ExisteBungalow(bungalow.Nombre))
                throw new InvalidOperationException($"Ya existe un bungaló llamado {bungalow.Nombre}.");
            Bungalows.Add(bungalow);
        }
    }
}
