namespace DS4Bridge.Core.Models;

// Encoded as a 4-bit enum value in the DS4 input report (byte 4 low nibble),
// not a bitfield. 8 = neutral.
public enum DpadDirection : byte
{
    North = 0,
    NorthEast = 1,
    East = 2,
    SouthEast = 3,
    South = 4,
    SouthWest = 5,
    West = 6,
    NorthWest = 7,
    Neutral = 8
}
