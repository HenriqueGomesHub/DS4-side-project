using DS4Bridge.Core.Mapping;
using FluentAssertions;
using Xunit;

namespace DS4Bridge.Core.Tests.Mapping;

public class StickProcessorTests
{
    [Fact]
    public void Centered_stick_returns_zero_axes()
    {
        var (x, y) = StickProcessor.Process(128, 128,
            deadzone: 0.08, sensitivity: 1.0, invertY: false);
        x.Should().Be(0);
        y.Should().Be(0);
    }

    [Fact]
    public void Full_right_full_down_maps_near_short_max_with_y_inverted_xbox_convention()
    {
        // DS4 raw: X=255 (right), Y=255 (down). After Xbox-axis inversion,
        // Y should become negative because Xbox convention is up=+.
        var (x, y) = StickProcessor.Process(255, 255,
            deadzone: 0.0, sensitivity: 1.0, invertY: false);
        x.Should().BeGreaterThan(32000);
        y.Should().BeLessThan(-32000);
    }

    [Fact]
    public void Full_left_full_up_maps_near_short_min_and_positive_y()
    {
        var (x, y) = StickProcessor.Process(0, 0,
            deadzone: 0.0, sensitivity: 1.0, invertY: false);
        x.Should().BeLessThan(-32000);
        y.Should().BeGreaterThan(32000);
    }

    [Fact]
    public void Inside_radial_deadzone_returns_zero()
    {
        // 130, 130: small offset from center. With 0.5 deadzone (50% radius),
        // this should be eaten.
        var (x, y) = StickProcessor.Process(130, 130,
            deadzone: 0.5, sensitivity: 1.0, invertY: false);
        x.Should().Be(0);
        y.Should().Be(0);
    }

    [Fact]
    public void Outside_deadzone_rescales_from_deadzone_edge_to_full_range()
    {
        // Just outside an 8% deadzone, output should not start at full magnitude
        // (avoid the "snap" effect of axial deadzones).
        var (xJust, _) = StickProcessor.Process(
            (byte)(128 + (int)(127 * 0.10)), 128,
            deadzone: 0.08, sensitivity: 1.0, invertY: false);
        xJust.Should().BeInRange(1, 5000);

        // At full deflection, output saturates near max
        var (xFull, _) = StickProcessor.Process(255, 128,
            deadzone: 0.08, sensitivity: 1.0, invertY: false);
        xFull.Should().BeGreaterThan(32000);
    }

    [Fact]
    public void Invert_y_flips_sign()
    {
        var (_, yNormal) = StickProcessor.Process(128, 0,
            deadzone: 0.0, sensitivity: 1.0, invertY: false);
        var (_, yInverted) = StickProcessor.Process(128, 0,
            deadzone: 0.0, sensitivity: 1.0, invertY: true);
        Math.Sign(yNormal).Should().Be(-Math.Sign(yInverted));
    }

    [Fact]
    public void Sensitivity_below_one_compresses_magnitude()
    {
        var (xFull, _) = StickProcessor.Process(255, 128,
            deadzone: 0.0, sensitivity: 1.0, invertY: false);
        var (xHalf, _) = StickProcessor.Process(255, 128,
            deadzone: 0.0, sensitivity: 0.5, invertY: false);
        xHalf.Should().BeLessThan(xFull);
        xHalf.Should().BeGreaterThan(0);
    }
}
