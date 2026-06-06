namespace DS4Bridge.Core.Models;

public sealed record Ds4OutputState
{
    public byte RumbleStrong { get; init; } // left motor (heavy), 0-255
    public byte RumbleWeak { get; init; }   // right motor (light), 0-255
    public byte LightbarR { get; init; }
    public byte LightbarG { get; init; }
    public byte LightbarB { get; init; }
    public byte FlashOn { get; init; }
    public byte FlashOff { get; init; }

    public static Ds4OutputState Off => new();
}
