using System;
using System.Windows.Forms;
using VillainLairManager.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace VillainLairManager
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DatabaseHelper.Initialize();
            DatabaseHelper.CreateSchemaIfNotExists();
            DatabaseHelper.SeedInitialData();

            var serviceProvider = ServiceConfigurator.ConfigureServices();
            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }
    }
}
