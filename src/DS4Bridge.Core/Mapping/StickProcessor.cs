namespace DS4Bridge.Core.Mapping;

public static class StickProcessor
{
    private const double Center = 128.0;
    private const double MaxOffsetPositive = 127.0;
    private const double MaxOffsetNegative = 128.0;

    // Returns (xboxX, xboxY) where Xbox axes are signed short in [-32768, 32767]
    // and Y follows Xbox convention (up = positive). DS4 raw input has up = 0,
    // so we always flip Y unless caller already wants DS4 convention via invertY.
    public static (short X, short Y) Process(
        byte rawX, byte rawY, double deadzone, double sensitivity, bool invertY)
    {
        var nx = Normalize(rawX);
        var ny = Normalize(rawY);

        // Convert DS4-Y (up=0, down=+) to Xbox-Y (up=+, down=-) by flipping.
        ny = -ny;
        if (invertY) ny = -ny;

        // Radial deadzone check (8% default), per-spec.
        var magnitude = Math.Sqrt(nx * nx + ny * ny);
        if (magnitude <= deadzone)
            return (0, 0);

        // Per-axis scaling with sensitivity. Diagonal corners reach full
        // per-axis range, matching standard Xbox360 behavior expected by games.
        var xOut = (short)Math.Clamp(nx * sensitivity * short.MaxValue, short.MinValue, short.MaxValue);
        var yOut = (short)Math.Clamp(ny * sensitivity * short.MaxValue, short.MinValue, short.MaxValue);
        return (xOut, yOut);
    }

    private static double Normalize(byte raw)
    {
        var offset = raw - Center;
        return offset >= 0
            ? offset / MaxOffsetPositive
            : offset / MaxOffsetNegative;
    }
}
