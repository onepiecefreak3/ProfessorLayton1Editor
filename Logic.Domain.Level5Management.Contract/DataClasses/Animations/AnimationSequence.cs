namespace Logic.Domain.Level5Management.Contract.DataClasses.Animations;

public class AnimationSequence
{
    public required string Name { get; set; }
    public required AnimationStep[] Steps { get; set; }
}