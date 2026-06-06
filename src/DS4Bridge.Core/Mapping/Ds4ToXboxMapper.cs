using DS4Bridge.Core.Bridge;
using DS4Bridge.Core.Models;

namespace DS4Bridge.Core.Mapping;

public static class Ds4ToXboxMapper
{
    public static void Apply(Ds4InputState state, MappingProfile profile, IVirtualXboxController vc)
    {
        foreach (var kv in profile.ButtonMap)
        {
            if (kv.Value == XboxButton.None) continue;
            var pressed = (state.Buttons & kv.Key) == kv.Key;
            vc.SetButton(kv.Value, pressed);
        }

        var (up, down, left, right) = DpadToDirections(state.Dpad);
        vc.SetDpad(up, down, left, right);

        var (lx, ly) = StickProcessor.Process(
            state.LeftStickX, state.LeftStickY,
            profile.LeftStickDeadzone, profile.StickSensitivity, profile.InvertLeftY);
        var (rx, ry) = StickProcessor.Process(
            state.RightStickX, state.RightStickY,
            profile.RightStickDeadzone, profile.StickSensitivity, profile.InvertRightY);
        vc.SetLeftThumb(lx, ly);
        vc.SetRightThumb(rx, ry);

        vc.SetLeftTrigger(ApplyThreshold(state.L2, profile.LeftTriggerThreshold));
        vc.SetRightTrigger(ApplyThreshold(state.R2, profile.RightTriggerThreshold));

        vc.SubmitReport();
    }

    private static byte ApplyThreshold(byte value, double threshold)
    {
        if (threshold <= 0.0) return value;
        var normalized = value / 255.0;
        return normalized < threshold ? (byte)0 : value;
    }

    private static (bool up, bool down, bool left, bool right) DpadToDirections(DpadDirection d) => d switch
    {
        DpadDirection.North      => (true,  false, false, false),
        DpadDirection.NorthEast  => (true,  false, false, true),
        DpadDirection.East       => (false, false, false, true),
        DpadDirection.SouthEast  => (false, true,  false, true),
        DpadDirection.South      => (false, true,  false, false),
        DpadDirection.SouthWest  => (false, true,  true,  false),
        DpadDirection.West       => (false, false, true,  false),
        DpadDirection.NorthWest  => (true,  false, true,  false),
        _                        => (false, false, false, false)
    };
}
