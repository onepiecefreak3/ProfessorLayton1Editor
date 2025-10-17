using Logic.Domain.Level5Management.Contract.DataClasses.Animations;

namespace Logic.Domain.Level5Management.Contract.Animations;

public interface IFrame3Parser
{
    AnimationSequences Parse(Stream input);
    AnimationSequences Parse(Frame3Container container);
}