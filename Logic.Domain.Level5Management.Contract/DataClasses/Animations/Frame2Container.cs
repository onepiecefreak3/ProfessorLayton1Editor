namespace Logic.Domain.Level5Management.Contract.DataClasses.Animations;

public class Frame2Container
{
    public required int ImageFormat { get; set; }
    public required Frame2ImageEntry[] ImageEntries { get; set; }
    public required byte[] PaletteData { get; set; }
    public required string[] AnimationNames { get; set; }
    public required AnimationEntry[] AnimationEntries { get; set; }
}