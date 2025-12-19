using System.Reflection;
using ImGui.Forms.Resources;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Resources;

class ImageProvider : IImageProvider
{
    private const string IconResource_ = "UI.Layton1Tool.Resources.resources.images.layton1tool.png";
    private const string CloseResource_ = "UI.Layton1Tool.Resources.resources.images.close.png";
    private const string AddResource_ = "UI.Layton1Tool.Resources.resources.images.add.png";
    private const string SaveResource_ = "UI.Layton1Tool.Resources.resources.images.save.png";
    private const string SaveAsResource_ = "UI.Layton1Tool.Resources.resources.images.save_as.png";
    private const string PlayResource_ = "UI.Layton1Tool.Resources.resources.images.play.png";
    private const string PauseResource_ = "UI.Layton1Tool.Resources.resources.images.pause.png";
    private const string FrameBackResource_ = "UI.Layton1Tool.Resources.resources.images.frame_backward.png";
    private const string StepBackResource_ = "UI.Layton1Tool.Resources.resources.images.step_backward.png";
    private const string FrameForwardResource_ = "UI.Layton1Tool.Resources.resources.images.frame_forward.png";
    private const string StepForwardResource_ = "UI.Layton1Tool.Resources.resources.images.step_forward.png";
    private const string SettingsResource_ = "UI.Layton1Tool.Resources.resources.images.settings.png";
    private const string FontRemoveResource_ = "UI.Layton1Tool.Resources.resources.images.font_remove.png";
    private const string FontEditResource_ = "UI.Layton1Tool.Resources.resources.images.font_edit.png";
    private const string FontRemapResource_ = "UI.Layton1Tool.Resources.resources.images.font_remap.png";
    private const string ImageExportResource_ = "UI.Layton1Tool.Resources.resources.images.image_export.png";

    public Image<Rgba32> Icon => FromResource(IconResource_);
    public ThemedImageResource SearchClear => new(GetImageResource(CloseResource_), GetImageResource(CloseResource_));
    public ThemedImageResource Add => new(GetImageResource(AddResource_), GetImageResource(AddResource_));
    public ThemedImageResource Save => new(GetImageResource(SaveResource_), GetImageResource(SaveResource_));
    public ThemedImageResource SaveAs => new(GetImageResource(SaveAsResource_), GetImageResource(SaveAsResource_));
    public ThemedImageResource Play => new(GetImageResource(PlayResource_), GetImageResource(PlayResource_));
    public ThemedImageResource Pause => new(GetImageResource(PauseResource_), GetImageResource(PauseResource_));
    public ThemedImageResource FrameBack => new(GetImageResource(FrameBackResource_), GetImageResource(FrameBackResource_));
    public ThemedImageResource StepBack => new(GetImageResource(StepBackResource_), GetImageResource(StepBackResource_));
    public ThemedImageResource FrameForward => new(GetImageResource(FrameForwardResource_), GetImageResource(FrameForwardResource_));
    public ThemedImageResource StepForward => new(GetImageResource(StepForwardResource_), GetImageResource(StepForwardResource_));
    public ThemedImageResource Settings => new(GetImageResource(SettingsResource_), GetImageResource(SettingsResource_));
    public ThemedImageResource FontRemove => new(GetImageResource(FontRemoveResource_), GetImageResource(FontRemoveResource_));
    public ThemedImageResource FontEdit => new(GetImageResource(FontEditResource_), GetImageResource(FontEditResource_));
    public ThemedImageResource FontRemap => new(GetImageResource(FontRemapResource_), GetImageResource(FontRemapResource_));
    public ThemedImageResource ImageExport => new(GetImageResource(ImageExportResource_), GetImageResource(ImageExportResource_));

    private static ImageResource GetImageResource(string name)
    {
        return ImageResource.FromResource(Assembly.GetAssembly(typeof(ImageProvider))!, name);
    }

    private static Image<Rgba32> FromResource(string name)
    {
        Stream? resourceStream = Assembly.GetAssembly(typeof(ImageProvider))!.GetManifestResourceStream(name);
        if (resourceStream is null)
            throw new InvalidOperationException($"Could not find resource with name {name}.");

        return Image.Load<Rgba32>(resourceStream);
    }
}