namespace CalculadoraTaller;

/// <summary>
/// m² de láminas/tableros y perfiles/barras para pintura electrostática
/// (Calculadora_m2_lamina_perfil.xlsx).
///  Lámina:  m² = Largo × Ancho × Hojas × Caras × %
///  Perfil:  m² = Perímetro exterior × Largo de tramo × Tramos × %
/// </summary>
class MetalTab : UserControl, IProjectPart
{
    const string Custom = "(Personalizado)";
    const string FormatsFile = "formatos_metal.json";
    MetalFormats formats;
    readonly DataGridView gSheets = UI.Grid();
    readonly DataGridView gProfiles = UI.Grid();
    readonly DataGridViewComboBoxColumn colSheetFmt, colProfFmt, colProfType;
    readonly TextBox tSheets = UI.Box("", true, 110), tProfiles = UI.Box("", true, 110), tTotal = UI.Box("", true, 110), tMl = UI.Box("", true, 110);
    bool busy;

    public DataGridView MainGrid => gProfiles;
    public Action<double>? SendToPaint;

    public MetalTab()
    {
        formats = Storage.Load(FormatsFile, DefaultData.Metal);
        Dock = DockStyle.Fill;
        Font = new Font("Segoe UI", 9.5f);

        var head = new Label { Text = "METAL — m² de láminas y perfiles para PINTURA ELECTROSTÁTICA", Dock = DockStyle.Top, Font = UI.TitleFont, ForeColor = UI.Accent, Height = 30, Padding = new Padding(8, 6, 0, 0) };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6, 2, 6, 2) };
        bar.Controls.AddRange(new Control[]
        {
            UI.Btn("⚙ Editar formatos (láminas y perfiles)...", (s, e) => EditFormats()),
            UI.Lbl("   Elige un formato en la lista y solo captura las hojas o tramos. Puedes escribir medidas a mano con \"(Personalizado)\"."),
        });

        // ---- Láminas / tableros
        colSheetFmt = UI.Combo(gSheets, "Fmt", "Formato de lámina / tablero", 250, SheetNames());
        UI.Col(gSheets, "Desc", "Descripción / calibre", 150, num: false);
        UI.Col(gSheets, "L", "Largo (m)", 70);
        UI.Col(gSheets, "W", "Ancho (m)", 70);
        UI.Col(gSheets, "Qty", "Hojas", 60);
        UI.Combo(gSheets, "Faces", "Caras a pintar", 70, new[] { "1", "2" });
        UI.Col(gSheets, "Pct", "% considerado", 75);
        UI.Col(gSheets, "M2u", "m² por hoja", 80, ro: true, dec: 3);
        UI.Col(gSheets, "M2", "m² TOTAL", 90, ro: true, dec: 3);
        gSheets.DefaultValuesNeeded += (s, e) =>
        {
            e.Row.Cells["Qty"].Value = "1"; e.Row.Cells["Faces"].Value = "2"; e.Row.Cells["Pct"].Value = "100";
        };
        gSheets.CellValueChanged += (s, e) => OnChanged(gSheets, e, "Fmt");
        gSheets.RowsRemoved += (s, e) => { if (!busy) Recalc(); };

        // ---- Perfiles / barras
        colProfFmt = UI.Combo(gProfiles, "Fmt", "Formato de perfil / barra", 250, ProfileNames());
        colProfType = UI.Combo(gProfiles, "Type", "Tipo", 120, Geometry.ProfileTypes);
        UI.Col(gProfiles, "A", "A: lado / ancho / Ø ext. (mm)", 90);
        UI.Col(gProfiles, "B", "B: alto / espesor (mm)", 80);
        UI.Col(gProfiles, "Len", "Largo del tramo (m)", 75);
        UI.Col(gProfiles, "Qty", "Tramos", 65);
        UI.Col(gProfiles, "Pct", "% considerado", 75);
        UI.Col(gProfiles, "Per", "Perímetro (m)", 80, ro: true, dec: 4);
        UI.Col(gProfiles, "Ml", "Metros lineales", 80, ro: true);
        UI.Col(gProfiles, "M2", "m² TOTAL", 90, ro: true, dec: 3);
        gProfiles.DefaultValuesNeeded += (s, e) =>
        {
            e.Row.Cells["Qty"].Value = "1"; e.Row.Cells["Pct"].Value = "100"; e.Row.Cells["Len"].Value = "6.1";
        };
        gProfiles.CellValueChanged += (s, e) => OnChanged(gProfiles, e, "Fmt");
        gProfiles.RowsRemoved += (s, e) => { if (!busy) Recalc(); };

        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
        split.Panel1.Controls.Add(gSheets);
        split.Panel1.Controls.Add(new Label { Text = "LÁMINAS Y TABLEROS (hojas)", Dock = DockStyle.Top, Font = UI.Bold, Height = 22, Padding = new Padding(4, 3, 0, 0) });
        split.Panel2.Controls.Add(gProfiles);
        split.Panel2.Controls.Add(new Label { Text = "PERFILES Y BARRAS (tramos): cuadrados, rectangulares, redondos huecos, barras, soleras, ángulos  —  se pinta el perímetro exterior", Dock = DockStyle.Top, Font = UI.Bold, Height = 22, Padding = new Padding(4, 3, 0, 0) });
        Load += (s, e) => { try { split.SplitterDistance = (int)(split.Height * 0.42); } catch { } };

        var totals = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6), BackColor = Color.FromArgb(245, 245, 245) };
        totals.Controls.AddRange(new Control[]
        {
            UI.Lbl("m² láminas:", true), tSheets, UI.Lbl("   m² perfiles:", true), tProfiles, UI.Lbl("   Metros lineales:", true), tMl,
            UI.Lbl("   m² TOTALES:", true), tTotal,
            UI.Btn("➜ Enviar m² a Pintura Electrostática", (s, e) => SendToPaint?.Invoke(Num.P(tTotal.Text))),
        });
        tTotal.BackColor = UI.TotalBack;

        Controls.Add(split);
        Controls.Add(totals);
        Controls.Add(bar);
        Controls.Add(head);

        // ejemplo del Excel
        busy = true;
        gSheets.Rows.Add("Lámina 4' x 8' (1.22 x 2.44)", "", "2.44", "1.22", "1", "2", "100", "", "");
        gProfiles.Rows.Add("Rectangular 40 x 20 mm", "Rectangular", "40", "20", "6", "1", "100", "", "", "");
        gProfiles.Rows.Add("Tubo redondo 1 1/2\" (38.1)", "Redondo hueco", "38.1", "", "6", "1", "100", "", "", "");
        busy = false;
        Recalc();
    }

    IEnumerable<string> SheetNames() => new[] { Custom }.Concat(formats.Sheets.Select(s => s.Name));
    IEnumerable<string> ProfileNames() => new[] { Custom }.Concat(formats.Profiles.Select(s => s.Name));

    void OnChanged(DataGridView g, DataGridViewCellEventArgs e, string fmtCol)
    {
        if (busy || e.RowIndex < 0) return;
        if (g.Columns[e.ColumnIndex].Name == fmtCol)
        {
            var name = g.Rows[e.RowIndex].Cells[fmtCol].Value as string;
            busy = true;
            var row = g.Rows[e.RowIndex];
            if (g == gSheets && formats.Sheets.FirstOrDefault(f => f.Name == name) is { } sf)
            {
                row.Cells["L"].Value = Num.R(sf.Length); row.Cells["W"].Value = Num.R(sf.Width);
                if (row.Cells["Desc"].Value is null or "") row.Cells["Desc"].Value = sf.Type;
            }
            else if (g == gProfiles && formats.Profiles.FirstOrDefault(f => f.Name == name) is { } pf)
            {
                row.Cells["Type"].Value = pf.Type; row.Cells["A"].Value = Num.R(pf.A);
                row.Cells["B"].Value = pf.B == 0 ? "" : Num.R(pf.B); row.Cells["Len"].Value = Num.R(pf.Length);
            }
            busy = false;
        }
        Recalc();
    }

    void Recalc()
    {
        if (busy) return;
        busy = true;
        double sumS = 0, sumP = 0, sumMl = 0;
        foreach (DataGridViewRow r in gSheets.Rows)
        {
            if (r.IsNewRow) continue;
            double faces = GridIO.N(r, "Faces"); if (faces == 0) faces = 1;
            double u = GridIO.N(r, "L") * GridIO.N(r, "W");
            double t = u * GridIO.N(r, "Qty") * faces * GridIO.N(r, "Pct") / 100;
            r.Cells["M2u"].Value = Num.F(u, 3);
            r.Cells["M2"].Value = Num.F(t, 3);
            sumS += t;
        }
        foreach (DataGridViewRow r in gProfiles.Rows)
        {
            if (r.IsNewRow) continue;
            var type = GridIO.Str(r, "Type");
            double per = Geometry.Perimeter(type, GridIO.N(r, "A"), GridIO.N(r, "B"));
            double ml = GridIO.N(r, "Len") * GridIO.N(r, "Qty");
            double t = per * ml * GridIO.N(r, "Pct") / 100;
            r.Cells["Per"].Value = Num.F(per, 4);
            r.Cells["Ml"].Value = Num.F(ml, 2);
            r.Cells["M2"].Value = Num.F(t, 3);
            r.Cells["B"].Style.BackColor = Geometry.UsesB(type) ? UI.InputBack : Color.Gainsboro;
            sumP += t; sumMl += ml;
        }
        tSheets.Text = Num.F(sumS, 3);
        tProfiles.Text = Num.F(sumP, 3);
        tMl.Text = Num.F(sumMl, 2);
        tTotal.Text = Num.F(sumS + sumP, 3);
        busy = false;
    }

    void EditFormats()
    {
        using var dlg = new FormatsEditor(formats);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        formats = dlg.Result;
        Storage.Save(FormatsFile, formats);
        UI.SetComboItems(colSheetFmt, SheetNames(), gSheets);
        UI.SetComboItems(colProfFmt, ProfileNames(), gProfiles);
    }

    public string Summary() => $"lam={tSheets.Text} perf={tProfiles.Text} total={tTotal.Text} ml={tMl.Text}";

    const string K = "metal";
    public void SaveTo(ProjectData p)
    {
        p.Grids[K + ".sheets"] = GridIO.Save(gSheets);
        p.Grids[K + ".profiles"] = GridIO.Save(gProfiles);
    }

    public void LoadFrom(ProjectData p)
    {
        busy = true;
        GridIO.Load(gSheets, p.Grids.GetValueOrDefault(K + ".sheets"));
        GridIO.Load(gProfiles, p.Grids.GetValueOrDefault(K + ".profiles"));
        busy = false;
        Recalc();
    }

    public void ClearAll()
    {
        busy = true; gSheets.Rows.Clear(); gProfiles.Rows.Clear(); busy = false;
        Recalc();
    }
}

/// <summary>Editor de los formatos de láminas y perfiles (se guardan en datos\formatos_metal.json).</summary>
class FormatsEditor : Form
{
    readonly DataGridView gS = UI.Grid(), gP = UI.Grid();
    public MetalFormats Result { get; private set; } = new();

    public FormatsEditor(MetalFormats f)
    {
        Text = "Formatos de láminas, tableros, perfiles y barras";
        Width = 1000; Height = 680; StartPosition = FormStartPosition.CenterParent; Font = new Font("Segoe UI", 9.5f);

        UI.Combo(gS, "Type", "Tipo", 110, new[] { "Lámina", "Tablero" });
        UI.Col(gS, "Name", "Nombre del formato", 330, num: false);
        UI.Col(gS, "L", "Largo (m)", 90);
        UI.Col(gS, "W", "Ancho (m)", 90);
        foreach (var s in f.Sheets) gS.Rows.Add(s.Type, s.Name, Num.R(s.Length), Num.R(s.Width));
        gS.DefaultValuesNeeded += (s, e) => e.Row.Cells["Type"].Value = "Lámina";

        UI.Combo(gP, "Type", "Tipo", 130, Geometry.ProfileTypes);
        UI.Col(gP, "Name", "Nombre del formato", 330, num: false);
        UI.Col(gP, "A", "A: lado / ancho / Ø ext. (mm)", 110);
        UI.Col(gP, "B", "B: alto / espesor (mm)", 100);
        UI.Col(gP, "Len", "Largo del tramo (m)", 100);
        foreach (var p in f.Profiles) gP.Rows.Add(p.Type, p.Name, Num.R(p.A), p.B == 0 ? "" : Num.R(p.B), Num.R(p.Length));
        gP.DefaultValuesNeeded += (s, e) => { e.Row.Cells["Type"].Value = "Cuadrado"; e.Row.Cells["Len"].Value = "6.1"; };

        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 230 };
        split.Panel1.Controls.Add(gS);
        split.Panel1.Controls.Add(new Label { Text = "LÁMINAS / TABLEROS", Dock = DockStyle.Top, Font = UI.Bold, Height = 22 });
        split.Panel2.Controls.Add(gP);
        split.Panel2.Controls.Add(new Label { Text = "PERFILES / BARRAS  (Cuadrado y barra cuadrada: solo A. Redondo y barra redonda: A = diámetro exterior)", Dock = DockStyle.Top, Font = UI.Bold, Height = 22 });

        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
        var ok = UI.Btn("Guardar", (s, e) => { Result = Collect(); DialogResult = DialogResult.OK; }, 100);
        var cancel = UI.Btn("Cancelar", (s, e) => DialogResult = DialogResult.Cancel, 100);
        var reset = UI.Btn("Restaurar predeterminados", (s, e) =>
        {
            var d = DefaultData.Metal();
            gS.Rows.Clear(); gP.Rows.Clear();
            foreach (var x in d.Sheets) gS.Rows.Add(x.Type, x.Name, Num.R(x.Length), Num.R(x.Width));
            foreach (var x in d.Profiles) gP.Rows.Add(x.Type, x.Name, Num.R(x.A), x.B == 0 ? "" : Num.R(x.B), Num.R(x.Length));
        });
        bar.Controls.AddRange(new Control[] { cancel, ok, reset });
        Controls.Add(split);
        Controls.Add(bar);
        CancelButton = cancel;
    }

    MetalFormats Collect()
    {
        var m = new MetalFormats();
        foreach (DataGridViewRow r in gS.Rows)
            if (!r.IsNewRow && GridIO.Str(r, "Name").Length > 0)
                m.Sheets.Add(new SheetFormat { Type = GridIO.Str(r, "Type"), Name = GridIO.Str(r, "Name"), Length = GridIO.N(r, "L"), Width = GridIO.N(r, "W") });
        foreach (DataGridViewRow r in gP.Rows)
            if (!r.IsNewRow && GridIO.Str(r, "Name").Length > 0)
                m.Profiles.Add(new ProfileFormat { Type = GridIO.Str(r, "Type"), Name = GridIO.Str(r, "Name"), A = GridIO.N(r, "A"), B = GridIO.N(r, "B"), Length = GridIO.N(r, "Len") });
        return m;
    }
}
