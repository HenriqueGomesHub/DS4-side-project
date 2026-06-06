namespace DS4Bridge.Hid;

public static class Ds4DeviceIds
{
    public const int SonyVendorId = 0x054C;

    // Accepted product IDs. Order matters only for documentation; we match any.
    public static readonly int[] AcceptedProductIds =
    {
        0x05C4, // DS4 v1
        0x09CC, // DS4 v2
        0x0BA0  // DS4 USB wireless adapter
    };

    public static bool IsDs4(int vendorId, int productId) =>
        vendorId == SonyVendorId && Array.IndexOf(AcceptedProductIds, productId) >= 0;
}
