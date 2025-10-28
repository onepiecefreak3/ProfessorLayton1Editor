using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Models;
using System.Numerics;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Forms;

partial class ImageForm : Component
{
    private ImageButton _extractButton;
    private ZoomablePictureBox _zoomableImage;

    private StackLayout _mainLayout;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent(IImageProvider images, ILocalizationProvider localizations)
    {
        _zoomableImage = new ZoomablePictureBox();
        _zoomableImage.Zoom(2f);

        _extractButton = new ImageButton(images.ImageExport)
        {
            Enabled = false,
            ImageSize = new Vector2(16),
            Tooltip = localizations.ImageExportText
        };

        _mainLayout = new StackLayout
        {
            Size = Size.Parent,
            Alignment = Alignment.Vertical,
            ItemSpacing = 5,
            Items =
            {
                _extractButton,
                _zoomableImage
            }
        };
    }
}