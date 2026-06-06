using System.Text.Json;
using System.Text.Json.Serialization;
using DS4Bridge.Core.Models;

namespace DS4Bridge.Core.Mapping;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum XboxButton
{
    A, B, X, Y,
    LeftShoulder, RightShoulder,
    Back, Start,
    LeftThumb, RightThumb,
    Guide,
    DpadUp, DpadDown, DpadLeft, DpadRight,
    None
}

public readonly record struct RgbColor(byte R, byte G, byte B);

public sealed record MappingProfile
{
    public required IReadOnlyDictionary<Ds4Buttons, XboxButton> ButtonMap { get; init; }
    public double LeftStickDeadzone { get; init; } = 0.08;
    public double RightStickDeadzone { get; init; } = 0.08;
    public double LeftTriggerThreshold { get; init; } = 0.0;
    public double RightTriggerThreshold { get; init; } = 0.0;
    public bool InvertLeftY { get; init; }
    public bool InvertRightY { get; init; }
    public double StickSensitivity { get; init; } = 1.0;
    public bool RumbleEnabled { get; init; } = true;
    public RgbColor LightbarColor { get; init; } = new(0, 0, 64);

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public static MappingProfile CreateDefault() => new()
    {
        ButtonMap = new Dictionary<Ds4Buttons, XboxButton>
        {
            [Ds4Buttons.Cross]    = XboxButton.A,
            [Ds4Buttons.Circle]   = XboxButton.B,
            [Ds4Buttons.Square]   = XboxButton.X,
            [Ds4Buttons.Triangle] = XboxButton.Y,
            [Ds4Buttons.L1]       = XboxButton.LeftShoulder,
            [Ds4Buttons.R1]       = XboxButton.RightShoulder,
            [Ds4Buttons.Share]    = XboxButton.Back,
            [Ds4Buttons.Options]  = XboxButton.Start,
            [Ds4Buttons.L3]       = XboxButton.LeftThumb,
            [Ds4Buttons.R3]       = XboxButton.RightThumb,
            [Ds4Buttons.Ps]       = XboxButton.Guide,
            [Ds4Buttons.TouchpadClick] = XboxButton.None
        }
    };
}
