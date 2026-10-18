namespace CalculadoraTaller;

/// <summary>
/// Pestaña de tabla de barniz / pintura (usada para BREUER e IDEMO).
/// Consumo = Cantidad por preparación × Superficie / Rendimiento.
/// </summary>
class FinishTab : UserControl, IProjectPart
{
    readonly string key, file;
    readonly Func<List<FinishSystem>> defaults;
    List<FinishSystem> systems;
    FinishSystem? current;
    bool busy;

    readonly ComboBox cboSystem = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 440, Font = new Font("Segoe UI", 10f) };
    readonly TextBox txtArea = UI.Box("1", width: 90);
    readonly Label lblUnit = UI.Lbl("m²", true);
    readonly Label lblNote = new() { AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(8, 4, 8, 6), ForeColor = Color.FromArgb(120, 60, 0), MaximumSize = new Size(1300, 0) };
    readonly DataGridView grid = UI.Grid();
    readonly DataGridView gridStage = UI.Grid(false);
    readonly DataGridView gridProduct = UI.Grid(false);
    readonly Label lblTotal = new() { AutoSize = true, Font = UI.TitleFont, ForeColor = UI.Accent, Padding = new Padding(8) };

    public DataGridView MainGrid => grid;
    /// <summary>Envía el costo total del acabado a la pestaña de costeo.</summary>
    public Action<double>? SendCost;

    public FinishTab(string key, string title, string file, Func<List<FinishSystem>> defaults)
    {
        this.key = key; this.file = file; this.defaults = defaults;
        systems = Storage.Load(file, defaults);
        Dock = DockStyle.Fill;
        Font = new Font("Segoe UI", 9.5f);

        var head = new Label { Text = title, Dock = DockStyle.Top, Font = UI.TitleFont, ForeColor = UI.Accent, Height = 30, Padding = new Padding(8, 6, 0, 0) };

        var bar1 = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6, 2, 6, 2) };
        bar1.Controls.AddRange(new Control[]
        {
            UI.Lbl("Sistema:", true), cboSystem, UI.Lbl("   Superficie a cubrir:", true), txtArea, lblUnit,
        });

        var bar2 = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6, 0, 6, 2) };
        bar2.Controls.AddRange(new Control[]
        {
            UI.Btn("Nuevo sistema", (s, e) => NewSystem()),
            UI.Btn("Duplicar", (s, e) => DuplicateSystem()),
            UI.Btn("Renombrar / Nota", (s, e) => RenameSystem()),
            UI.Btn("Eliminar sistema", (s, e) => DeleteSystem()),
            UI.Btn("Guardar recetas", (s, e) => SaveRecipes()),
            UI.Btn("Restaurar originales", (s, e) => RestoreDefaults()),
            UI.Btn("➜ Enviar costo a Costeo", (s, e) => SendCost?.Invoke(TotalCost)),
        });

        UI.Col(grid, "Stage", "Etapa", 170, num: false);
        UI.Col(grid, "Code", "Código", 130, num: false);
        UI.Col(grid, "Name", "Producto", 340, num: false);
        UI.Col(grid, "Qty", "Cantidad por preparación", 95);
        UI.Col(grid, "Unit", "Unidad", 60, num: false);
        UI.Col(grid, "Yield", "Rendimiento (m² por preparación)", 115);
        UI.Col(grid, "App", "Aplicación", 200, num: false);
        UI.Col(grid, "Cons", "CONSUMO", 100, ro: true);
        UI.Col(grid, "Price", "Precio $ por unidad", 90);
        UI.Col(grid, "Cost", "Costo $", 100, ro: true);
        grid.CellValueChanged += (s, e) => { if (!busy) { SyncFromGrid(); Recalc(); } };
        grid.RowsRemoved += (s, e) => { if (!busy) { SyncFromGrid(); Recalc(); } };
        grid.DefaultValuesNeeded += (s, e) =>
        {
            var prev = e.Row.Index > 0 ? grid.Rows[e.Row.Index - 1] : null;
            e.Row.Cells["Stage"].Value = prev?.Cells["Stage"].Value ?? "";
            e.Row.Cells["Unit"].Value = "LT";
            e.Row.Cells["Yield"].Value = prev?.Cells["Yield"].Value ?? "1";
        };

        UI.Col(gridStage, "Stage", "Etapa", 200, ro: true, num: false);
        UI.Col(gridStage, "Prep", "Total preparación", 110, ro: true);
        UI.Col(gridStage, "Cons", "Consumo de la etapa", 120, ro: true);
        UI.Col(gridProduct, "Name", "Producto (total a surtir)", 330, ro: true, num: false);
        UI.Col(gridProduct, "Unit", "Unidad", 60, ro: true, num: false);
        UI.Col(gridProduct, "Cons", "Consumo total", 110, ro: true);
        UI.Col(gridProduct, "Cost", "Costo $", 100, ro: true);
        foreach (var g in new[] { gridStage, gridProduct }) { g.ReadOnly = true; g.RowHeadersVisible = false; }

        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        bottom.Controls.Add(Wrap("Resumen por etapa", gridStage), 0, 0);
        bottom.Controls.Add(Wrap("Total por producto", gridProduct), 1, 0);

        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
        split.Panel1.Controls.Add(grid);
        split.Panel2.Controls.Add(bottom);
        split.Panel2.Controls.Add(lblTotal);
        lblTotal.Dock = DockStyle.Bottom;

        Controls.Add(split);
        Controls.Add(lblNote);
        Controls.Add(bar2);
        Controls.Add(bar1);
        Controls.Add(head);
        Load += (s, e) => { try { split.SplitterDistance = (int)(split.Height * 0.55); } catch { } };

        cboSystem.SelectedIndexChanged += (s, e) => ShowSystem(cboSystem.SelectedIndex);
        txtArea.TextChanged += (s, e) => Recalc();
        RefreshCombo(0);
    }

    static Control Wrap(string title, Control c)
    {
        var p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(2) };
        p.Controls.Add(c);
        p.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Font = UI.Bold, Height = 22 });
        return p;
    }

    double TotalCost { get; set; }

    void RefreshCombo(int select)
    {
        cboSystem.Items.Clear();
        foreach (var s in systems) cboSystem.Items.Add(s.Name);
        if (systems.Count > 0) cboSystem.SelectedIndex = Math.Clamp(select, 0, systems.Count - 1);
        else ShowSystem(-1);
    }

    void ShowSystem(int i)
    {
        current = i >= 0 && i < systems.Count ? systems[i] : null;
        busy = true;
        grid.Rows.Clear();
        if (current != null)
        {
            foreach (var it in current.Items)
                grid.Rows.Add(it.Stage, it.Code, it.Name, Num.R(it.Qty), it.Unit, Num.R(it.Yield), it.Application, "", it.Price == 0 ? "" : Num.R(it.Price), "");
            lblNote.Text = string.IsNullOrWhiteSpace(current.Note) ? "" : "NOTA: " + current.Note;
            lblUnit.Text = current.InputUnit;
            grid.Columns["Yield"]!.HeaderText = $"Rendimiento ({current.InputUnit} por preparación)";
        }
        busy = false;
        Recalc();
    }

    void SyncFromGrid()
    {
        if (current == null) return;
        current.Items = grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow)
            .Select(r => new FinishItem
            {
                Stage = GridIO.Str(r, "Stage"), Code = GridIO.Str(r, "Code"), Name = GridIO.Str(r, "Name"),
                Qty = GridIO.N(r, "Qty"), Unit = GridIO.Str(r, "Unit"), Yield = GridIO.N(r, "Yield"),
                Application = GridIO.Str(r, "App"), Price = GridIO.N(r, "Price"),
            }).ToList();
    }

    void Recalc()
    {
        if (busy) return;
        busy = true;
        double area = Num.P(txtArea.Text);
        var stages = new List<(string stage, double prep, double cons, string unit)>();
        var prods = new List<(string name, string unit, double cons, double cost)>();
        double total = 0;
        foreach (DataGridViewRow r in grid.Rows)
        {
            if (r.IsNewRow) continue;
            double qty = GridIO.N(r, "Qty"), yld = GridIO.N(r, "Yield"), price = GridIO.N(r, "Price");
            double cons = yld > 0 ? qty * area / yld : 0;
            double cost = cons * price;
            r.Cells["Cons"].Value = Num.F(cons, 3);
            r.Cells["Cost"].Value = price > 0 ? Num.F(cost) : "";
            total += cost;

            string st = GridIO.Str(r, "Stage"), unit = GridIO.Str(r, "Unit");
            int si = stages.FindIndex(x => x.stage == st);
            if (si < 0) stages.Add((st, qty, cons, unit));
            else stages[si] = (st, stages[si].prep + qty, stages[si].cons + cons, stages[si].unit == unit ? unit : "");

            string name = (GridIO.Str(r, "Code") is { Length: > 0 } c && c != "**" ? c + "  " : "") + GridIO.Str(r, "Name");
            int pi = prods.FindIndex(x => x.name == name && x.unit == unit);
            if (pi < 0) prods.Add((name, unit, cons, cost));
            else prods[pi] = (name, unit, prods[pi].cons + cons, prods[pi].cost + cost);
        }
        gridStage.Rows.Clear();
        foreach (var s in stages)
            gridStage.Rows.Add(s.stage, Num.F(s.prep, 3) + " " + s.unit, Num.F(s.cons, 3) + " " + s.unit);
        gridProduct.Rows.Clear();
        foreach (var p in prods)
            gridProduct.Rows.Add(p.name, p.unit, Num.F(p.cons, 3), p.cost > 0 ? Num.F(p.cost) : "");

        var byUnit = prods.GroupBy(p => p.unit).Select(g => $"{Num.F(g.Sum(x => x.cons), 3)} {g.Key}");
        TotalCost = total;
        lblTotal.Text = $"Superficie: {Num.F(area)} {current?.InputUnit}     Consumo total: {string.Join(" + ", byUnit)}" +
                        (total > 0 ? $"     COSTO TOTAL: ${Num.F(total)}" : "");
        busy = false;
    }

    /// <summary>Pone la superficie (m²) y opcionalmente selecciona un sistema cuyo nombre contenga el texto.</summary>
    public void SetArea(double value, string? systemContains = null)
    {
        txtArea.Text = Num.R(Math.Round(value, 4));
        if (systemContains != null)
        {
            int i = systems.FindIndex(s => s.Name.Contains(systemContains, StringComparison.OrdinalIgnoreCase));
            if (i >= 0) cboSystem.SelectedIndex = i;
        }
    }

    void NewSystem()
    {
        var name = Prompt.Ask("Nombre del nuevo sistema:", "Nuevo sistema", "NUEVO SISTEMA");
        if (string.IsNullOrWhiteSpace(name)) return;
        systems.Add(new FinishSystem { Name = name.Trim().ToUpperInvariant() });
        RefreshCombo(systems.Count - 1);
    }

    void DuplicateSystem()
    {
        if (current == null) return;
        var copy = Storage.Deserialize<FinishSystem>(Storage.Serialize(current))!;
        copy.Name += " (COPIA)";
        systems.Insert(cboSystem.SelectedIndex + 1, copy);
        RefreshCombo(cboSystem.SelectedIndex + 1);
    }

    void RenameSystem()
    {
        if (current == null) return;
        var name = Prompt.Ask("Nombre del sistema:", "Renombrar", current.Name);
        if (name == null) return;
        var note = Prompt.Ask("Nota (opcional):", "Nota", current.Note, multiline: true);
        var unit = Prompt.Ask("Unidad de superficie (m², m lineales, piezas...):", "Unidad", current.InputUnit);
        current.Name = name.Trim();
        if (note != null) current.Note = note.Trim();
        if (!string.IsNullOrWhiteSpace(unit)) current.InputUnit = unit.Trim();
        RefreshCombo(cboSystem.SelectedIndex);
    }

    void DeleteSystem()
    {
        if (current == null) return;
        if (MessageBox.Show($"¿Eliminar el sistema \"{current.Name}\"?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        int i = cboSystem.SelectedIndex;
        systems.Remove(current);
        RefreshCombo(i - 1);
    }

    void SaveRecipes()
    {
        SyncFromGrid();
        Storage.Save(file, systems);
        MessageBox.Show($"Recetas guardadas en:\n{Path.Combine(Storage.DataDir, file)}", "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    void RestoreDefaults()
    {
        if (MessageBox.Show("¿Restaurar todas las recetas a los valores originales del Excel? Se perderán los cambios no guardados.",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        systems = defaults();
        RefreshCombo(0);
    }

    public string Summary() => $"[{current?.Name}] " + lblTotal.Text + " | etapas: " + string.Join("; ", gridStage.Rows.Cast<DataGridViewRow>().Select(r => r.Cells[0].Value + "=" + r.Cells[2].Value));
    public int SystemCount => systems.Count;
    public void Select(int i) => cboSystem.SelectedIndex = i;

    public void SaveTo(ProjectData p)
    {
        p.Fields[key + ".area"] = txtArea.Text;
        p.Fields[key + ".system"] = current?.Name ?? "";
    }

    public void LoadFrom(ProjectData p)
    {
        if (p.Fields.TryGetValue(key + ".system", out var sys))
        {
            int i = systems.FindIndex(s => s.Name == sys);
            if (i >= 0) cboSystem.SelectedIndex = i;
        }
        if (p.Fields.TryGetValue(key + ".area", out var a)) txtArea.Text = a;
    }

    public void ClearAll() => txtArea.Text = "1";
}

static class Prompt
{
    public static string? Ask(string text, string caption, string value = "", bool multiline = false)
    {
        using var f = new Form
        {
            Text = caption, Width = 520, Height = multiline ? 260 : 160, FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent, MaximizeBox = false, MinimizeBox = false, Font = new Font("Segoe UI", 9.5f),
        };
        var lbl = new Label { Text = text, Left = 10, Top = 10, AutoSize = true };
        var tb = new TextBox { Text = value, Left = 10, Top = 34, Width = 480, Multiline = multiline, Height = multiline ? 120 : 24, ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None };
        var ok = new Button { Text = "Aceptar", DialogResult = DialogResult.OK, Left = 310, Width = 85, Top = tb.Bottom + 12 };
        var cancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Left = 405, Width = 85, Top = tb.Bottom + 12 };
        f.Controls.AddRange(new Control[] { lbl, tb, ok, cancel });
        if (!multiline) f.AcceptButton = ok;
        f.CancelButton = cancel;
        return f.ShowDialog() == DialogResult.OK ? tb.Text : null;
    }
}
