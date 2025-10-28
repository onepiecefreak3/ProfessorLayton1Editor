using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace UI.Layton1Tool.Messages;

public record SelectedImageChangedMessage(Layton1NdsRom Rom, Image<Rgba32> Image);