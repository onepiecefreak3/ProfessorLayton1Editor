using ImGui.Forms.Resources;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;

namespace UI.Layton1Tool.Forms.DataClasses;

class AnimationState
{
    public required AnimationSequences Animations { get; init; }
    public required ImageResource[] Images { get; init; }

    public AnimationSequence? ActiveAnimation { get; set; }
    public int FrameCounter { get; set; }
    public int StepCounter { get; set; }
}