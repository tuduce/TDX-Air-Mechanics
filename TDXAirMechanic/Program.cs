using Microsoft.Extensions.DependencyInjection;

namespace TDXAirMechanic
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configure DI
            var services = new ServiceCollection();
            services.AddSingleton<Services.SimConnectService>();
            services.AddSingleton<Services.MechanicService>();
            services.AddSingleton<MainForm>();

            using var serviceProvider = services.BuildServiceProvider();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(serviceProvider.GetRequiredService<MainForm>());
        }
    }
}