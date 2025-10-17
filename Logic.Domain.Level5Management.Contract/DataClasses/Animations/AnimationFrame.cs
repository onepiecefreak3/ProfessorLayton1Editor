using Logic.Domain.Level5Management.Contract.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Logic.Domain.Level5Management.Contract.DataClasses.Animations;

public class AnimationFrame
{
    public required Image<Rgba32> Image { get; set; }
    public required AnimationFrameErrorType Errors { get; set; }
}