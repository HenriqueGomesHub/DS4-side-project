using DS4Bridge.Core.Bridge;
using DS4Bridge.Core.Models;
using DS4Bridge.Core.Parsing;
using HidSharp;
using Microsoft.Extensions.Logging;

namespace DS4Bridge.Hid;

public sealed class Ds4Device : IDs4Device
{
    private readonly HidDevice _hidDevice;
    private readonly ILogger<Ds4Device> _logger;
    private HidStream? _stream;
    private Thread? _readThread;
    private CancellationTokenSource? _cts;
    private readonly object _writeLock = new();

    public string DevicePath { get; }
    public ConnectionMode Mode { get; }
    public bool IsOpen { get; private set; }
    public event EventHandler<Ds4InputState>? InputReceived;
    public event EventHandler<Exception>? ReadFailed;

    public Ds4Device(HidDevice hidDevice, ILogger<Ds4Device> logger)
    {
        _hidDevice = hidDevice;
        _logger = logger;
        DevicePath = hidDevice.DevicePath ?? string.Empty;
        Mode = DetectMode(DevicePath);
    }

    private static ConnectionMode DetectMode(string devicePath) =>
        devicePath.Contains("BTHENUM", StringComparison.OrdinalIgnoreCase)
            ? ConnectionMode.Bluetooth
            : ConnectionMode.Usb;

    public void Start(CancellationToken token)
    {
        if (IsOpen) throw new InvalidOperationException("Already started");

        _stream = _hidDevice.Open();
        _stream.ReadTimeout = Timeout.Infinite;
        IsOpen = true;

        if (Mode == ConnectionMode.Bluetooth)
            BluetoothFeatureReport.EnableFullReport(_stream, _logger);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _readThread = new Thread(() => ReadLoop(_cts.Token))
        {
            IsBackground = true,
            Name = $"DS4Read-{Mode}"
        };
        _readThread.Start();
        _logger.LogInformation("DS4 device opened ({Mode}): {Path}", Mode, DevicePath);
    }

    private void ReadLoop(CancellationToken token)
    {
        var bufferSize = Mode == ConnectionMode.Usb ? 64 : 78;
        var buffer = new byte[bufferSize];
        try
        {
            while (!token.IsCancellationRequested && _stream is not null)
            {
                int read;
                try
                {
                    read = _stream.Read(buffer, 0, buffer.Length);
                }
                catch (OperationCanceledException) { break; }
                catch (IOException ex)
                {
                    _logger.LogWarning(ex, "HID read IOException — device likely disconnected");
                    ReadFailed?.Invoke(this, ex);
                    break;
                }

                if (read <= 0) continue;

                Ds4InputState state;
                try
                {
                    state = Ds4Parser.Parse(buffer.AsSpan(0, read), Mode);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex, "Skipped unexpected report length {Len}", read);
                    continue;
                }

                InputReceived?.Invoke(this, state);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected read-loop failure");
            ReadFailed?.Invoke(this, ex);
        }
        finally
        {
            IsOpen = false;
        }
    }

    public void WriteOutput(Ds4OutputState output)
    {
        if (_stream is null || !IsOpen) return;
        if (Mode == ConnectionMode.Bluetooth)
        {
            // v1: BT output requires CRC32 and a 78-byte layout. Skipping for v1
            // per spec; only USB output is supported.
            return;
        }

        var buf = new byte[32];
        buf[0] = 0x05;       // report id
        buf[1] = 0xFF;       // flags: update everything
        buf[4] = output.RumbleWeak;   // weak (right)
        buf[5] = output.RumbleStrong; // strong (left)
        buf[6] = output.LightbarR;
        buf[7] = output.LightbarG;
        buf[8] = output.LightbarB;
        buf[9] = output.FlashOn;
        buf[10] = output.FlashOff;

        try
        {
            lock (_writeLock)
            {
                _stream.Write(buf, 0, buf.Length);
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "HID write failed");
        }
    }

    public void Dispose()
    {
        try { _cts?.Cancel(); } catch { /* ignore */ }
        try { _readThread?.Join(TimeSpan.FromMilliseconds(500)); } catch { /* ignore */ }
        try { _stream?.Dispose(); } catch { /* ignore */ }
        _cts?.Dispose();
        IsOpen = false;
        _logger.LogInformation("DS4 device disposed: {Path}", DevicePath);
    }
}
