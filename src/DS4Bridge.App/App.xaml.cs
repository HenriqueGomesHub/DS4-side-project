using System.IO;
using System.Windows;
using DS4Bridge.App.Services;
using DS4Bridge.App.Web;
using DS4Bridge.Core.Configuration;
using DS4Bridge.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DS4Bridge.App;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var verbose = e.Args.Contains("--verbose", StringComparer.OrdinalIgnoreCase);
        var (loggerFactory, logDir) = LoggingSetup.Initialize(verbose);
        var startupLogger = loggerFactory.CreateLogger("Startup");
        startupLogger.LogInformation("DS4Bridge starting. Logs at {LogDir}", logDir);

        try
        {
            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(loggerFactory);
                    services.AddSingleton(ConfigStore.Default());
                    services.AddSingleton<ViGEmBusDetector>();
                    services.AddSingleton<SteamConflictDetector>();
                    services.AddSingleton<UiStateBroadcaster>();
                    services.AddSingleton<AppHostService>();
                    services.AddHostedService(sp => sp.GetRequiredService<AppHostService>());
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            var detector = _host.Services.GetRequiredService<ViGEmBusDetector>();
            if (!detector.IsInstalled())
            {
                MessageBox.Show(
                    "DS4Bridge requires the ViGEmBus driver.\n\n" +
                    "Download from:\nhttps://github.com/nefarius/ViGEmBus/releases\n\n" +
                    "After installing, restart DS4Bridge.",
                    "ViGEmBus not installed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown(2);
                return;
            }

            await _host.StartAsync();

            EnsureDesktopShortcut(startupLogger);

            var window = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            startupLogger.LogCritical(ex, "Startup failed");
            MessageBox.Show($"DS4Bridge failed to start:\n\n{ex.Message}", "DS4Bridge",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void EnsureDesktopShortcut(Microsoft.Extensions.Logging.ILogger logger)
    {
        try
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var shortcutPath = Path.Combine(desktop, "DS4Bridge.lnk");
            if (File.Exists(shortcutPath)) return;

            var exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) return;

            // Use COM via dynamic to avoid a hard reference to IWshRuntimeLibrary.
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType is null) return;
            dynamic shell = Activator.CreateInstance(shellType)!;
            var lnk = shell.CreateShortcut(shortcutPath);
            lnk.TargetPath = exePath;
            lnk.WorkingDirectory = Path.GetDirectoryName(exePath);
            lnk.IconLocation = exePath + ",0";
            lnk.Description = "DS4Bridge — DualShock 4 → Xbox 360 controller bridge";
            lnk.Save();
            logger.LogInformation("Created desktop shortcut at {Path}", shortcutPath);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Desktop shortcut creation skipped");
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            try { await _host.StopAsync(TimeSpan.FromSeconds(3)); } catch { /* ignore */ }
            _host.Dispose();
        }
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
