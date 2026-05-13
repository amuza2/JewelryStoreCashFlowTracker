using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using JewelryStoreCashFlowTracker.ViewModels;
using JewelryStoreCashFlowTracker.Views;
using JewelryStoreCashFlowTracker.Database;
using JewelryStoreCashFlowTracker.Repositories;
using JewelryStoreCashFlowTracker.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Serilog;

namespace JewelryStoreCashFlowTracker;

public partial class App : Application
{
    /// <summary>
    /// Service provider for dependency injection.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ConfigureLogging();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure dependency injection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        // Ensure database and tables are created
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Resolve MainWindowViewModel from DI container
            var mainWindowViewModel = Services.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Configure Serilog logging.
    /// </summary>
    private void ConfigureLogging()
    {
        var logDirectory = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "JewelryStoreCashFlowTracker",
            "logs");

        System.IO.Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                System.IO.Path.Combine(logDirectory, "cashflow-.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Application starting...");
    }

    /// <summary>
    /// Configure dependency injection services.
    /// </summary>
    private void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddDbContext<AppDbContext>();

        // Repositories
        services.AddSingleton<IDaySessionRepository, DaySessionRepository>();
        services.AddSingleton<ITransactionRepository, TransactionRepository>();

        // Services
        services.AddSingleton<IBalanceService, BalanceService>();
        services.AddSingleton<IDaySessionService, DaySessionService>();
        services.AddSingleton<ITransactionService, TransactionService>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<StartDayDialogViewModel>();
        services.AddTransient<AddTransactionDialogViewModel>();
        services.AddTransient<EditTransactionDialogViewModel>();
        services.AddTransient<CloseDayDialogViewModel>();
        services.AddTransient<EditInitialCashDialogViewModel>();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}