using DS4Bridge.Core.Models;

namespace DS4Bridge.Core.Parsing;

public static class Ds4Parser
{
    // Minimum sizes after stripping report-id prefix.
    private const int MinUsbDataLength = 31;          // we touch up to byte 30 (battery+charging)
    private const int MinBluetoothDataLength = 31;
    private const int UsbDataOffset = 1;              // 1-byte report id
    private const int BluetoothDataOffset = 3;        // report id + 2 header

    public static Ds4InputState Parse(ReadOnlySpan<byte> report, ConnectionMode mode)
    {
        var dataOffset = mode == ConnectionMode.Usb ? UsbDataOffset : BluetoothDataOffset;
        var minLength = mode == ConnectionMode.Usb
            ? UsbDataOffset + MinUsbDataLength
            : BluetoothDataOffset + MinBluetoothDataLength;
        if (report.Length < minLength)
            throw new ArgumentException(
                $"Report too short for {mode}: got {report.Length}, need >= {minLength}",
                nameof(report));

        var data = report[dataOffset..];

        var dpadNibble = (byte)(data[4] & 0x0F);
        var dpad = dpadNibble <= 8 ? (DpadDirection)dpadNibble : DpadDirection.Neutral;

        Ds4Buttons buttons = Ds4Buttons.None;

        var b4 = data[4];
        if ((b4 & 0x10) != 0) buttons |= Ds4Buttons.Square;
        if ((b4 & 0x20) != 0) buttons |= Ds4Buttons.Cross;
        if ((b4 & 0x40) != 0) buttons |= Ds4Buttons.Circle;
        if ((b4 & 0x80) != 0) buttons |= Ds4Buttons.Triangle;

        var b5 = data[5];
        if ((b5 & 0x01) != 0) buttons |= Ds4Buttons.L1;
        if ((b5 & 0x02) != 0) buttons |= Ds4Buttons.R1;
        if ((b5 & 0x04) != 0) buttons |= Ds4Buttons.L2;
        if ((b5 & 0x08) != 0) buttons |= Ds4Buttons.R2;
        if ((b5 & 0x10) != 0) buttons |= Ds4Buttons.Share;
        if ((b5 & 0x20) != 0) buttons |= Ds4Buttons.Options;
        if ((b5 & 0x40) != 0) buttons |= Ds4Buttons.L3;
        if ((b5 & 0x80) != 0) buttons |= Ds4Buttons.R3;

        var b6 = data[6];
        if ((b6 & 0x01) != 0) buttons |= Ds4Buttons.Ps;
        if ((b6 & 0x02) != 0) buttons |= Ds4Buttons.TouchpadClick;

        var batteryByte = data[30];
        var batteryLevel = (byte)(batteryByte & 0x0F);
        var isCharging = (batteryByte & 0x10) != 0;

        return new Ds4InputState
        {
            LeftStickX = data[0],
            LeftStickY = data[1],
            RightStickX = data[2],
            RightStickY = data[3],
            Buttons = buttons,
            Dpad = dpad,
            L2 = data[7],
            R2 = data[8],
            GyroX = ReadShortLE(data,12),
            GyroY = ReadShortLE(data,14),
            GyroZ = ReadShortLE(data,16),
            AccelX = ReadShortLE(data,18),
            AccelY = ReadShortLE(data,20),
            AccelZ = ReadShortLE(data,22),
            BatteryLevel = batteryLevel,
            IsCharging = isCharging,
            TimestampTicks = DateTime.UtcNow.Ticks
        };
    }

    private static short ReadShortLE(ReadOnlySpan<byte> data, int offset) =>
        (short)(data[offset] | (data[offset + 1] << 8));
}