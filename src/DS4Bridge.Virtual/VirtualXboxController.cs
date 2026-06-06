using DS4Bridge.Core.Bridge;
using DS4Bridge.Core.Mapping;
using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace DS4Bridge.Virtual;

public sealed class VirtualXboxController : IVirtualXboxController
{
    private readonly ViGEmClient _client;
    private readonly IXbox360Controller _controller;
    private readonly ILogger<VirtualXboxController> _logger;
    private bool _connected;
    private bool _disposed;

    public event EventHandler<Ds4FeedbackEvent>? FeedbackReceived;

    public VirtualXboxController(ViGEmClient client, ILogger<VirtualXboxController> logger)
    {
        _client = client;
        _logger = logger;
        _controller = _client.CreateXbox360Controller();
        _controller.AutoSubmitReport = false;
        _controller.FeedbackReceived += OnFeedback;
    }

    private void OnFeedback(object? sender, Xbox360FeedbackReceivedEventArgs e)
    {
        FeedbackReceived?.Invoke(this, new Ds4FeedbackEvent(e.LargeMotor, e.SmallMotor));
    }

    public void Connect()
    {
        if (_connected) return;
        _controller.Connect();
        _connected = true;
        _logger.LogInformation("Virtual Xbox 360 controller connected");
    }

    public void Disconnect()
    {
        if (!_connected) return;
        try { _controller.Disconnect(); }
        catch (Exception ex) { _logger.LogWarning(ex, "Virtual disconnect threw"); }
        _connected = false;
        _logger.LogInformation("Virtual Xbox 360 controller disconnected");
    }

    public void SetButton(XboxButton button, bool pressed)
    {
        var target = Map(button);
        if (target is null) return;
        _controller.SetButtonState(target, pressed);
    }

    public void SetLeftThumb(short x, short y)
    {
        _controller.SetAxisValue(Xbox360Axis.LeftThumbX, x);
        _controller.SetAxisValue(Xbox360Axis.LeftThumbY, y);
    }

    public void SetRightThumb(short x, short y)
    {
        _controller.SetAxisValue(Xbox360Axis.RightThumbX, x);
        _controller.SetAxisValue(Xbox360Axis.RightThumbY, y);
    }

    public void SetLeftTrigger(byte value)  => _controller.SetSliderValue(Xbox360Slider.LeftTrigger, value);
    public void SetRightTrigger(byte value) => _controller.SetSliderValue(Xbox360Slider.RightTrigger, value);

    public void SetDpad(bool up, bool down, bool left, bool right)
    {
        _controller.SetButtonState(Xbox360Button.Up, up);
        _controller.SetButtonState(Xbox360Button.Down, down);
        _controller.SetButtonState(Xbox360Button.Left, left);
        _controller.SetButtonState(Xbox360Button.Right, right);
    }

    public void SubmitReport() => _controller.SubmitReport();

    private static Xbox360Button? Map(XboxButton b) => b switch
    {
        XboxButton.A             => Xbox360Button.A,
        XboxButton.B             => Xbox360Button.B,
        XboxButton.X             => Xbox360Button.X,
        XboxButton.Y             => Xbox360Button.Y,
        XboxButton.LeftShoulder  => Xbox360Button.LeftShoulder,
        XboxButton.RightShoulder => Xbox360Button.RightShoulder,
        XboxButton.Back          => Xbox360Button.Back,
        XboxButton.Start         => Xbox360Button.Start,
        XboxButton.LeftThumb     => Xbox360Button.LeftThumb,
        XboxButton.RightThumb    => Xbox360Button.RightThumb,
        XboxButton.Guide         => Xbox360Button.Guide,
        XboxButton.DpadUp        => Xbox360Button.Up,
        XboxButton.DpadDown      => Xbox360Button.Down,
        XboxButton.DpadLeft      => Xbox360Button.Left,
        XboxButton.DpadRight     => Xbox360Button.Right,
        _ => null
    };

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _controller.FeedbackReceived -= OnFeedback;
        Disconnect();
    }
}
