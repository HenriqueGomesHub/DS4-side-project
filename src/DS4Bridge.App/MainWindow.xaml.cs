using System.Text.Json;
using System.Windows;
using DS4Bridge.App.Services;
using DS4Bridge.App.Web;
using Microsoft.Extensions.Logging;

namespace DS4Bridge.App;

public partial class MainWindow : Window
{
    private readonly WebViewHost _webViewHost;
    private readonly UiStateBroadcaster _broadcaster;
    private readonly AppHostService _appHost;
    private readonly ILogger<MainWindow> _logger;

    public MainWindow(
        UiStateBroadcaster broadcaster,
        AppHostService appHost,
        ILoggerFactory loggerFactory)
    {
        InitializeComponent();
        _broadcaster = broadcaster;
        _appHost = appHost;
        _logger = loggerFactory.CreateLogger<MainWindow>();
        _webViewHost = new WebViewHost(WebView, loggerFactory.CreateLogger<WebViewHost>());
        _webViewHost.MessageReceived += OnWebMessageReceived;
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _webViewHost.InitializeAsync();
            _broadcaster.Attach(_webViewHost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebView2 initialization failed");
            MessageBox.Show(
                $"Failed to initialize the UI:\n\n{ex.Message}\n\nMake sure the Microsoft Edge WebView2 Runtime is installed.",
                "DS4Bridge",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    private void OnWebMessageReceived(object? sender, JsonElement message)
    {
        if (!message.TryGetProperty("cmd", out var cmdEl) || cmdEl.ValueKind != JsonValueKind.String) return;
        var cmd = cmdEl.GetString();
        try
        {
            switch (cmd)
            {
                case "exit":
                    Dispatcher.BeginInvoke(() => Application.Current.Shutdown());
                    break;
                case "restart":
                    _appHost.RequestRestart();
                    break;
                case "setProfile":
                    if (message.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == JsonValueKind.String)
                        _appHost.SetActiveProfile(nameEl.GetString()!);
                    break;
                case "minimize":
                    Dispatcher.BeginInvoke(() => WindowState = WindowState.Minimized);
                    break;
                default:
                    _logger.LogDebug("Unknown web command: {Cmd}", cmd);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to handle web command {Cmd}", cmd);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _broadcaster.Detach();
        _webViewHost.Dispose();
    }
}
