namespace DS4Bridge.App.Web;

// Wire-types for C# -> JS messages. Field names serialize as camelCase via the
// host's JsonSerializerOptions; keep names short — these go over the wire often.

public sealed record ConnectionMessage(
    string Type,
    bool Connected,
    string Mode,
    string DevicePath,
    int Battery,
    bool Charging)
{
    public static ConnectionMessage Live(string mode, string devicePath, int battery, bool charging) =>
        new("connection", true, mode, devicePath, battery, charging);

    public static ConnectionMessage Idle() =>
        new("connection", false, string.Empty, string.Empty, 0, false);
}

public sealed record InputMessage(
    string Type,
    byte Lx, byte Ly,
    byte Rx, byte Ry,
    byte L2, byte R2,
    uint Buttons,
    byte Dpad)
{
    public const string TypeName = "input";
}

public sealed record ProfilesMessage(
    string Type,
    string Active,
    IReadOnlyList<string> Available)
{
    public const string TypeName = "profiles";
}

public sealed record LogMessage(
    string Type,
    string Level,
    string Message)
{
    public const string TypeName = "log";
}
