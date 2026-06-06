using DS4Bridge.App.Web;
using DS4Bridge.Core.Bridge;
using DS4Bridge.Core.Configuration;
using DS4Bridge.Core.Mapping;
using DS4Bridge.Core.Models;
using DS4Bridge.Hid;
using DS4Bridge.Virtual;
using HidSharp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client;

namespace DS4Bridge.App.Services;

public sealed class AppHostService : IHostedService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<AppHostService> _logger;
    private readonly ConfigStore _configStore;
    private readonly SteamConflictDetector _steamDetector;
    private readonly UiStateBroadcaster _ui;
    private readonly Dictionary<string, BridgeEntry> _bridges = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _gate = new();
    private DeviceWatcher? _watcher;
    private ViGEmClient? _client;
    private CancellationTokenSource? _cts;

    public AppHostService(
        ILoggerFactory loggerFactory,
        ConfigStore store,
        SteamConflictDetector steamDetector,
        UiStateBroadcaster uiBroadcaster)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<AppHostService>();
        _configStore = store;
        _steamDetector = steamDetector;
        _ui = uiBroadcaster;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _client = OrphanCleanup.ConstructClient(_logger);
        _watcher = new DeviceWatcher(_loggerFactory.CreateLogger<DeviceWatcher>());
        _watcher.Connected += OnConnected;
        _watcher.Disconnected += OnDisconnected;
        _watcher.Start();
        BroadcastProfiles();
        _ui.PushDisconnected();
        _logger.LogInformation("AppHostService started");
        return Task.CompletedTask;
    }

    public void RequestRestart()
    {
        lock (_gate)
        {
            foreach (var entry in _bridges.Values)
            {
                try { entry.Bridge.Dispose(); } catch { /* ignore */ }
            }
            _bridges.Clear();
        }
        _ui.PushDisconnected();
        _watcher?.ForceRescan();
    }

    public void SetActiveProfile(string name)
    {
        var cfg = _configStore.Load();
        if (!cfg.Profiles.ContainsKey(name)) return;
        _configStore.Save(cfg with { ActiveProfile = name });
        var newProfile = cfg.Profiles[name];
        lock (_gate)
        {
            foreach (var entry in _bridges.Values)
                entry.Bridge.UpdateProfile(newProfile);
        }
        BroadcastProfiles();
    }

    private void OnConnected(object? sender, HidDevice device)
    {
        lock (_gate)
        {
            var devicePath = device.DevicePath ?? string.Empty;
            var isBt = devicePath.Contains("BTHENUM", StringComparison.OrdinalIgnoreCase);
            if (isBt && _bridges.Values.Any(e => e.Bridge.Mode == ConnectionMode.Usb))
            {
                _logger.LogInformation("Ignoring BT DS4 because USB DS4 is already active");
                return;
            }
            if (_bridges.Count > 0)
            {
                _logger.LogWarning("Additional DS4 detected but v1 supports only one; ignoring {Path}", devicePath);
                return;
            }

            try
            {
                var ds4 = new Ds4Device(device, _loggerFactory.CreateLogger<Ds4Device>());
                var vc = new VirtualXboxController(_client!, _loggerFactory.CreateLogger<VirtualXboxController>());
                var profile = ResolveActiveProfile();
                var bridge = new ControllerBridge(ds4, vc, profile, _loggerFactory);
                EventHandler<Ds4InputState> inputTap = (_, state) => OnInputForUi(devicePath, state);
                ds4.InputReceived += inputTap;
                bridge.Failed += (_, _) => OnBridgeFailed(devicePath);
                bridge.Start(_cts!.Token);
                _bridges[devicePath] = new BridgeEntry(bridge, ds4, inputTap);
                _ui.PushConnection(ds4.Mode, devicePath, rawBattery: 0, charging: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start bridge for {Path}", devicePath);
            }
        }
    }

    private void OnInputForUi(string devicePath, Ds4InputState state)
    {
        _ui.PushInput(state);

        // Battery updates are cheap (1 push every ~5s would do); just resend
        // connection envelope at low frequency by piggy-backing every 256th frame.
        if ((state.TimestampTicks & 0xFF) == 0)
        {
            BridgeEntry? entry;
            lock (_gate)
            {
                _bridges.TryGetValue(devicePath, out entry);
            }
            if (entry is not null)
                _ui.PushConnection(entry.Device.Mode, devicePath, state.BatteryLevel, state.IsCharging);
        }
    }

    private void OnDisconnected(object? sender, string devicePath)
    {
        lock (_gate)
        {
            if (_bridges.Remove(devicePath, out var entry))
            {
                entry.Device.InputReceived -= entry.InputTap;
                entry.Bridge.Dispose();
            }
        }
        _ui.PushDisconnected();
    }

    private void OnBridgeFailed(string devicePath)
    {
        lock (_gate)
        {
            if (_bridges.Remove(devicePath, out var entry))
                entry.Device.InputReceived -= entry.InputTap;
        }
        _ui.PushDisconnected();
        if (_steamDetector.IsSteamRunning())
            _logger.LogWarning(_steamDetector.FormatHint());
    }

    private MappingProfile ResolveActiveProfile()
    {
        var cfg = _configStore.Load();
        return cfg.Profiles.TryGetValue(cfg.ActiveProfile, out var p)
            ? p
            : MappingProfile.CreateDefault();
    }

    private void BroadcastProfiles()
    {
        var cfg = _configStore.Load();
        _ui.PushProfiles(cfg.ActiveProfile, cfg.Profiles.Keys.ToList());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AppHostService stopping");
        try { _cts?.Cancel(); } catch { /* ignore */ }
        lock (_gate)
        {
            foreach (var entry in _bridges.Values)
            {
                try { entry.Device.InputReceived -= entry.InputTap; } catch { /* ignore */ }
                try { entry.Bridge.Dispose(); } catch { /* ignore */ }
            }
            _bridges.Clear();
        }
        _watcher?.Dispose();
        _client?.Dispose();
        _cts?.Dispose();
        return Task.CompletedTask;
    }

    private sealed record BridgeEntry(ControllerBridge Bridge, Ds4Device Device, EventHandler<Ds4InputState> InputTap);
}
