using Microsoft.Extensions.DependencyInjection;
using VillainLairManager.Forms;
using VillainLairManager.Repositories;
using VillainLairManager.Services;

namespace VillainLairManager
{
    public static class ServiceConfigurator
    {
        public static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<RepositoryFactory>(sp =>
                new RepositoryFactory(AppSettings.Instance.DatabasePath));

            services.AddSingleton<IMinionRepository>(sp =>
                sp.GetRequiredService<RepositoryFactory>().Minions);
            services.AddSingleton<ISchemeRepository>(sp =>
                sp.GetRequiredService<RepositoryFactory>().Schemes);
            services.AddSingleton<IEquipmentRepository>(sp =>
                sp.GetRequiredService<RepositoryFactory>().Equipment);
            services.AddSingleton<ISecretBaseRepository>(sp =>
                sp.GetRequiredService<RepositoryFactory>().Bases);

            services.AddSingleton<MinionService>();
            services.AddSingleton<SchemeService>();
            services.AddSingleton<EquipmentService>();
            services.AddSingleton<BaseService>();
            services.AddSingleton<DashboardService>();

            services.AddTransient<MainForm>();
            services.AddTransient<MinionManagementForm>();
            services.AddTransient<SchemeManagementForm>();
            services.AddTransient<EquipmentInventoryForm>();
            services.AddTransient<BaseManagementForm>();

            var serviceProvider = services.BuildServiceProvider();

            services.AddSingleton<IServiceProvider>(serviceProvider);

            return serviceProvider;
        }
    }
}
