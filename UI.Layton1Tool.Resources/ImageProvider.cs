using System.Reflection;
using ImGui.Forms.Resources;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Resources;

class ImageProvider : IImageProvider
{
    private const string CloseResource_ = "UI.Layton1Tool.Resources.resources.images.close.png";
    private const string SaveResource_ = "UI.Layton1Tool.Resources.resources.images.save.png";
    private const string SaveAsResource_ = "UI.Layton1Tool.Resources.resources.images.save_as.png";

    public ThemedImageResource SearchClear => new(GetImageResource(CloseResource_), GetImageResource(CloseResource_));
    public ThemedImageResource Save => new(GetImageResource(SaveResource_), GetImageResource(SaveResource_));
    public ThemedImageResource SaveAs => new(GetImageResource(SaveAsResource_), GetImageResource(SaveAsResource_));

    private static ImageResource GetImageResource(string name)
    {
        return ImageResource.FromResource(Assembly.GetAssembly(typeof(ImageProvider))!, name);
    }
}