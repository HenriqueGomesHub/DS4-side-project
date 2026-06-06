using DS4Bridge.Core.Mapping;
using DS4Bridge.Core.Models;
using Microsoft.Extensions.Logging;

namespace DS4Bridge.Core.Bridge;

public sealed class ControllerBridge : IDisposable
{
    private readonly IDs4Device _ds4;
    private readonly IVirtualXboxController _virtual;
    private readonly ILogger<ControllerBridge> _logger;
    private MappingProfile _profile;
    private int _disposed;

    public event EventHandler? Failed;

    public IDs4Device Ds4 => _ds4;
    public ConnectionMode Mode => _ds4.Mode;

    public ControllerBridge(
        IDs4Device ds4,
        IVirtualXboxController virtualController,
        MappingProfile profile,
        ILoggerFactory loggerFactory)
    {
        _ds4 = ds4;
        _virtual = virtualController;
        _profile = profile;
        _logger = loggerFactory.CreateLogger<ControllerBridge>();
    }

    public void UpdateProfile(MappingProfile profile) => _profile = profile;

    public void Start(CancellationToken token)
    {
        _ds4.InputReceived += OnInput;
        _ds4.ReadFailed += OnReadFailed;
        _virtual.FeedbackReceived += OnFeedback;

        _virtual.Connect();
        _ds4.Start(token);
    }

    private void OnInput(object? sender, Ds4InputState state)
    {
        try
        {
            Ds4ToXboxMapper.Apply(state, _profile, _virtual);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mapping/apply failed");
        }
    }

    private void OnFeedback(object? sender, Ds4FeedbackEvent feedback)
    {
        if (!_profile.RumbleEnabled) return;
        _ds4.WriteOutput(new Ds4OutputState
        {
            RumbleStrong = feedback.StrongMotor,
            RumbleWeak = feedback.WeakMotor,
            LightbarR = _profile.LightbarColor.R,
            LightbarG = _profile.LightbarColor.G,
            LightbarB = _profile.LightbarColor.B
        });
    }

    private void OnReadFailed(object? sender, Exception ex)
    {
        _logger.LogWarning(ex, "DS4 read failed; tearing down bridge");
        Dispose();
        Failed?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
        _ds4.InputReceived -= OnInput;
        _ds4.ReadFailed -= OnReadFailed;
        _virtual.FeedbackReceived -= OnFeedback;
        try { _virtual.Disconnect(); } catch { /* ignore */ }
        try { _ds4.Dispose(); } catch { /* ignore */ }
        try { _virtual.Dispose(); } catch { /* ignore */ }
    }
}
