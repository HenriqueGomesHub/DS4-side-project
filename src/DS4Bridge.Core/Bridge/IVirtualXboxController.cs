using DS4Bridge.Core.Mapping;

namespace DS4Bridge.Core.Bridge;

public interface IVirtualXboxController : IDisposable
{
    void Connect();
    void Disconnect();
    void SetButton(XboxButton button, bool pressed);
    void SetLeftThumb(short x, short y);
    void SetRightThumb(short x, short y);
    void SetLeftTrigger(byte value);
    void SetRightTrigger(byte value);
    void SetDpad(bool up, bool down, bool left, bool right);
    void SubmitReport();
    event EventHandler<Ds4FeedbackEvent>? FeedbackReceived;
}
