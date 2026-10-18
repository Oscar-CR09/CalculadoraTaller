using System.Reflection;

namespace CalculadoraTaller;

/// <summary>
/// Datos del programa (nombre, autor, versión...). NO se editan aquí:
/// se toman de CalculadoraTaller.csproj (Product, Authors, Version, Copyright, Description).
/// </summary>
static class AppInfo
{
    static readonly Assembly Asm = typeof(AppInfo).Assembly;

    public static string Product => Asm.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Calculadora de Taller";
    public static string Author => Asm.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "";
    public static string Copyright => Asm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "";
    public static string Description => Asm.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
    public static string Version
    {
        get
        {
            var v = Asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? Asm.GetName().Version?.ToString() ?? "";
            int plus = v.IndexOf('+');            // quita el sufijo +hash que agrega el compilador
            return plus >= 0 ? v[..plus] : v;
        }
    }
    public static string BuildDate
    {
        get
        {
            try { return File.GetLastWriteTime(Environment.ProcessPath ?? Asm.Location).ToString("dd/MM/yyyy"); }
            catch { return ""; }
        }
    }

    public static void ShowAbout(IWin32Window owner) => MessageBox.Show(owner,
        $"{Product}\nVersión {Version}\n\nAutor: {Author}\n{Copyright}\n\n{Description}\n\nCompilado: {BuildDate}",
        "Acerca de", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
