using DS4Bridge.Core.Mapping;
using DS4Bridge.Core.Models;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace DS4Bridge.Core.Tests.Mapping;

public class MappingProfileTests
{
    [Fact]
    public void Default_profile_has_canonical_button_map()
    {
        var p = MappingProfile.CreateDefault();
        p.ButtonMap[Ds4Buttons.Cross].Should().Be(XboxButton.A);
        p.ButtonMap[Ds4Buttons.Circle].Should().Be(XboxButton.B);
        p.ButtonMap[Ds4Buttons.Square].Should().Be(XboxButton.X);
        p.ButtonMap[Ds4Buttons.Triangle].Should().Be(XboxButton.Y);
        p.ButtonMap[Ds4Buttons.L1].Should().Be(XboxButton.LeftShoulder);
        p.ButtonMap[Ds4Buttons.R1].Should().Be(XboxButton.RightShoulder);
        p.ButtonMap[Ds4Buttons.Share].Should().Be(XboxButton.Back);
        p.ButtonMap[Ds4Buttons.Options].Should().Be(XboxButton.Start);
        p.ButtonMap[Ds4Buttons.L3].Should().Be(XboxButton.LeftThumb);
        p.ButtonMap[Ds4Buttons.R3].Should().Be(XboxButton.RightThumb);
        p.ButtonMap[Ds4Buttons.Ps].Should().Be(XboxButton.Guide);
    }

    [Fact]
    public void Default_profile_has_sensible_deadzones()
    {
        var p = MappingProfile.CreateDefault();
        p.LeftStickDeadzone.Should().BeApproximately(0.08, 0.0001);
        p.RightStickDeadzone.Should().BeApproximately(0.08, 0.0001);
        p.StickSensitivity.Should().Be(1.0);
        p.RumbleEnabled.Should().BeTrue();
    }

    [Fact]
    public void Profile_round_trips_through_json()
    {
        var p = MappingProfile.CreateDefault() with
        {
            LeftStickDeadzone = 0.15,
            InvertLeftY = true,
            LightbarColor = new RgbColor(255, 0, 64)
        };
        var json = JsonSerializer.Serialize(p, MappingProfile.JsonOptions);
        var rt = JsonSerializer.Deserialize<MappingProfile>(json, MappingProfile.JsonOptions);
        rt.Should().NotBeNull();
        rt!.LeftStickDeadzone.Should().Be(0.15);
        rt.InvertLeftY.Should().BeTrue();
        rt.LightbarColor.Should().Be(new RgbColor(255, 0, 64));
        rt.ButtonMap.Should().ContainKey(Ds4Buttons.Cross).WhoseValue.Should().Be(XboxButton.A);
    }
}
