using DS4Bridge.Core.Models;
using DS4Bridge.Core.Parsing;
using FluentAssertions;
using Xunit;

namespace DS4Bridge.Core.Tests.Parsing;

public class Ds4ParserTests
{
    // Build a 64-byte USB report: byte 0 = report ID 0x01, controller data
    // starts at byte 1. So index N in the spec table => raw byte N+1.
    private static byte[] BuildUsbReport(Action<byte[]> setBytes)
    {
        var buf = new byte[64];
        buf[0] = 0x01;
        // Set neutral defaults at data offsets:
        buf[1 + 0] = 128; // LX
        buf[1 + 1] = 128; // LY
        buf[1 + 2] = 128; // RX
        buf[1 + 3] = 128; // RY
        buf[1 + 4] = 0x08; // dpad neutral, no face buttons
        setBytes(buf);
        return buf;
    }

    [Fact]
    public void Usb_neutral_report_yields_neutral_state()
    {
        var report = BuildUsbReport(_ => { });
        var state = Ds4Parser.Parse(report, ConnectionMode.Usb);
        state.LeftStickX.Should().Be(128);
        state.LeftStickY.Should().Be(128);
        state.RightStickX.Should().Be(128);
        state.RightStickY.Should().Be(128);
        state.Buttons.Should().Be(Ds4Buttons.None);
        state.Dpad.Should().Be(DpadDirection.Neutral);
    }

    [Fact]
    public void Usb_sticks_parsed_at_correct_offsets()
    {
        var report = BuildUsbReport(b =>
        {
            b[1 + 0] = 10;
            b[1 + 1] = 200;
            b[1 + 2] = 50;
            b[1 + 3] = 150;
        });
        var state = Ds4Parser.Parse(report, ConnectionMode.Usb);
        state.LeftStickX.Should().Be(10);
        state.LeftStickY.Should().Be(200);
        state.RightStickX.Should().Be(50);
        state.RightStickY.Should().Be(150);
    }

    [Theory]
    [InlineData(0x00, DpadDirection.North)]
    [InlineData(0x01, DpadDirection.NorthEast)]
    [InlineData(0x02, DpadDirection.East)]
    [InlineData(0x03, DpadDirection.SouthEast)]
    [InlineData(0x04, DpadDirection.South)]
    [InlineData(0x05, DpadDirection.SouthWest)]
    [InlineData(0x06, DpadDirection.West)]
    [InlineData(0x07, DpadDirection.NorthWest)]
    [InlineData(0x08, DpadDirection.Neutral)]
    public void Usb_dpad_low_nibble_is_enum_not_bits(byte nibble, DpadDirection expected)
    {
        var report = BuildUsbReport(b => b[1 + 4] = nibble);
        var state = Ds4Parser.Parse(report, ConnectionMode.Usb);
        state.Dpad.Should().Be(expected);
    }

    [Fact]
    public void Usb_face_buttons_in_high_nibble_of_byte4()
    {
        // Set Square (bit 4), Cross (bit 5), Circle (bit 6), Triangle (bit 7)
        var report = BuildUsbReport(b => b[1 + 4] = 0xF8); // dpad=8, faces=1111
        var state = Ds4Parser.Parse(report, ConnectionMode.Usb);
        state.Buttons.Should().HaveFlag(Ds4Buttons.Square);
        state.Buttons.Should().HaveFlag(Ds4Buttons.Cross);
        state.Buttons.Should().HaveFlag(Ds4Buttons.Circle);
        state.Buttons.Should().HaveFlag(Ds4Buttons.Triangle);
    }

    [Fact]
    public void Usb_shoulder_trigger_thumb_buttons_in_byte5()
    {
        // Each bit: L1=0, R1=1, L2=2, R2=3, Share=4, Options=5, L3=6, R3=7
        var report = BuildUsbReport(b => b[1 + 5] = 0xFF);
        var state = Ds4Parser.Parse(report, ConnectionMode.Usb);
        state.Buttons.Should().HaveFlag(Ds4Buttons.L1);
        state.Buttons.Should().HaveFlag(Ds4Buttons.R1);
        state.Buttons.Should().HaveFlag(Ds4Buttons.L2);
        state.Buttons.Should().HaveFlag(Ds4Buttons.R2);
        state.Buttons.Should().HaveFlag(Ds4Buttons.Share);
        state.Buttons.Should().HaveFlag(Ds4Buttons.Options);
        state.Buttons.Should().HaveFlag(Ds4Buttons.L3);
        state.Buttons.Should().HaveFlag(Ds4Buttons.R3);
    }

    [Fact]
    public void Usb_ps_and_touchpad_in_byte6()
    {
        var report = BuildUsbReport(b => b[1 + 6] = 0x03); // bits 0 and 1
        var state = Ds4Parser.Parse(report, ConnectionMode.Usb);
        state.Buttons.Should().HaveFlag(Ds4Buttons.Ps);
        state.Buttons.Should().HaveFlag(Ds4Buttons.TouchpadClick);
    }

    [Fact]
    public void Usb_triggers_byte7_and_byte8()
    {
        var report = BuildUsbReport(b => { b[1 + 7] = 200; b[1 + 8] = 50; });
        var state = Ds4Parser.Parse(report, ConnectionMode.Usb);
        state.L2.Should().Be(200);
        state.R2.Should().Be(50);
    }

    [Fact]
    public void Usb_gyro_accel_signed_little_endian()
    {
        var report = BuildUsbReport(b =>
        {
            // Gyro X = -1 => 0xFFFF little-endian
            b[1 + 12] = 0xFF; b[1 + 13] = 0xFF;
            // Gyro Y = 256
            b[1 + 14] = 0x00; b[1 + 15] = 0x01;
            // Accel X = 1000
            b[1 + 18] = 0xE8; b[1 + 19] = 0x03;
        });
        var state = Ds4Parser.Parse(report, ConnectionMode.Usb);
        state.GyroX.Should().Be(-1);
        state.GyroY.Should().Be(256);
        state.AccelX.Should().Be(1000);
    }

    [Fact]
    public void Usb_throws_on_undersized_report()
    {
        var tooShort = new byte[20];
        var act = () => Ds4Parser.Parse(tooShort, ConnectionMode.Usb);
        act.Should().Throw<ArgumentException>();
    }

    private static byte[] BuildBluetoothReport(Action<byte[]> setBytes)
    {
        var buf = new byte[78];
        buf[0] = 0x11;
        // 2 header bytes after report id are typically status/sequence; leave 0
        // Data starts at offset 3.
        buf[3 + 0] = 128;
        buf[3 + 1] = 128;
        buf[3 + 2] = 128;
        buf[3 + 3] = 128;
        buf[3 + 4] = 0x08;
        setBytes(buf);
        return buf;
    }

    [Fact]
    public void Bluetooth_sticks_parsed_at_offset_3()
    {
        var report = BuildBluetoothReport(b =>
        {
            b[3 + 0] = 20;
            b[3 + 1] = 220;
            b[3 + 2] = 40;
            b[3 + 3] = 180;
        });
        var state = Ds4Parser.Parse(report, ConnectionMode.Bluetooth);
        state.LeftStickX.Should().Be(20);
        state.LeftStickY.Should().Be(220);
        state.RightStickX.Should().Be(40);
        state.RightStickY.Should().Be(180);
    }

    [Fact]
    public void Bluetooth_buttons_and_triggers()
    {
        var report = BuildBluetoothReport(b =>
        {
            b[3 + 4] = 0xF2;          // dpad=2 (East), Square+Cross+Circle+Triangle high nibble
            b[3 + 5] = 0xFF;
            b[3 + 6] = 0x03;
            b[3 + 7] = 99;
            b[3 + 8] = 199;
        });
        var state = Ds4Parser.Parse(report, ConnectionMode.Bluetooth);
        state.Dpad.Should().Be(DpadDirection.East);
        state.Buttons.Should().HaveFlag(Ds4Buttons.Square);
        state.Buttons.Should().HaveFlag(Ds4Buttons.L3);
        state.Buttons.Should().HaveFlag(Ds4Buttons.Ps);
        state.L2.Should().Be(99);
        state.R2.Should().Be(199);
    }

    [Fact]
    public void Bluetooth_throws_on_undersized_report()
    {
        var tooShort = new byte[20];
        var act = () => Ds4Parser.Parse(tooShort, ConnectionMode.Bluetooth);
        act.Should().Throw<ArgumentException>();
    }
}