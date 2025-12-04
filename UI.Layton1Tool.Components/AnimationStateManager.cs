using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using UI.Layton1Tool.Components.Contract;
using UI.Layton1Tool.Components.Contract.DataClasses;
using UI.Layton1Tool.Components.Contract.Enums;

namespace UI.Layton1Tool.Components;

internal class AnimationStateManager : IAnimationStateManager
{
    public void Step(AnimationState state)
    {
        float frameSpeed = state.FrameSpeed;
        while (frameSpeed > 0)
            frameSpeed -= Step(state, frameSpeed);
    }

    public void Increment(AnimationState state, AnimationUnit unit)
    {
        if (state.ActiveAnimation is null)
            return;

        switch (unit)
        {
            case AnimationUnit.Step:
                if (state.StepCounter + 1 >= state.ActiveAnimation.Steps.Length)
                    return;

                state.TotalFrameCounter += state.ActiveAnimation.Steps[state.StepCounter].FrameCounter - state.FrameCounter;
                state.FrameCounter = 0;
                state.StepCounter++;
                break;

            case AnimationUnit.Frame:
                int totalFrames = state.ActiveAnimation.Steps.Sum(x => x.FrameCounter);

                state.TotalFrameCounter = Math.Min(totalFrames - 1, state.TotalFrameCounter + 1);

                AnimationStep step = state.ActiveAnimation.Steps[state.StepCounter];

                if (state.FrameCounter + 1 >= step.FrameCounter)
                {
                    if (state.StepCounter + 1 >= state.ActiveAnimation.Steps.Length)
                        return;

                    state.StepCounter = Math.Min(state.ActiveAnimation.Steps.Length - 1, state.StepCounter + 1);
                    state.FrameCounter = 0;
                }
                else
                {
                    state.FrameCounter++;
                }
                break;
        }
    }

    public void Decrement(AnimationState state, AnimationUnit unit)
    {
        if (state.ActiveAnimation is null)
            return;

        switch (unit)
        {
            case AnimationUnit.Step:
                if (state.FrameCounter is not 0)
                {
                    state.TotalFrameCounter -= state.FrameCounter;
                    state.FrameCounter = 0;
                }
                else
                {
                    state.StepCounter = Math.Max(0, state.StepCounter - 1);
                    state.TotalFrameCounter -= state.ActiveAnimation.Steps[state.StepCounter].FrameCounter;
                }
                break;

            case AnimationUnit.Frame:
                state.TotalFrameCounter = Math.Max(0, state.TotalFrameCounter - 1);

                if (state.FrameCounter <= 0)
                {
                    if (state.StepCounter <= 0)
                        return;

                    AnimationStep step = state.ActiveAnimation.Steps[state.StepCounter - 1];

                    state.StepCounter--;
                    state.FrameCounter = step.FrameCounter;
                }

                state.FrameCounter--;
                break;
        }
    }

    private static float Step(AnimationState state, float speed)
    {
        if (state.ActiveAnimation is null)
            return speed;

        int stepIndex = state.StepCounter;
        if (stepIndex >= state.ActiveAnimation.Steps.Length)
            return speed;

        AnimationStep step = state.ActiveAnimation.Steps[stepIndex];

        var stepTotalFrames = 0;
        for (var i = 0; i < stepIndex; i++)
            stepTotalFrames += state.ActiveAnimation.Steps[i].FrameCounter;

        float maxFrameSpeed = Math.Min(speed, 1);

        state.FrameCounter += maxFrameSpeed;
        state.TotalFrameCounter = stepTotalFrames + state.FrameCounter;

        if (state.FrameCounter < step.FrameCounter)
            return maxFrameSpeed;

        state.StepCounter = step.NextStepIndex;
        state.FrameCounter = 0;

        return maxFrameSpeed;
    }
}