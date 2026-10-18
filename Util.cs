using System.Globalization;
using System.Text;
using System.Text.Json;

namespace CalculadoraTaller;

/// <summary>Lectura y formato de números tolerante a coma o punto decimal.</summary>
static class Num
{
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static double P(object? v)
    {
        if (v == null) return 0;
        if (v is double d0) return d0;
        var s = v.ToString()!.Trim().Replace(" ", "").Replace("$", "").Replace("%", "");
        if (s.Length == 0) return 0;
        if (s.Contains(',') && s.Contains('.')) s = s.Replace(",", "");
        else s = s.Replace(',', '.');
        return double.TryParse(s, NumberStyles.Float, Inv, out var d) ? d : 0;
    }

    public static string F(double d, int dec = 2) => d.ToString("#,##0." + new string('0', dec), Inv);
    public static string R(double d) => d.ToString("0.#####", Inv);
}

/// <summary>Datos guardados de un proyecto (todas las pestañas).</summary>
class ProjectData
{
    public Dictionary<string, List<Dictionary<string, string>>> Grids { get; set; } = new();
    public Dictionary<string, string> Fields { get; set; } = new();
}

interface IProjectPart
{
    DataGridView MainGrid { get; }
    void SaveTo(ProjectData p);
    void LoadFrom(ProjectData p);
    void ClearAll();
}

static class Storage
{
    public static string DataDir
    {
        get
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "datos");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };

    public static T Load<T>(string file, Func<T> fallback)
    {
        try
        {
            var path = Path.Combine(DataDir, file);
            if (File.Exists(path))
            {
                var v = JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), Opts);
                if (v != null) return v;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo leer {file}: {ex.Message}\nSe usarán los valores originales.", "Aviso");
        }
        return fallback();
    }

    public static void Save<T>(string file, T value) =>
        File.WriteAllText(Path.Combine(DataDir, file), JsonSerializer.Serialize(value, Opts), Encoding.UTF8);

    public static string Serialize<T>(T v) => JsonSerializer.Serialize(v, Opts);
    public static T? Deserialize<T>(string s) => JsonSerializer.Deserialize<T>(s, Opts);
}

static class UI
{
    public static readonly Color InputBack = Color.FromArgb(255, 251, 214);   // amarillo como en Excel
    public static readonly Color CalcBack = Color.FromArgb(232, 241, 251);
    public static readonly Color TotalBack = Color.FromArgb(214, 234, 214);
    public static readonly Color Accent = Color.FromArgb(31, 78, 121);
    public static readonly Font Bold = new("Segoe UI", 9.5f, FontStyle.Bold);
    public static readonly Font TitleFont = new("Segoe UI", 12f, FontStyle.Bold);

    public static DataGridView Grid(bool allowAdd = true)
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = allowAdd,
            AllowUserToDeleteRows = allowAdd,
            AllowUserToResizeRows = false,
            RowHeadersWidth = 28,
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            EnableHeadersVisualStyles = false,
        };
        g.ColumnHeadersDefaultCellStyle.BackColor = Accent;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.Font = Bold;
        g.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
        g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 200, 120);
        g.DefaultCellStyle.SelectionForeColor = Color.Black;
        g.DataError += (s, e) => e.ThrowException = false;
        // Confirmar al instante casillas y listas desplegables
        g.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (g.IsCurrentCellDirty && g.CurrentCell is DataGridViewCheckBoxCell or DataGridViewComboBoxCell)
                g.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        if (allowAdd)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Insertar fila arriba", null, (s, e) =>
            {
                int i = g.CurrentCell?.RowIndex ?? -1;
                if (i >= 0) g.Rows.Insert(i, 1);
            });
            menu.Items.Add("Eliminar fila(s) seleccionada(s)", null, (s, e) => DeleteSelectedRows(g));
            g.ContextMenuStrip = menu;
            g.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete && !g.IsCurrentCellInEditMode)
                {
                    // Supr borra el contenido de las celdas editables seleccionadas
                    foreach (DataGridViewCell c in g.SelectedCells)
                        if (!c.ReadOnly && c.OwningRow is { IsNewRow: false } && c is DataGridViewTextBoxCell) c.Value = null;
                    e.Handled = true;
                }
            };
        }
        return g;
    }

    public static void DeleteSelectedRows(DataGridView g)
    {
        var rows = g.SelectedCells.Cast<DataGridViewCell>().Select(c => c.OwningRow).OfType<DataGridViewRow>()
            .Where(r => !r.IsNewRow).Distinct().OrderByDescending(r => r.Index).ToList();
        foreach (var r in rows) g.Rows.Remove(r);
    }

    public static DataGridViewTextBoxColumn Col(DataGridView g, string name, string header, int width,
        bool ro = false, bool num = true, int dec = 2)
    {
        var c = new DataGridViewTextBoxColumn
        {
            Name = name, HeaderText = header, Width = width, ReadOnly = ro,
            SortMode = DataGridViewColumnSortMode.NotSortable,
        };
        c.DefaultCellStyle.BackColor = ro ? CalcBack : InputBack;
        if (num) c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        if (ro) c.DefaultCellStyle.Font = Bold;
        g.Columns.Add(c);
        return c;
    }

    public static DataGridViewCheckBoxColumn Check(DataGridView g, string name, string header, int width)
    {
        var c = new DataGridViewCheckBoxColumn { Name = name, HeaderText = header, Width = width };
        c.DefaultCellStyle.BackColor = InputBack;
        g.Columns.Add(c);
        return c;
    }

    public static DataGridViewComboBoxColumn Combo(DataGridView g, string name, string header, int width, IEnumerable<string> items)
    {
        var c = new DataGridViewComboBoxColumn
        {
            Name = name, HeaderText = header, Width = width,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton, FlatStyle = FlatStyle.Flat,
        };
        c.Items.AddRange(items.Cast<object>().ToArray());
        c.DefaultCellStyle.BackColor = InputBack;
        g.Columns.Add(c);
        return c;
    }

    public static void SetComboItems(DataGridViewComboBoxColumn c, IEnumerable<string> items, DataGridView g)
    {
        var all = items.ToList();
        // conservar los valores ya usados aunque ya no existan en la lista
        foreach (DataGridViewRow r in g.Rows)
            if (r.Cells[c.Name].Value is string s && s.Length > 0 && !all.Contains(s)) all.Add(s);
        c.Items.Clear();
        c.Items.AddRange(all.Cast<object>().ToArray());
    }

    public static TextBox Box(string value = "", bool ro = false, int width = 90)
    {
        return new TextBox
        {
            Text = value, ReadOnly = ro, Width = width, TextAlign = HorizontalAlignment.Right,
            BackColor = ro ? CalcBack : InputBack, Font = ro ? Bold : new Font("Segoe UI", 9.5f),
        };
    }

    public static Label Lbl(string text, bool bold = false) => new()
    {
        Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Font = bold ? Bold : new Font("Segoe UI", 9.5f),
        Padding = new Padding(0, 6, 0, 0),
    };

    public static Button Btn(string text, EventHandler click, int width = 0)
    {
        var b = new Button { Text = text, AutoSize = width == 0, Height = 30, Padding = new Padding(6, 0, 6, 0) };
        if (width > 0) b.Width = width;
        b.Click += click;
        return b;
    }

    /// <summary>Tabla de dos columnas: etiqueta | caja de texto.</summary>
    public static TableLayoutPanel Form(params (string label, Control ctl)[] rows)
    {
        var t = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(4) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        foreach (var (label, ctl) in rows)
        {
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.Controls.Add(Lbl(label));
            t.Controls.Add(ctl);
        }
        return t;
    }

    public static GroupBox Group(string title, Control content, int width)
    {
        var g = new GroupBox { Text = title, Width = width, Dock = DockStyle.Left, Font = Bold, Padding = new Padding(6) };
        content.Font = new Font("Segoe UI", 9.5f);
        g.Controls.Add(content);
        return g;
    }

    public static void ExportCsv(DataGridView g, string path)
    {
        var sb = new StringBuilder();
        var cols = g.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).OrderBy(c => c.DisplayIndex).ToList();
        sb.AppendLine(string.Join(",", cols.Select(c => Q(c.HeaderText.Replace("\n", " ")))));
        foreach (DataGridViewRow r in g.Rows)
        {
            if (r.IsNewRow) continue;
            sb.AppendLine(string.Join(",", cols.Select(c => Q(r.Cells[c.Index].FormattedValue?.ToString() ?? ""))));
        }
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        static string Q(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
    }
}

/// <summary>Guarda/carga el contenido editable de una tabla por nombre de columna.</summary>
static class GridIO
{
    public static List<Dictionary<string, string>> Save(DataGridView g)
    {
        var list = new List<Dictionary<string, string>>();
        foreach (DataGridViewRow r in g.Rows)
        {
            if (r.IsNewRow) continue;
            var d = new Dictionary<string, string>();
            foreach (DataGridViewColumn c in g.Columns)
            {
                if (c.ReadOnly) continue;
                var v = r.Cells[c.Index].Value;
                d[c.Name] = v is bool b ? (b ? "1" : "0") : v?.ToString() ?? "";
            }
            list.Add(d);
        }
        return list;
    }

    public static void Load(DataGridView g, List<Dictionary<string, string>>? rows)
    {
        g.Rows.Clear();
        if (rows == null) return;
        foreach (var d in rows)
        {
            int i = g.Rows.Add();
            foreach (var (k, v) in d)
            {
                if (!g.Columns.Contains(k)) continue;
                var col = g.Columns[k];
                if (col is DataGridViewCheckBoxColumn) g.Rows[i].Cells[k].Value = v == "1";
                else
                {
                    if (col is DataGridViewComboBoxColumn cb && v.Length > 0 && !cb.Items.Contains(v)) cb.Items.Add(v);
                    g.Rows[i].Cells[k].Value = v;
                }
            }
        }
    }

    public static string Str(DataGridViewRow r, string col) => r.Cells[col].Value?.ToString()?.Trim() ?? "";
    public static double N(DataGridViewRow r, string col) => Num.P(r.Cells[col].Value);
    public static bool B(DataGridViewRow r, string col) => r.Cells[col].Value is true;
}
