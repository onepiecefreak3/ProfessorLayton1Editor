using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Models;
using Veldrid;

namespace UI.Layton1Tool.Forms;

partial class BgxForm : Component
{
    private ZoomablePictureBox _zoomableImage;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _zoomableImage.Update(contentRect);
    }

    private void InitializeComponent()
    {
        _zoomableImage = new ZoomablePictureBox();
        _zoomableImage.Zoom(2f);
    }
}