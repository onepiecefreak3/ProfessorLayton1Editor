using ImGui.Forms.Resources;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace UI.Layton1Tool.Resources.Contract;

public interface IImageProvider
{
    Image<Rgba32> Icon { get; }
    ThemedImageResource SearchClear { get; }
    ThemedImageResource Add { get; }
    ThemedImageResource Save { get; }
    ThemedImageResource SaveAs { get; }
    ThemedImageResource Play { get; }
    ThemedImageResource Pause { get; }
    ThemedImageResource FrameBack { get; }
    ThemedImageResource StepBack { get; }
    ThemedImageResource FrameForward { get; }
    ThemedImageResource StepForward { get; }
    ThemedImageResource Settings { get; }
    ThemedImageResource FontRemove { get; }
    ThemedImageResource FontEdit { get; }
    ThemedImageResource FontRemap { get; }
    ThemedImageResource ImageExport { get; }
}