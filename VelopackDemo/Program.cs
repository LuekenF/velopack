using Velopack;

namespace VelopackDemo;

static class Program
{
    [STAThread]
    static void Main()
    {
        VelopackApp.Build().Run();

        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}
