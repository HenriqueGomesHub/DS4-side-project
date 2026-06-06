using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace DS4Bridge.App.Web;

// Owns the WebView2 lifecycle: extracts the embedded React build to a temp
// folder, maps it via virtual host, exposes a typed message bus.
public sealed class WebViewHost : IDisposable
{
    private readonly WebView2 _webView;
    private readonly ILogger<WebViewHost> _logger;
    private readonly Dispatcher _dispatcher;
    private readonly string _userDataFolder;
    private readonly string _webRoot;
    private bool _initialized;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public event EventHandler<JsonElement>? MessageReceived;

    public WebViewHost(WebView2 webView, ILogger<WebViewHost> logger)
    {
        _webView = webView;
        _logger = logger;
        _dispatcher = webView.Dispatcher;
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DS4Bridge");
        _userDataFolder = Path.Combine(baseDir, "WebView2");
        _webRoot = Path.Combine(baseDir, "web");
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        ExtractWebAssets();
        Directory.CreateDirectory(_userDataFolder);

        var env = await CoreWebView2Environment.CreateAsync(
            userDataFolder: _userDataFolder);
        await _webView.EnsureCoreWebView2Async(env);

        var core = _webView.CoreWebView2;
        core.SetVirtualHostNameToFolderMapping(
            "ds4bridge.local", _webRoot, CoreWebView2HostResourceAccessKind.Allow);

        core.Settings.AreDevToolsEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.AreDefaultContextMenusEnabled = false;
        core.Settings.IsZoomControlEnabled = false;
        core.Settings.AreBrowserAcceleratorKeysEnabled = false;

        core.WebMessageReceived += OnWebMessageReceived;

        _webView.Source = new Uri("https://ds4bridge.local/index.html");
        _initialized = true;
        _logger.LogInformation("WebView2 initialized; assets extracted to {WebRoot}", _webRoot);
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var doc = JsonDocument.Parse(e.WebMessageAsJson);
            MessageReceived?.Invoke(this, doc.RootElement.Clone());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse web message: {Raw}", e.WebMessageAsJson);
        }
    }

    public void Post(object message)
    {
        if (!_initialized) return;
        var json = JsonSerializer.Serialize(message, JsonOptions);
        if (!_dispatcher.CheckAccess())
        {
            _dispatcher.BeginInvoke(() => PostInternal(json));
            return;
        }
        PostInternal(json);
    }

    private void PostInternal(string json)
    {
        try { _webView.CoreWebView2?.PostWebMessageAsJson(json); }
        catch (Exception ex) { _logger.LogDebug(ex, "Post message failed (webview likely closed)"); }
    }

    private void ExtractWebAssets()
    {
        var asm = Assembly.GetExecutingAssembly();
        var resourceNames = asm.GetManifestResourceNames()
            .Where(n => n.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (resourceNames.Count == 0)
        {
            _logger.LogWarning("No embedded web assets found. UI will not load.");
            return;
        }

        if (Directory.Exists(_webRoot))
        {
            try { Directory.Delete(_webRoot, recursive: true); } catch { /* best effort */ }
        }
        Directory.CreateDirectory(_webRoot);

        foreach (var name in resourceNames)
        {
            var relative = name["wwwroot/".Length..].Replace('/', Path.DirectorySeparatorChar);
            var target = Path.Combine(_webRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            using var src = asm.GetManifestResourceStream(name);
            if (src is null) continue;
            using var dst = File.Create(target);
            src.CopyTo(dst);
        }
        _logger.LogInformation("Extracted {Count} web assets", resourceNames.Count);
    }

    public void Dispose()
    {
        if (_webView.CoreWebView2 is not null)
            _webView.CoreWebView2.WebMessageReceived -= OnWebMessageReceived;
    }
}
