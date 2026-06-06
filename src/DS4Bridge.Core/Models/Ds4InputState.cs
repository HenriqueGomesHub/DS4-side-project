namespace DS4Bridge.Core.Models;

// Parsed snapshot of one DS4 input report. Values are normalized to standard
// ranges: sticks as raw unsigned bytes (0-255, center ~128); triggers 0-255;
// gyro/accel as raw signed 16-bit values; battery 0-15 (raw, see spec).
public sealed record Ds4InputState
{
    public byte LeftStickX { get; init; }
    public byte LeftStickY { get; init; }
    public byte RightStickX { get; init; }
    public byte RightStickY { get; init; }
    public byte L2 { get; init; }
    public byte R2 { get; init; }
    public Ds4Buttons Buttons { get; init; }
    public DpadDirection Dpad { get; init; } = DpadDirection.Neutral;
    public short GyroX { get; init; }
    public short GyroY { get; init; }
    public short GyroZ { get; init; }
    public short AccelX { get; init; }
    public short AccelY { get; init; }
    public short AccelZ { get; init; }
    public byte BatteryLevel { get; init; }
    public bool IsCharging { get; init; }
    public long TimestampTicks { get; init; }

    public static Ds4InputState Neutral => new()
    {
        LeftStickX = 128, LeftStickY = 128,
        RightStickX = 128, RightStickY = 128,
        Dpad = DpadDirection.Neutral
    };
}
