using UI.Layton1Tool.Components.Contract.DataClasses;
using UI.Layton1Tool.Components.Contract.Enums;

namespace UI.Layton1Tool.Components.Contract;

public interface IAnimationStateManager
{
    void Step(AnimationState state);
    void Increment(AnimationState state, AnimationUnit unit);
    void Decrement(AnimationState state, AnimationUnit unit);
}