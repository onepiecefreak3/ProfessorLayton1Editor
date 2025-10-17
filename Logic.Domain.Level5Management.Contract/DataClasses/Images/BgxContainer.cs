namespace Logic.Domain.Level5Management.Contract.DataClasses.Images;

public class BgxContainer
{
    public required byte[] PaletteData { get; set; }
    public required byte[] TileData { get; set; }
    public required short[] TileIndexes { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
}