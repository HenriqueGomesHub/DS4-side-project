using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DS4Bridge.App.Services;

// Heuristic: if Steam's running and we just lost a DS4 read, the user may
// have Steam Input grabbing it. We only check periodically; this isn't a
// hard guarantee, just a hint shown in the UI / logs.
public sealed class SteamConflictDetector
{
    private readonly ILogger<SteamConflictDetector> _logger;

    public SteamConflictDetector(ILogger<SteamConflictDetector> logger) => _logger = logger;

    public bool IsSteamRunning()
    {
        try
        {
            return Process.GetProcessesByName("steam").Length > 0
                || Process.GetProcessesByName("steamwebhelper").Length > 0;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to enumerate Steam processes");
            return false;
        }
    }

    public string FormatHint() =>
        "Steam appears to be running. If your DS4 is recognized but inputs go to the wrong place, " +
        "right-click the controller in Steam > Settings > Controller and disable Steam Input for this device.";
}
