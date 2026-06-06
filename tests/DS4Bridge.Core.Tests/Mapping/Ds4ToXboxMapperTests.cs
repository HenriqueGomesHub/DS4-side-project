using DS4Bridge.Core.Bridge;
using DS4Bridge.Core.Mapping;
using DS4Bridge.Core.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace DS4Bridge.Core.Tests.Mapping;

public class Ds4ToXboxMapperTests
{
    [Fact]
    public void Apply_routes_face_buttons_through_profile()
    {
        var vc = new Mock<IVirtualXboxController>();
        var profile = MappingProfile.CreateDefault();
        var state = Ds4InputState.Neutral with { Buttons = Ds4Buttons.Cross };

        Ds4ToXboxMapper.Apply(state, profile, vc.Object);

        vc.Verify(v => v.SetButton(XboxButton.A, true), Times.Once);
        vc.Verify(v => v.SetButton(XboxButton.B, false), Times.Once);
        vc.Verify(v => v.SubmitReport(), Times.Once);
    }

    [Fact]
    public void Apply_decomposes_dpad_into_4_directions()
    {
        var vc = new Mock<IVirtualXboxController>();
        var profile = MappingProfile.CreateDefault();
        var state = Ds4InputState.Neutral with { Dpad = DpadDirection.NorthEast };

        Ds4ToXboxMapper.Apply(state, profile, vc.Object);

        vc.Verify(v => v.SetDpad(true, false, false, true), Times.Once);
    }

    [Fact]
    public void Apply_neutral_dpad_clears_all_4()
    {
        var vc = new Mock<IVirtualXboxController>();
        var profile = MappingProfile.CreateDefault();

        Ds4ToXboxMapper.Apply(Ds4InputState.Neutral, profile, vc.Object);

        vc.Verify(v => v.SetDpad(false, false, false, false), Times.Once);
    }

    [Fact]
    public void Apply_sends_triggers_as_bytes_after_threshold()
    {
        var vc = new Mock<IVirtualXboxController>();
        var profile = MappingProfile.CreateDefault() with
        {
            LeftTriggerThreshold = 0.5
        };
        var state = Ds4InputState.Neutral with { L2 = 100, R2 = 200 };

        Ds4ToXboxMapper.Apply(state, profile, vc.Object);

        // Left trigger 100/255 = 0.39, below threshold => 0
        vc.Verify(v => v.SetLeftTrigger(0), Times.Once);
        // Right trigger has no threshold => passes through
        vc.Verify(v => v.SetRightTrigger(200), Times.Once);
    }

    [Fact]
    public void Apply_routes_unmapped_button_as_noop()
    {
        var vc = new Mock<IVirtualXboxController>();
        var profile = MappingProfile.CreateDefault();
        var state = Ds4InputState.Neutral with { Buttons = Ds4Buttons.TouchpadClick };

        Ds4ToXboxMapper.Apply(state, profile, vc.Object);

        vc.Verify(v => v.SetButton(XboxButton.None, It.IsAny<bool>()), Times.Never);
    }
}
