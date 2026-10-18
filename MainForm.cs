namespace CalculadoraTaller;

class MainForm : Form
{
    readonly TabControl tabs = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), Padding = new Point(14, 6) };
    readonly WoodTab wood = new();
    readonly CosteoTab costeo = new();
    readonly MetalTab metal = new();
    readonly FinishTab breuer = new("breuer", "TABLA DE BARNIZ — BREUER", "barniz_breuer.json", DefaultData.Breuer);
    readonly FinishTab idemo = new("idemo", "TABLA DE BARNIZ — IDEMO  (acabados, impermeabilizante, pintura electrostática, adhesivo)", "barniz_idemo.json", DefaultData.Idemo);
    readonly ToolStripStatusLabel status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    string? projectPath;

    IProjectPart[] Parts => new IProjectPart[] { wood, costeo, metal, breuer, idemo };

    public MainForm()
    {
        Text = BaseTitle;
        Width = 1400; Height = 900; StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 650);
        Font = new Font("Segoe UI", 9.5f);

        AddTab("Pie Tabla (madera)", wood);
        AddTab("Costeo Mueble", costeo);
        AddTab("Metal (m² láminas y perfiles)", metal);
        AddTab("Barniz BREUER", breuer);
        AddTab("Barniz IDEMO / Pintura", idemo);

        // Conexiones entre pestañas
        wood.SendToBreuer = m2 => { breuer.SetArea(m2); Go(breuer, $"Se enviaron {Num.F(m2, 3)} m² de madera a Barniz BREUER."); };
        wood.SendToIdemo = m2 => { idemo.SetArea(m2); Go(idemo, $"Se enviaron {Num.F(m2, 3)} m² de madera a Barniz IDEMO."); };
        wood.SendToCosteo = pcs => { costeo.ImportPieces(pcs); Go(costeo, $"{pcs.Count} pieza(s) pasadas al costeo."); };
        metal.SendToPaint = m2 => { idemo.SetArea(m2, "ELECTROST"); Go(idemo, $"Se enviaron {Num.F(m2, 3)} m² de metal a Pintura Electrostática."); };
        breuer.SendCost = c => { costeo.SetFinishCost(c); Go(costeo, $"Costo de acabado ${Num.F(c)} enviado a Costeo (Acabados y pintura)."); };
        idemo.SendCost = c => { costeo.SetFinishCost(c); Go(costeo, $"Costo de acabado ${Num.F(c)} enviado a Costeo (Acabados y pintura)."); };

        var menu = new MenuStrip();
        var file = new ToolStripMenuItem("&Archivo");
        Item(file, "Nuevo proyecto", () => NewProject(), Keys.Control | Keys.N);
        Item(file, "Abrir proyecto...", () => OpenProject(), Keys.Control | Keys.O);
        Item(file, "Guardar proyecto", () => SaveProject(false), Keys.Control | Keys.S);
        file.DropDownItems.Add("Guardar proyecto como...", null, (s, e) => SaveProject(true));
        file.DropDownItems.Add(new ToolStripSeparator());
        Item(file, "Exportar tabla de esta pestaña a Excel (CSV)...", () => ExportCsv(), Keys.Control | Keys.E);
        file.DropDownItems.Add("Abrir carpeta de datos", null, (s, e) => System.Diagnostics.Process.Start("explorer.exe", Storage.DataDir));
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("Salir", null, (s, e) => Close());
        var help = new ToolStripMenuItem("A&yuda");
        Item(help, "Guía para ingresar los datos", () => ShowGuide(), Keys.F1);
        help.DropDownItems.Add("Cómo se calcula (fórmulas)...", null, (s, e) => ShowHelp());
        help.DropDownItems.Add(new ToolStripSeparator());
        help.DropDownItems.Add("Acerca de...", null, (s, e) => AppInfo.ShowAbout(this));
        menu.Items.AddRange(new ToolStripItem[] { file, help });
        MainMenuStrip = menu;

        var strip = new StatusStrip();
        strip.Items.Add(status);
        status.Text = "Listo. Celdas amarillas = datos que capturas; celdas azules = calculadas. Clic derecho en una tabla para insertar/eliminar filas.";

        Controls.Add(tabs);
        Controls.Add(strip);
        Controls.Add(menu);
    }

    static void Item(ToolStripMenuItem parent, string text, Action click, Keys keys)
    {
        var it = new ToolStripMenuItem(text, null, (s, e) => click()) { ShortcutKeys = keys };
        parent.DropDownItems.Add(it);
    }

    public string SelfTest()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("WOOD " + wood.Summary());
        sb.AppendLine("COSTEO " + costeo.Summary());
        sb.AppendLine("METAL " + metal.Summary());
        metal.SendToPaint!(Num.P("2.5"));
        for (int i = 0; i < breuer.SystemCount; i++) { breuer.Select(i); breuer.SetArea(1); sb.AppendLine("BREUER " + breuer.Summary()); }
        for (int i = 0; i < idemo.SystemCount; i++) { idemo.Select(i); idemo.SetArea(7); sb.AppendLine("IDEMO " + idemo.Summary()); }
        return sb.ToString();
    }

    static string BaseTitle => $"{AppInfo.Product} v{AppInfo.Version} — Pie tabla · Barniz · Metal";

    void AddTab(string title, Control c)
    {
        var page = new TabPage(title) { Font = new Font("Segoe UI", 9.5f) };
        // Barra de ayuda arriba de cada pestaña
        var helpBar = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.FromArgb(255, 244, 214) };
        var btn = new Button
        {
            Text = "?  Ayuda: cómo ingresar los datos (F1)", Dock = DockStyle.Right, Width = 290,
            BackColor = Color.FromArgb(255, 193, 7), FlatStyle = FlatStyle.Flat, Font = UI.Bold, Cursor = Cursors.Hand,
        };
        btn.Click += (s, e) => ShowGuide();
        helpBar.Controls.Add(new Label
        {
            Text = "Amarillo = captura   ·   Azul = calculado   ·   Verde = resultado   ·   Clic derecho en la tabla = insertar / eliminar filas",
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0),
        });
        helpBar.Controls.Add(btn);
        page.Controls.Add(c);
        page.Controls.Add(helpBar);
        tabs.TabPages.Add(page);
    }

    void ShowGuide()
    {
        using var f = new HelpForm(tabs.SelectedIndex);
        f.ShowDialog(this);
    }

    void Go(Control c, string msg)
    {
        tabs.SelectedTab = (TabPage)c.Parent!;
        status.Text = msg;
    }

    void NewProject()
    {
        if (MessageBox.Show("¿Borrar todos los datos capturados y empezar un proyecto nuevo?", "Nuevo proyecto",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        foreach (var p in Parts) p.ClearAll();
        projectPath = null;
        Text = BaseTitle;
    }

    void OpenProject()
    {
        using var dlg = new OpenFileDialog { Filter = "Proyecto de taller (*.taller.json)|*.taller.json|Todos (*.*)|*.*" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var data = Storage.Deserialize<ProjectData>(File.ReadAllText(dlg.FileName)) ?? new ProjectData();
            foreach (var p in Parts) p.LoadFrom(data);
            projectPath = dlg.FileName;
            Text = BaseTitle + "  [" + Path.GetFileName(projectPath) + "]";
            status.Text = "Proyecto abierto: " + projectPath;
        }
        catch (Exception ex) { MessageBox.Show("No se pudo abrir el proyecto: " + ex.Message, "Error"); }
    }

    void SaveProject(bool saveAs)
    {
        if (saveAs || projectPath == null)
        {
            using var dlg = new SaveFileDialog { Filter = "Proyecto de taller (*.taller.json)|*.taller.json", FileName = "proyecto.taller.json" };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            projectPath = dlg.FileName;
        }
        var data = new ProjectData();
        foreach (var p in Parts) p.SaveTo(data);
        File.WriteAllText(projectPath, Storage.Serialize(data));
        Text = BaseTitle + "  [" + Path.GetFileName(projectPath) + "]";
        status.Text = "Proyecto guardado: " + projectPath;
    }

    void ExportCsv()
    {
        var part = Parts[tabs.SelectedIndex];
        using var dlg = new SaveFileDialog { Filter = "CSV para Excel (*.csv)|*.csv", FileName = tabs.SelectedTab!.Text.Trim() + ".csv" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        UI.ExportCsv(part.MainGrid, dlg.FileName);
        status.Text = "Exportado: " + dlg.FileName;
    }

    void ShowHelp() => MessageBox.Show(
        "PIE TABLA (madera)\n" +
        "  PT = ((Largo+sierra)/1000 × (Ancho+sierra)/1000 × Espesor\" × 10.8) × 1.2\n" +
        "  Total PT = PT × Factor × Cantidad\n" +
        "  Tablas necesarias = Total PT ÷ PT por tabla (redondeado hacia arriba)\n" +
        "  PT por tabla = Espesor\" × Ancho\" × Largo(pies) × 12 / 144\n" +
        "  m² a barnizar = 2(L·A + L·E + A·E) × Cantidad (todas las caras)\n\n" +
        "COSTEO MUEBLE\n" +
        "  PT = Espesor\" × Ancho\" × Largo\" / 144;  PT c/merma = PT × (1+%merma)\n" +
        "  Precio de venta = Subtotal / (1 − margen)\n\n" +
        "METAL\n" +
        "  Lámina: Largo × Ancho × Hojas × Caras × %\n" +
        "  Perfil: Perímetro exterior × Largo de tramo × Tramos × %\n" +
        "    Cuadrado 4A · Rectangular 2(A+B) · Redondo πA · Solera/Ángulo 2(A+B)\n\n" +
        "BARNIZ / PINTURA\n" +
        "  Consumo = Cantidad por preparación × Superficie ÷ Rendimiento\n" +
        "  Pintura electrostática: 1 KG por cada 7 m²\n\n" +
        "Los formatos y recetas editados se guardan en la carpeta \"datos\" junto al programa.",
        "Cómo se calcula", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
