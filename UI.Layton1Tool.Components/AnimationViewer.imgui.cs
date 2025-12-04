using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Resources;
using UI.Layton1Tool.Components.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using Rectangle = Veldrid.Rectangle;
using Size = ImGui.Forms.Models.Size;

namespace UI.Layton1Tool.Components;

partial class AnimationViewer : Component
{
    private StackLayout _mainLayout;
    private ZoomablePictureBox _zoomableImage;
    private Component _player;

    public override Size GetSize() => Size.Parent;

    private void InitializeComponent(Layton1NdsInfo ndsInfo, IComponentFactory components)
    {
        _zoomableImage = new ZoomablePictureBox { ShowBorder = true, ShowImageBorder = true };
        _zoomableImage.Zoom(2f);

        _player = components.CreateAnimationPlayer(ndsInfo);

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                _zoomableImage,
                _player
            }
        };
    }

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void SetFrame(ImageResource image)
    {
        _zoomableImage.SetImage(image, false);
    }
}