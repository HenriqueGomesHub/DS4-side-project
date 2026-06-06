using DS4Bridge.Core.Bridge;
using DS4Bridge.Core.Mapping;
using DS4Bridge.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DS4Bridge.Core.Tests.Bridge;

public class ControllerBridgeTests
{
    private static (Mock<IDs4Device> ds4, Mock<IVirtualXboxController> vc) MakeMocks()
    {
        var ds4 = new Mock<IDs4Device>();
        ds4.SetupGet(d => d.Mode).Returns(ConnectionMode.Usb);
        ds4.SetupGet(d => d.DevicePath).Returns(@"USB\some-path");
        ds4.SetupGet(d => d.IsOpen).Returns(true);
        var vc = new Mock<IVirtualXboxController>();
        return (ds4, vc);
    }

    [Fact]
    public void Start_connects_virtual_and_starts_ds4()
    {
        var (ds4, vc) = MakeMocks();
        var bridge = new ControllerBridge(ds4.Object, vc.Object, MappingProfile.CreateDefault(), NullLoggerFactory.Instance);

        bridge.Start(CancellationToken.None);

        vc.Verify(v => v.Connect(), Times.Once);
        ds4.Verify(d => d.Start(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Ds4_input_applies_to_virtual()
    {
        var (ds4, vc) = MakeMocks();
        var bridge = new ControllerBridge(ds4.Object, vc.Object, MappingProfile.CreateDefault(), NullLoggerFactory.Instance);
        bridge.Start(CancellationToken.None);

        ds4.Raise(d => d.InputReceived += null, ds4.Object,
            Ds4InputState.Neutral with { Buttons = Ds4Buttons.Cross });

        vc.Verify(v => v.SetButton(XboxButton.A, true), Times.AtLeastOnce);
        vc.Verify(v => v.SubmitReport(), Times.AtLeastOnce);
    }

    [Fact]
    public void Virtual_feedback_writes_rumble_to_ds4_when_enabled()
    {
        var (ds4, vc) = MakeMocks();
        var bridge = new ControllerBridge(ds4.Object, vc.Object, MappingProfile.CreateDefault(), NullLoggerFactory.Instance);
        bridge.Start(CancellationToken.None);

        vc.Raise(v => v.FeedbackReceived += null, vc.Object, new Ds4FeedbackEvent(200, 100));

        ds4.Verify(d => d.WriteOutput(It.Is<Ds4OutputState>(o =>
            o.RumbleStrong == 200 && o.RumbleWeak == 100)), Times.Once);
    }

    [Fact]
    public void Rumble_suppressed_when_profile_disables_it()
    {
        var (ds4, vc) = MakeMocks();
        var profile = MappingProfile.CreateDefault() with { RumbleEnabled = false };
        var bridge = new ControllerBridge(ds4.Object, vc.Object, profile, NullLoggerFactory.Instance);
        bridge.Start(CancellationToken.None);

        vc.Raise(v => v.FeedbackReceived += null, vc.Object, new Ds4FeedbackEvent(200, 100));

        ds4.Verify(d => d.WriteOutput(It.IsAny<Ds4OutputState>()), Times.Never);
    }

    [Fact]
    public void Read_failure_disposes_both_sides()
    {
        var (ds4, vc) = MakeMocks();
        var bridge = new ControllerBridge(ds4.Object, vc.Object, MappingProfile.CreateDefault(), NullLoggerFactory.Instance);
        bridge.Start(CancellationToken.None);

        ds4.Raise(d => d.ReadFailed += null, ds4.Object, new IOException("disconnect"));

        vc.Verify(v => v.Disconnect(), Times.AtLeastOnce);
        ds4.Verify(d => d.Dispose(), Times.AtLeastOnce);
    }
}
