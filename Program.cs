namespace CalculadoraTaller;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Length > 0 && args[0] == "--selftest")
        {
            using var f = new MainForm();
            File.WriteAllText(args[1], f.SelfTest());
            return;
        }
        Application.Run(new MainForm());
    }
}
