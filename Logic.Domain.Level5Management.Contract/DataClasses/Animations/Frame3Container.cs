namespace Logic.Domain.Level5Management.Contract.DataClasses.Animations;

public class Frame3Container
{
    public required int ImageFormat { get; set; }
    public required Frame3ImageEntry[] ImageEntries { get; set; }
    public required string[] AnimationNames { get; set; }
    public required AnimationEntry[] AnimationEntries { get; set; }
}