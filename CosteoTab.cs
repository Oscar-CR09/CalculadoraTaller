namespace CalculadoraTaller;

/// <summary>
/// Pies tabla y costeo del mueble (pies_tabla_costeo.xlsx):
/// PT = (Espesor" × Ancho" × Largo") / 144 ; PT c/merma = PT × (1 + %merma) ; Costo = PT total × $/PT
/// </summary>
class CosteoTab : UserControl, IProjectPart
{
    readonly DataGridView grid = UI.Grid();
    readonly TextBox txtTotalPT = UI.Box("", true), txtMaterial = UI.Box("", true);
    readonly TextBox bT = UI.Box("1"), bW = UI.Box("6"), bL = UI.Box("8"), bPrice = UI.Box("45");
    readonly TextBox bPT = UI.Box("", true), bPricePT = UI.Box("", true), bBoards = UI.Box("", true);
    readonly TextBox cLabor = UI.Box("0"), cHardware = UI.Box("0"), cFinish = UI.Box("0"), cOther = UI.Box("0"), cMargin = UI.Box("30");
    readonly TextBox cMaterial = UI.Box("", true), cSubtotal = UI.Box("", true), cSale = UI.Box("", true), cProfit = UI.Box("", true);
    bool busy;

    public DataGridView MainGrid => grid;

    public CosteoTab()
    {
        Dock = DockStyle.Fill;
        Font = new Font("Segoe UI", 9.5f);
        var head = new Label { Text = "PIES TABLA (PT) Y COSTO DE MATERIAL — MUEBLE", Dock = DockStyle.Top, Font = UI.TitleFont, ForeColor = UI.Accent, Height = 30, Padding = new Padding(8, 6, 0, 0) };
        var sub = new Label { Text = "PT = (Espesor\" × Ancho\" × Largo\") / 144     |     1 PT = tabla de 1\" × 12\" × 12\"     |     Amarillo = editable", Dock = DockStyle.Top, Height = 22, Padding = new Padding(8, 0, 0, 0) };

        UI.Col(grid, "No", "No.", 40, ro: true);
        UI.Col(grid, "Desc", "Descripción de pieza", 230, num: false);
        UI.Col(grid, "Qty", "Cant.", 55);
        UI.Col(grid, "T", "Espesor (mm)", 70);
        UI.Col(grid, "W", "Ancho (mm)", 70);
        UI.Col(grid, "L", "Largo (mm)", 70);
        UI.Col(grid, "Tin", "Espesor (pulg)", 70, ro: true);
        UI.Col(grid, "Win", "Ancho (pulg)", 70, ro: true);
        UI.Col(grid, "Lin", "Largo (pulg)", 70, ro: true);
        UI.Col(grid, "PT", "PT unitario (1 pieza)", 85, ro: true, dec: 3);
        UI.Col(grid, "Merma", "% Merma (desperdicio)", 75);
        UI.Col(grid, "PTm", "PT c/merma (1 pieza)", 85, ro: true);
        UI.Col(grid, "PTt", "PT total (× cant.)", 85, ro: true);
        UI.Col(grid, "Price", "Precio $/PT", 75);
        UI.Col(grid, "Cost", "Costo material ($)", 100, ro: true);
        grid.DefaultValuesNeeded += (s, e) =>
        {
            e.Row.Cells["Qty"].Value = "1"; e.Row.Cells["Merma"].Value = "20";
            e.Row.Cells["Price"].Value = bPricePT.Text.Replace(",", "");
        };
        grid.CellValueChanged += (s, e) => { if (!busy) Recalc(); };
        grid.RowsRemoved += (s, e) => { if (!busy) Recalc(); };
        foreach (var tb in new[] { bT, bW, bL, bPrice, cLabor, cHardware, cFinish, cOther, cMargin }) tb.TextChanged += (s, e) => Recalc();

        var gA = UI.Form(("Espesor tabla comprada (pulg, 4/4 = 1\"):", bT), ("Ancho tabla comprada (pulg):", bW),
            ("Largo tabla comprada (pies):", bL), ("Precio pagado por la tabla ($):", bPrice), ("PT de la tabla comprada:", bPT),
            ("Precio $/PT resultante:", bPricePT), ("Tablas necesarias para el mueble:", bBoards));
        var applyBtn = UI.Btn("Aplicar este $/PT a todas las piezas", (s, e) => ApplyPricePT());
        gA.Controls.Add(new Label()); gA.Controls.Add(applyBtn);

        var gB = UI.Form(("Costo de material (madera):", cMaterial), ("Mano de obra ($):", cLabor), ("Herrajes y accesorios ($):", cHardware),
            ("Acabados y pintura ($):", cFinish), ("Otros costos indirectos ($):", cOther), ("SUBTOTAL costo de fabricación:", cSubtotal),
            ("Margen de utilidad deseado (%):", cMargin), ("PRECIO DE VENTA SUGERIDO:", cSale), ("Utilidad bruta ($):", cProfit));
        cSale.BackColor = UI.TotalBack;

        var gT = UI.Form(("TOTAL PT del mueble:", txtTotalPT), ("TOTAL costo material ($):", txtMaterial));

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 330 };
        bottom.Controls.Add(UI.Group("B) Costeo total del mueble", gB, 360));
        bottom.Controls.Add(UI.Group("A) Precio $/PT a partir de una tabla comprada", gA, 400));
        bottom.Controls.Add(UI.Group("Totales del mueble", gT, 290));

        Controls.Add(grid);
        Controls.Add(bottom);
        Controls.Add(sub);
        Controls.Add(head);

        busy = true;
        grid.Rows.Add("", "Costado lateral (EJEMPLO - reemplazar)", "2", "20", "200", "1200", "", "", "", "", "20", "", "", "35", "");
        busy = false;
        Recalc();
    }

    void Recalc()
    {
        if (busy) return;
        busy = true;
        double totPT = 0, totCost = 0; int n = 0;
        foreach (DataGridViewRow r in grid.Rows)
        {
            if (r.IsNewRow) continue;
            n++;
            r.Cells["No"].Value = n.ToString();
            double t = GridIO.N(r, "T") / 25.4, w = GridIO.N(r, "W") / 25.4, l = GridIO.N(r, "L") / 25.4;
            double pt = t * w * l / 144;
            double ptm = pt * (1 + GridIO.N(r, "Merma") / 100);
            double ptt = ptm * GridIO.N(r, "Qty");
            double cost = ptt * GridIO.N(r, "Price");
            r.Cells["Tin"].Value = Num.F(t, 3); r.Cells["Win"].Value = Num.F(w, 3); r.Cells["Lin"].Value = Num.F(l, 3);
            r.Cells["PT"].Value = Num.F(pt, 3); r.Cells["PTm"].Value = Num.F(ptm, 3); r.Cells["PTt"].Value = Num.F(ptt, 3);
            r.Cells["Cost"].Value = Num.F(cost);
            totPT += ptt; totCost += cost;
        }
        txtTotalPT.Text = Num.F(totPT, 3);
        txtMaterial.Text = Num.F(totCost);

        double boardPT = Num.P(bT.Text) * Num.P(bW.Text) * (Num.P(bL.Text) * 12) / 144;
        bPT.Text = Num.F(boardPT, 3);
        bPricePT.Text = boardPT > 0 ? Num.F(Num.P(bPrice.Text) / boardPT) : "";
        bBoards.Text = boardPT > 0 ? $"{Math.Ceiling(Math.Round(totPT / boardPT, 6)):0}  ({Num.F(totPT / boardPT)} exactas)" : "";

        double subtotal = totCost + Num.P(cLabor.Text) + Num.P(cHardware.Text) + Num.P(cFinish.Text) + Num.P(cOther.Text);
        double margin = Num.P(cMargin.Text) / 100;
        double sale = margin < 1 ? subtotal / (1 - margin) : 0;
        cMaterial.Text = Num.F(totCost);
        cSubtotal.Text = Num.F(subtotal);
        cSale.Text = Num.F(sale);
        cProfit.Text = Num.F(sale - subtotal);
        busy = false;
    }

    void ApplyPricePT()
    {
        var v = Num.R(Num.P(bPricePT.Text));
        busy = true;
        foreach (DataGridViewRow r in grid.Rows) if (!r.IsNewRow) r.Cells["Price"].Value = v;
        busy = false;
        Recalc();
    }

    public void ImportPieces(List<WoodPiece> pieces)
    {
        if (pieces.Count == 0) { MessageBox.Show("No hay piezas con medidas para pasar."); return; }
        var replace = MessageBox.Show("¿Reemplazar las piezas actuales del costeo?\n\nSí = reemplazar     No = agregar al final",
            "Pasar piezas", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (replace == DialogResult.Cancel) return;
        busy = true;
        if (replace == DialogResult.Yes) grid.Rows.Clear();
        var price = Num.R(Num.P(bPricePT.Text));
        foreach (var p in pieces)
            grid.Rows.Add("", p.Name, Num.R(p.Qty), Num.R(Math.Round(p.ThickMm, 3)), Num.R(p.WidthMm), Num.R(p.LengthMm), "", "", "", "", "20", "", "", price, "");
        busy = false;
        Recalc();
    }

    public void SetFinishCost(double cost) => cFinish.Text = Num.R(Math.Round(cost, 2));

    public string Summary() => $"PT={txtTotalPT.Text} costo={txtMaterial.Text} ptTabla={bPT.Text} $/PT={bPricePT.Text} venta={cSale.Text} util={cProfit.Text}";

    const string K = "costeo";
    IEnumerable<(string, TextBox)> Fields() => new[]
    {
        ("bT", bT), ("bW", bW), ("bL", bL), ("bPrice", bPrice), ("labor", cLabor), ("hw", cHardware), ("finish", cFinish), ("other", cOther), ("margin", cMargin),
    };

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
        foreach (var tb in new[] { cLabor, cHardware, cFinish, cOther }) tb.Text = "0";
        Recalc();
    }
}
