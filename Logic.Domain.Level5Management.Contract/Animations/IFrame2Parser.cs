using Logic.Domain.Level5Management.Contract.DataClasses.Animations;

namespace Logic.Domain.Level5Management.Contract.Animations;

public interface IFrame2Parser
{
    AnimationSequences Parse(Stream input);
    AnimationSequences Parse(Frame2Container container);
}