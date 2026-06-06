using HidSharp;
using Microsoft.Extensions.Logging;

namespace DS4Bridge.Hid;

public sealed class DeviceWatcher : IDisposable
{
    private readonly ILogger<DeviceWatcher> _logger;
    private readonly DeviceList _list = DeviceList.Local;
    private readonly HashSet<string> _known = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _gate = new();

    public event EventHandler<HidDevice>? Connected;
    public event EventHandler<string>? Disconnected;

    public DeviceWatcher(ILogger<DeviceWatcher> logger)
    {
        _logger = logger;
    }

    private bool _started;

    public void Start()
    {
        if (_started) return;
        _started = true;
        _list.Changed += OnListChanged;
        Rescan();
    }

    // Drop known-device state and re-emit Connected for every current DS4.
    // Used when consumers tore down their per-device wrappers and need
    // them rebuilt without a physical replug.
    public void ForceRescan()
    {
        lock (_gate)
        {
            _known.Clear();
        }
        Rescan();
    }

    private void OnListChanged(object? sender, DeviceListChangedEventArgs e) => Rescan();

    private void Rescan()
    {
        lock (_gate)
        {
            var current = _list.GetHidDevices()
                .Where(d => Ds4DeviceIds.IsDs4(d.VendorID, d.ProductID))
                .ToDictionary(d => d.DevicePath ?? string.Empty, d => d, StringComparer.OrdinalIgnoreCase);

            foreach (var path in _known.Where(p => !current.ContainsKey(p)).ToList())
            {
                _known.Remove(path);
                _logger.LogInformation("DS4 disconnected: {Path}", path);
                Disconnected?.Invoke(this, path);
            }

            foreach (var (path, dev) in current)
            {
                if (string.IsNullOrEmpty(path)) continue;
                if (_known.Add(path))
                {
                    _logger.LogInformation(
                        "DS4 connected: VID={Vid:X4} PID={Pid:X4} Path={Path}",
                        dev.VendorID, dev.ProductID, path);
                    Connected?.Invoke(this, dev);
                }
            }
        }
    }

    public void Dispose()
    {
        if (!_started) return;
        _list.Changed -= OnListChanged;
        _started = false;
    }
}
