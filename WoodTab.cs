namespace CalculadoraTaller;

record WoodPiece(string Name, double Qty, double ThickMm, double WidthMm, double LengthMm);

/// <summary>
/// Calculadora Pie Tabla (CALCULADORA PIE TABLA.xlsx):
/// PT = ((Largo+Sierra)/1000 × (Ancho+Sierra)/1000 × Espesor" × 10.8) × 1.2 ;  Total = PT × Factor × Cantidad
/// Además calcula cuántas tablas comprar y los m² a barnizar.
/// </summary>
class WoodTab : UserControl, IProjectPart
{
    readonly DataGridView grid = UI.Grid();
    readonly TextBox txtConst = UI.Box("10.8", width: 70);
    readonly TextBox txtMerma = UI.Box("1.2", width: 70);
    readonly TextBox txtTotalPT = UI.Box("", true), txtM2 = UI.Box("", true), txtPieces = UI.Box("", true);
    readonly TextBox txtBoardT = UI.Box("1"), txtBoardW = UI.Box("12"), txtBoardL = UI.Box("8"), txtBoardPrice = UI.Box("");
    readonly TextBox txtBoardPT = UI.Box("", true), txtBoards = UI.Box("", true), txtBoardCost = UI.Box("", true), txtPricePT = UI.Box("", true);
    bool busy;

    public DataGridView MainGrid => grid;
    public Action<double>? SendToBreuer, SendToIdemo;
    public Action<List<WoodPiece>>? SendToCosteo;

    public WoodTab()
    {
        Dock = DockStyle.Fill;
        Font = new Font("Segoe UI", 9.5f);

        var head = new Label { Text = "CALCULADORA PIE TABLA  —  piezas de madera", Dock = DockStyle.Top, Font = UI.TitleFont, ForeColor = UI.Accent, Height = 30, Padding = new Padding(8, 6, 0, 0) };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6, 2, 6, 4) };
        bar.Controls.AddRange(new Control[]
        {
            UI.Lbl("PT = ((L+sierra)/1000 × (A+sierra)/1000 × Espesor\" ×"), txtConst, UI.Lbl(") ×"), txtMerma,
            UI.Lbl("    ×  Factor  ×  Cantidad          (celdas amarillas = editables)"),
        });

        UI.Col(grid, "Name", "PIEZA", 190, num: false);
        UI.Col(grid, "Len", "LARGO (mm)", 80);
        UI.Col(grid, "KerfL", "SIERRA LARGO (mm)", 75);
        UI.Col(grid, "Wid", "ANCHO (mm)", 80);
        UI.Col(grid, "KerfW", "SIERRA ANCHO (mm)", 75);
        UI.Col(grid, "Thk", "ESPESOR (in)", 75);
        UI.Col(grid, "Qty", "CANTIDAD", 75);
        UI.Col(grid, "PT", "PIE TABLA (1 pieza)", 90, ro: true, dec: 3);
        UI.Col(grid, "Factor", "FACTOR", 65);
        UI.Col(grid, "Total", "TOTAL PT", 90, ro: true);
        UI.Check(grid, "Varnish", "¿Barnizar?", 70);
        UI.Col(grid, "M2", "m² a barnizar (todas las caras)", 110, ro: true);
        grid.DefaultValuesNeeded += (s, e) =>
        {
            e.Row.Cells["KerfL"].Value = "4"; e.Row.Cells["KerfW"].Value = "4";
            e.Row.Cells["Qty"].Value = "1"; e.Row.Cells["Factor"].Value = "1.4"; e.Row.Cells["Varnish"].Value = true;
        };
        grid.CellValueChanged += (s, e) => { if (!busy) Recalc(); };
        grid.RowsRemoved += (s, e) => { if (!busy) Recalc(); };
        foreach (var tb in new[] { txtConst, txtMerma, txtBoardT, txtBoardW, txtBoardL, txtBoardPrice })
            tb.TextChanged += (s, e) => Recalc();

        var totals = UI.Form(("Total pies tabla (PT):", txtTotalPT), ("m² a barnizar:", txtM2), ("Piezas:", txtPieces));
        var boards = UI.Form(
            ("Espesor de la tabla (in):", txtBoardT), ("Ancho de la tabla (in):", txtBoardW), ("Largo de la tabla (pies):", txtBoardL),
            ("Precio por tabla ($, opcional):", txtBoardPrice), ("PT por tabla:", txtBoardPT), ("TABLAS NECESARIAS:", txtBoards),
            ("Costo de las tablas ($):", txtBoardCost), ("Precio por PT ($):", txtPricePT));
        txtBoards.BackColor = UI.TotalBack;

        var actions = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, Dock = DockStyle.Fill, Padding = new Padding(4) };
        actions.Controls.AddRange(new Control[]
        {
            UI.Btn("➜ Enviar m² a Barniz BREUER", (s, e) => SendToBreuer?.Invoke(Num.P(txtM2.Text)), 260),
            UI.Btn("➜ Enviar m² a Barniz IDEMO", (s, e) => SendToIdemo?.Invoke(Num.P(txtM2.Text)), 260),
            UI.Btn("➜ Pasar piezas a Costeo Mueble", (s, e) => SendToCosteo?.Invoke(Pieces()), 260),
            UI.Btn("Agregar fila de ejemplo", (s, e) => grid.Rows.Add("NUEVA PIEZA", "1000", "4", "100", "4", "1", "1", "", "1.4", "", true, ""), 260),
        });

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 290 };
        bottom.Controls.Add(UI.Group("Enviar datos", actions, 290));
        bottom.Controls.Add(UI.Group("¿Cuántas tablas necesito?", boards, 340));
        bottom.Controls.Add(UI.Group("Totales", totals, 270));

        Controls.Add(grid);
        Controls.Add(bottom);
        Controls.Add(bar);
        Controls.Add(head);
        LoadExample();
    }

    void LoadExample()
    {
        busy = true;
        grid.Rows.Clear();
        grid.Rows.Add("BUMPER", "1220", "4", "48", "4", "2", "1", "", "1.4", "", true, "");
        grid.Rows.Add("TIRA REFUERZO", "424", "4", "24", "4", "1", "2", "", "1.4", "", true, "");
        busy = false;
        Recalc();
    }

    void Recalc()
    {
        if (busy) return;
        busy = true;
        double k = Num.P(txtConst.Text), merma = Num.P(txtMerma.Text);
        double totalPT = 0, totalM2 = 0, pieces = 0;
        foreach (DataGridViewRow r in grid.Rows)
        {
            if (r.IsNewRow) continue;
            double L = GridIO.N(r, "Len"), W = GridIO.N(r, "Wid"), T = GridIO.N(r, "Thk"), q = GridIO.N(r, "Qty");
            double pt = ((L + GridIO.N(r, "KerfL")) / 1000) * ((W + GridIO.N(r, "KerfW")) / 1000) * T * k * merma;
            if (L == 0 || W == 0) pt = 0;
            double total = pt * GridIO.N(r, "Factor") * q;
            double tmm = T * 25.4;
            double m2 = GridIO.B(r, "Varnish") ? 2 * (L * W + L * tmm + W * tmm) / 1e6 * q : 0;
            r.Cells["PT"].Value = Num.F(pt, 3);
            r.Cells["Total"].Value = Num.F(total, 3);
            r.Cells["M2"].Value = Num.F(m2, 3);
            totalPT += total; totalM2 += m2; pieces += q;
        }
        txtTotalPT.Text = Num.F(totalPT, 3);
        txtM2.Text = Num.F(totalM2, 3);
        txtPieces.Text = Num.R(pieces);

        double boardPT = Num.P(txtBoardT.Text) * Num.P(txtBoardW.Text) * Num.P(txtBoardL.Text) * 12 / 144;
        double boards = boardPT > 0 ? Math.Ceiling(Math.Round(totalPT / boardPT, 6)) : 0;
        double price = Num.P(txtBoardPrice.Text);
        txtBoardPT.Text = Num.F(boardPT, 3);
        txtBoards.Text = boardPT > 0 ? $"{boards:0}  ({Num.F(totalPT / boardPT, 2)} exactas)" : "";
        txtBoardCost.Text = price > 0 ? Num.F(boards * price) : "";
        txtPricePT.Text = price > 0 && boardPT > 0 ? Num.F(price / boardPT) : "";
        busy = false;
    }

    List<WoodPiece> Pieces() => grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow && GridIO.N(r, "Len") > 0)
        .Select(r => new WoodPiece(GridIO.Str(r, "Name"), GridIO.N(r, "Qty"), GridIO.N(r, "Thk") * 25.4, GridIO.N(r, "Wid"), GridIO.N(r, "Len")))
        .ToList();

    public string Summary() => $"PT={txtTotalPT.Text} m2={txtM2.Text} tablas={txtBoards.Text} rows=" + string.Join(" | ", grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).Select(r => r.Cells["PT"].Value + "/" + r.Cells["Total"].Value));

    const string K = "wood";
    public void SaveTo(ProjectData p)
    {
        p.Grids[K] = GridIO.Save(grid);
        foreach (var (n, tb) in Fields()) p.Fields[$"{K}.{n}"] = tb.Text;
    }

    public void LoadFrom(ProjectData p)
    {
        busy = true;
        GridIO.Load(grid, p.Grids.GetValueOrDefault(K));
        foreach (var (n, tb) in Fields()) if (p.Fields.TryGetValue($"{K}.{n}", out var v)) tb.Text = v;
        busy = false;
        Recalc();
    }

    public void ClearAll()
    {
        busy = true; grid.Rows.Clear(); busy = false;
        Recalc();
    }

    IEnumerable<(string, TextBox)> Fields() => new[]
    {
        ("const", txtConst), ("merma", txtMerma), ("bt", txtBoardT), ("bw", txtBoardW), ("bl", txtBoardL), ("bp", txtBoardPrice),
    };
}
