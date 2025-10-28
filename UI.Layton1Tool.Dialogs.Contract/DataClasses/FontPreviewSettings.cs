using Kaligraphy.Enums.Layout;

namespace UI.Layton1Tool.Dialogs.Contract.DataClasses;

public class FontPreviewSettings
{
    public bool ShowDebugBoxes { get; set; }
    public int Spacing { get; set; } = 1;
    public int LineHeight { get; set; }
    public HorizontalTextAlignment HorizontalAlignment { get; set; } = HorizontalTextAlignment.Left;
}