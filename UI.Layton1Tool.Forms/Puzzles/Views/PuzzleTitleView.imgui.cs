using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Models;
using ImGui.Forms.Support;

namespace UI.Layton1Tool.Forms.Puzzles.Views;

internal partial class PuzzleTitleView : Component
{
    private ZoomablePictureBox _titleImageBox;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _titleImageBox.Update(contentRect);
    }

    private void InitializeComponent()
    {
        _titleImageBox = new ZoomablePictureBox { ShowBorder = true };
    }
}