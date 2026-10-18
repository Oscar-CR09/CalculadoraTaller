namespace CalculadoraTaller;

/// <summary>Un sistema de acabado (barniz, laca, pintura...) con sus productos por etapa.</summary>
class FinishSystem
{
    public string Name { get; set; } = "";
    public string Note { get; set; } = "";
    /// <summary>Unidad de la superficie a cubrir (m², m lineales...).</summary>
    public string InputUnit { get; set; } = "m²";
    public List<FinishItem> Items { get; set; } = new();
}

/// <summary>
/// Producto de una etapa. Igual que en Excel: Consumo = Cantidad × Superficie / Rendimiento
/// (Cantidad de producto que se prepara para cubrir "Rendimiento" m²).
/// </summary>
class FinishItem
{
    public string Stage { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public double Qty { get; set; }
    public string Unit { get; set; } = "LT";
    public double Yield { get; set; } = 1;
    public string Application { get; set; } = "";
    public double Price { get; set; }

    public FinishItem Clone() => (FinishItem)MemberwiseClone();
}

/// <summary>Formato de lámina/tablero (hoja).</summary>
class SheetFormat
{
    public string Type { get; set; } = "Lámina";
    public string Name { get; set; } = "";
    public double Length { get; set; }   // m
    public double Width { get; set; }    // m
}

/// <summary>Formato de perfil o barra (tramo).</summary>
class ProfileFormat
{
    public string Type { get; set; } = "Cuadrado";
    public string Name { get; set; } = "";
    public double A { get; set; }        // mm (lado, ancho o diámetro exterior)
    public double B { get; set; }        // mm (alto / segundo lado)
    public double Length { get; set; } = 6.1; // m por tramo
}

class MetalFormats
{
    public List<SheetFormat> Sheets { get; set; } = new();
    public List<ProfileFormat> Profiles { get; set; } = new();
}

static class Geometry
{
    public static readonly string[] ProfileTypes =
        { "Cuadrado", "Rectangular", "Redondo hueco", "Barra redonda", "Barra cuadrada", "Solera", "Ángulo", "Canal / Monten" };

    /// <summary>Perímetro exterior en metros. A y B en mm.</summary>
    public static double Perimeter(string type, double a, double b)
    {
        double mm = type switch
        {
            "Cuadrado" or "Barra cuadrada" => 4 * a,
            "Redondo hueco" or "Barra redonda" => Math.PI * a,
            "Canal / Monten" => 2 * a + 4 * b,      // alma + 2 patines, ambas caras
            _ => 2 * (a + b),                        // Rectangular, Solera, Ángulo (ambas caras de las alas)
        };
        return mm / 1000.0;
    }

    public static bool UsesB(string type) => type is "Rectangular" or "Solera" or "Ángulo" or "Canal / Monten";
}
