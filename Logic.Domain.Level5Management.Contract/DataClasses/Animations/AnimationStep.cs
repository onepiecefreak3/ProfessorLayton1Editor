namespace Logic.Domain.Level5Management.Contract.DataClasses.Animations;

public class AnimationStep
{
    public required int NextStepIndex { get; set; }
    public required int FrameCounter { get; set; }
    public required int FrameIndex { get; set; }
}