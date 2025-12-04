using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Models;
using Veldrid;

namespace UI.Layton1Tool.Forms.Puzzles.Views;

internal partial class PuzzleHint1View : Component
{
    private ZoomablePictureBox _indexImageBox;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _indexImageBox.Update(contentRect);
    }

    private void InitializeComponent()
    {
        _indexImageBox = new ZoomablePictureBox { ShowBorder = true };
    }
}