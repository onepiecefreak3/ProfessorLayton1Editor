namespace Logic.Domain.Level5Management.DataClasses.Animations;

struct AnimationReaderContext
{
    public int FrameIndex { get; set; }
    public int FrameCount { get; set; }
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public int PartIndex { get; set; }
    public int PartCount { get; set; }
    public int ImageFormat { get; set; }
    public int ColorCount { get; set; }
    public bool IsValid { get; set; }
}