using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Exceptions;

namespace DS4Bridge.App.Services;

public sealed class ViGEmBusDetector
{
    private readonly ILogger<ViGEmBusDetector> _logger;

    public ViGEmBusDetector(ILogger<ViGEmBusDetector> logger) => _logger = logger;

    // Lightweight probe: instantiate a client and dispose it. If the driver
    // is missing, VigemBusNotFoundException is thrown immediately.
    public bool IsInstalled()
    {
        try
        {
            using var client = new ViGEmClient();
            return true;
        }
        catch (VigemBusNotFoundException ex)
        {
            _logger.LogWarning(ex, "ViGEmBus driver not detected");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error probing ViGEmBus");
            return false;
        }
    }
}
