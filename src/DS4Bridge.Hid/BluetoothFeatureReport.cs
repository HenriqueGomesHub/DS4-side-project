using HidSharp;
using Microsoft.Extensions.Logging;

namespace DS4Bridge.Hid;

internal static class BluetoothFeatureReport
{
    // Sending feature report 0x02 forces the DS4 to switch to the full 78-byte
    // input report. Without it Bluetooth only gives a stripped 10-byte report.
    public static bool EnableFullReport(HidStream stream, ILogger logger)
    {
        // 38-byte buffer expected by DS4. Buffer[0] is report id.
        var buffer = new byte[38];
        buffer[0] = 0x02;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                stream.GetFeature(buffer);
                logger.LogInformation("Bluetooth full-report enabled (attempt {Attempt})", attempt);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "BT enable-report attempt {Attempt} failed", attempt);
                Thread.Sleep(100);
            }
        }
        logger.LogWarning("BT enable-report failed after 3 attempts");
        return false;
    }
}
