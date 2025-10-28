namespace Logic.Domain.NintendoManagement.DataClasses.Font;

struct NftrCglpSection
{
    public byte cellWidth;
    public byte cellHeight;
    public short cellSize;
    public byte baseline;
    public byte maxCharWidth;
    public byte cellBitDepth;
    public byte cellRotation;
    public byte[][] cellData;
}