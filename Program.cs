using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmployeeSalaryProcessor;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .Build();
        
        using var scope = host.Services.CreateScope();
        var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();
        
        Application.Run(mainForm);
    }

    static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.Configure<AppConfig>(context.Configuration);
        
        services.AddTransient<XmlProcessor>();
        services.AddTransient<MainForm>();
        services.AddTransient<AddEmployeeForm>();
    }
}