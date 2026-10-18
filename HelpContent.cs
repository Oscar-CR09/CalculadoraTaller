namespace CalculadoraTaller;

/// <summary>
/// TEXTOS DE AYUDA. Aquí se edita lo que aparece en el botón "? Ayuda" de cada pestaña.
/// Formato sencillo:  "# Título"  = título grande,  "## Subtítulo" = subtítulo,  "- texto" = viñeta.
/// El orden de la lista Pages debe coincidir con el orden de las pestañas en MainForm.
/// </summary>
static class HelpContent
{
    public static readonly (string Title, string Text)[] Pages =
    {
        ("Pie Tabla (madera)", """
# Pie Tabla (madera) — cómo capturar los datos
Esta pestaña calcula los pies tabla (PT) de cada pieza, cuántas tablas debes comprar y los m² que vas a barnizar.

## Paso 1. Revisa los valores generales (barra de arriba)
- Constante 10.8 = conversión de m² × pulgada a pies tabla. Normalmente no se cambia.
- Merma 1.2 = 20 % extra por desperdicio. Cámbialo si tu taller usa otro valor.

## Paso 2. Captura una fila por cada pieza (celdas amarillas)
- PIEZA: nombre de la pieza (ej. BUMPER, COSTADO, TAPA).
- LARGO (mm) y ANCHO (mm): medida final de la pieza en milímetros.
- SIERRA LARGO / SIERRA ANCHO (mm): lo que se pierde en el corte. Por defecto 4 mm.
- ESPESOR (in): grueso de la tabla en PULGADAS (1, 1.5, 2...). ¡No en mm!
- CANTIDAD: cuántas piezas iguales lleva el mueble.
- FACTOR: factor de rendimiento de la madera (1.4 por defecto).
- ¿Barnizar?: quita la palomita si esa pieza no lleva barniz.
Las columnas azules (PIE TABLA, TOTAL PT, m²) se calculan solas.

## Paso 3. ¿Cuántas tablas necesito? (recuadro de abajo)
- Escribe el espesor (in), ancho (in) y largo (pies) de la tabla que compras.
- Opcional: escribe el precio por tabla para ver el costo total y el precio por PT.
- El resultado "TABLAS NECESARIAS" ya viene redondeado hacia arriba.

## Paso 4. Envía los datos
- "Enviar m² a Barniz BREUER / IDEMO": pasa los m² a barnizar a esa pestaña.
- "Pasar piezas a Costeo Mueble": copia las piezas para sacar el costo y precio de venta.

## Consejos
- Clic derecho en la tabla = insertar o eliminar filas.
- Puedes usar punto o coma como decimal (1.5 o 1,5).
"""),
        ("Costeo Mueble", """
# Costeo Mueble — cómo capturar los datos
Calcula los pies tabla con la fórmula estándar (pulgadas³ / 144), el costo de la madera y el precio de venta sugerido.

## Paso 1. Captura las piezas
- Puedes escribirlas aquí o traerlas desde la pestaña Pie Tabla con el botón "Pasar piezas a Costeo".
- Cant.: número de piezas iguales.
- Espesor, Ancho y Largo: en MILÍMETROS (el programa los convierte a pulgadas).
- % Merma: desperdicio de corte/cepillado en porcentaje (20 = 20 %). Típico 15 a 30.
- Precio $/PT: precio de la madera por pie tabla.

## Paso 2. Bloque A — precio por PT a partir de una tabla comprada
- Escribe espesor (pulg), ancho (pulg), largo (pies) y lo que pagaste por la tabla.
- El programa calcula el $/PT. Presiona "Aplicar este $/PT a todas las piezas" para usarlo.
- También indica cuántas tablas de ese tamaño necesitas para el mueble.

## Paso 3. Bloque B — costeo total
- Mano de obra, herrajes, acabados y otros costos: en pesos.
- "Acabados y pintura" se puede llenar solo con el botón "Enviar costo a Costeo" de las pestañas de barniz.
- Margen de utilidad (%): 30 = 30 %. Precio de venta = Subtotal ÷ (1 − margen).
"""),
        ("Metal (m² láminas y perfiles)", """
# Metal — cómo capturar los datos
Calcula los m² de láminas, tableros, perfiles y barras para pintura electrostática.

## Tabla de LÁMINAS Y TABLEROS (arriba)
- Formato: elige de la lista (ej. Lámina 4' x 8'). Se llenan solas las medidas.
  Si no está tu medida, elige "(Personalizado)" y escribe Largo y Ancho en METROS.
- Descripción / calibre: texto libre (ej. Cal. 18).
- Hojas: cuántas hojas ocupas (acepta decimales, ej. 1.5).
- Caras a pintar: 2 si se pinta por ambos lados, 1 si solo por un lado.
- % considerado: 100 = toda la hoja. Usa menos si solo se pinta una parte.

## Tabla de PERFILES Y BARRAS (abajo)
- Formato: elige el perfil (PTR, tubo redondo, barra, solera, ángulo...).
- Tipo: define cómo se calcula el perímetro:
  - Cuadrado / Barra cuadrada: 4 × A
  - Rectangular / Solera / Ángulo: 2 × (A + B)
  - Redondo hueco / Barra redonda: π × A  (A = diámetro exterior)
- A y B en MILÍMETROS. B solo se usa en rectangular, solera, ángulo y canal (en gris si no aplica).
- Largo del tramo (m): normalmente 6.0 o 6.10 m.
- Tramos: cuántos tramos ocupas (acepta decimales, ej. 2.5).

## Formatos
- Botón "Editar formatos...": agrega, cambia o borra tus medidas de láminas y perfiles. Se guardan para la próxima vez.

## Enviar
- "Enviar m² a Pintura Electrostática" pasa el total a la pestaña IDEMO y calcula los kilos de pintura.
"""),
        ("Barniz BREUER", """
# Barniz BREUER — cómo usar la tabla
Calcula los litros de cada producto según la superficie a barnizar.

## Paso 1. Elige el sistema
- En "Sistema" elige el acabado: poliuretano mate/brillante, poro abierto/cerrado, Polyform, Bona, sellador, laca automotiva.
- Lee la NOTA en color café: indica en qué muebles se usa.

## Paso 2. Escribe la superficie
- "Superficie a cubrir" en m². Se llena sola si usas el botón "Enviar m²" de la pestaña Pie Tabla.

## Paso 3. Lee los resultados
- CONSUMO (columna azul) = Cantidad por preparación × Superficie ÷ Rendimiento.
- "Resumen por etapa": litros de base, fondo y acabado.
- "Total por producto": lo que tienes que surtir de almacén (suma el mismo producto de todas las etapas).

## Editar recetas (opcional)
- Puedes cambiar cantidades, rendimientos o agregar productos directo en la tabla (celdas amarillas).
- Precio $ por unidad: si lo llenas, se calcula el costo del acabado.
- Presiona "Guardar recetas" para que los cambios se queden. "Restaurar originales" regresa a los valores del Excel.
- "Nuevo sistema", "Duplicar", "Renombrar / Nota" y "Eliminar sistema" administran la lista de sistemas.
- "Enviar costo a Costeo" pasa el costo al campo "Acabados y pintura" de Costeo Mueble.
"""),
        ("Barniz IDEMO / Pintura", """
# Barniz IDEMO / Pintura — cómo usar la tabla
Funciona igual que la pestaña BREUER, con los sistemas de la tabla IDEMO.

## Sistemas incluidos
- Acabado natural, Entintado, Poliuretano F-218, Laqueado, Lacas (igualado de color), Barniz marino, Impermeabilizante.
- PINTURA ELECTROSTÁTICA: 1 kg por cada 7 m². Recibe los m² desde la pestaña Metal.
- ADHESIVO SUPERFICIE SÓLIDA: aquí la superficie va en METROS LINEALES de unión.

## Importante
- En Entintado, Lacas y Adhesivo la tabla trae varias opciones (colores o presentaciones).
  El total las suma todas: borra o pon 0 en las que no vayas a usar.
- "Entintado" y "Lacas" se SUMAN al proceso base (Acabado natural o Laqueado): consulta ambos sistemas.

## Pasos
- Elige el sistema, escribe la superficie y lee el consumo, igual que en BREUER.
- Guarda tus cambios de recetas con "Guardar recetas".
"""),
        ("General", """
# Uso general del programa
## Colores
- Amarillo = dato que tú capturas.  Azul = calculado.  Verde = resultado principal.

## Tablas
- Escribe en la última fila (vacía) para agregar una pieza nueva.
- Clic derecho: insertar fila arriba o eliminar filas seleccionadas.
- Tecla Supr: borra el contenido de las celdas seleccionadas.
- Acepta punto o coma decimal.

## Proyectos
- Archivo → Guardar proyecto (Ctrl+S) guarda TODO lo capturado en un archivo .taller.json.
- Archivo → Abrir proyecto (Ctrl+O) lo vuelve a cargar.
- Archivo → Exportar (Ctrl+E) guarda la tabla de la pestaña actual en CSV para abrirla en Excel.

## Datos guardados
- Las recetas de barniz y los formatos de metal se guardan en la carpeta "datos" junto al programa.
- Si copias el programa a otra computadora, copia también esa carpeta.

## Atajo
- F1 abre esta ayuda en la pestaña donde estás.
"""),
    };
}

/// <summary>Ventana de ayuda con una pestaña por cada sección.</summary>
class HelpForm : Form
{
    public HelpForm(int startPage)
    {
        Text = "Ayuda — cómo ingresar los datos";
        Width = 820; Height = 700; StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 10f);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        foreach (var (title, text) in HelpContent.Pages)
        {
            var rtb = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Color.White, Padding = new Padding(10) };
            Fill(rtb, text);
            var page = new TabPage(title) { Padding = new Padding(10), BackColor = Color.White };
            page.Controls.Add(rtb);
            tabs.TabPages.Add(page);
        }
        tabs.SelectedIndex = Math.Clamp(startPage, 0, tabs.TabCount - 1);
        var close = new Button { Text = "Cerrar", Dock = DockStyle.Bottom, Height = 36, DialogResult = DialogResult.OK };
        Controls.Add(tabs);
        Controls.Add(close);
        AcceptButton = close;
        CancelButton = close;
    }

    static void Fill(RichTextBox rtb, string text)
    {
        var h1 = new Font("Segoe UI", 15f, FontStyle.Bold);
        var h2 = new Font("Segoe UI", 11.5f, FontStyle.Bold);
        var body = new Font("Segoe UI", 10.5f);
        foreach (var raw in text.Replace("\r", "").Split('\n'))
        {
            string line = raw.TrimEnd();
            Font f = body; Color c = Color.Black; string t = line;
            if (line.StartsWith("# ")) { f = h1; c = UI.Accent; t = line[2..]; }
            else if (line.StartsWith("## ")) { f = h2; c = Color.FromArgb(160, 80, 0); t = "\n" + line[3..]; }
            else if (line.TrimStart().StartsWith("- "))
            {
                int indent = line.Length - line.TrimStart().Length;
                t = new string(' ', indent * 2 + 3) + "•  " + line.TrimStart()[2..];
            }
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionFont = f;
            rtb.SelectionColor = c;
            rtb.AppendText(t + "\n");
        }
        rtb.SelectionStart = 0;
    }
}
