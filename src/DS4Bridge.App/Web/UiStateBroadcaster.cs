using DS4Bridge.Core.Models;

namespace DS4Bridge.App.Web;

// Single point that pushes app state to the React UI. Throttles input updates
// so we don't flood the IPC channel at 250Hz.
public sealed class UiStateBroadcaster
{
    private const int InputThrottleMs = 33; // ~30 fps visual refresh
    private WebViewHost? _host;
    private long _lastInputTicks;

    public void Attach(WebViewHost host) => _host = host;
    public void Detach() => _host = null;

    public void PushConnection(ConnectionMode mode, string devicePath, byte rawBattery, bool charging)
    {
        // DS4 battery raw is 0-10 when on battery, 11-15 when charging.
        // Convert to a percentage clamped to [0, 100].
        var percent = charging
            ? Math.Clamp(((rawBattery & 0x0F) - 10) * 10, 0, 100)
            : Math.Clamp(rawBattery * 10, 0, 100);
        _host?.Post(ConnectionMessage.Live(mode.ToString(), devicePath, percent, charging));
    }

    public void PushDisconnected() => _host?.Post(ConnectionMessage.Idle());

    public void PushInput(in Ds4InputState state)
    {
        var now = Environment.TickCount64;
        if (now - _lastInputTicks < InputThrottleMs) return;
        _lastInputTicks = now;

        _host?.Post(new InputMessage(
            InputMessage.TypeName,
            state.LeftStickX, state.LeftStickY,
            state.RightStickX, state.RightStickY,
            state.L2, state.R2,
            (uint)state.Buttons,
            (byte)state.Dpad));
    }

    public void PushProfiles(string active, IReadOnlyList<string> available) =>
        _host?.Post(new ProfilesMessage(ProfilesMessage.TypeName, active, available));
}
