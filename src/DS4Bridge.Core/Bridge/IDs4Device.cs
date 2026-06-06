using DS4Bridge.Core.Models;

namespace DS4Bridge.Core.Bridge;

public interface IDs4Device : IDisposable
{
    string DevicePath { get; }
    ConnectionMode Mode { get; }
    bool IsOpen { get; }
    event EventHandler<Ds4InputState>? InputReceived;
    event EventHandler<Exception>? ReadFailed;
    void Start(CancellationToken token);
    void WriteOutput(Ds4OutputState output);
}
