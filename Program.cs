using EmployeeSalaryProcessor.Core.Services;
using EmployeeSalaryProcessor.Core.Utilities;
using EmployeeSalaryProcessor.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

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
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        AppLogger.Initialize(loggerFactory);
        AppLogger.LogInformation("Application started");
        Application.Run(mainForm);
    }

    static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.Configure<AppConfig>(context.Configuration);

        services.AddLogging(configure =>
            {
                configure.ClearProviders();
                configure.AddConsole(options =>
                {
                    options.FormatterName = "custom";
                });
                configure.SetMinimumLevel(LogLevel.Debug);
                configure.AddConsoleFormatter<CustomLogFormatter, ConsoleFormatterOptions>();
            });

        services.AddTransient<IXmlProcessor, XmlProcessor>();
        services.AddSingleton<IEmployeeDataService, EmployeeDataService>();

        services.AddTransient<MainForm>();
        services.AddTransient<AddPaymentForm>();
    }
}
//TODO: Says, that better to configure logs in appsettings.json
public class CustomLogFormatter : ConsoleFormatter
{
    public CustomLogFormatter() : base("custom") { }
    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter
        )
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var logLevel = logEntry.LogLevel.ToString()[..4].ToUpper();
        var shortCategory = logEntry.Category.Split('.')[^1];

        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

        textWriter.WriteLine($"{timestamp}|{logLevel}|{shortCategory}|{message}");
    }
}