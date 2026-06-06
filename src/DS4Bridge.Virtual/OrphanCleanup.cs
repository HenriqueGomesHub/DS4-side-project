using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Exceptions;

namespace DS4Bridge.Virtual;

public static class OrphanCleanup
{
    // ViGEmClient.Dispose() releases all controllers owned by the *current
    // process*. If a previous DS4Bridge process crashed, Windows already
    // released them when the process died — so the practical cleanup is just
    // proving we can construct a fresh client without errors. We surface any
    // VigemBusNotFoundException up to the caller.
    public static ViGEmClient ConstructClient(ILogger logger)
    {
        try
        {
            var client = new ViGEmClient();
            logger.LogInformation("ViGEmClient constructed; previous-process orphans (if any) already cleaned by OS");
            return client;
        }
        catch (VigemBusNotFoundException)
        {
            throw;
        }
    }
}
