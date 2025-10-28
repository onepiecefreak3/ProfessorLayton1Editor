using ImGui.Forms.Resources;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;

namespace UI.Layton1Tool.Components.Contract.DataClasses;

public class AnimationState
{
    public required AnimationSequences Animations { get; init; }
    public required ImageResource[] Images { get; init; }

    public AnimationSequence? ActiveAnimation { get; set; }

    public float TotalFrameCounter { get; set; }
    public float FrameCounter { get; set; }
    public int StepCounter { get; set; }

    public float FrameSpeed { get; set; }
}