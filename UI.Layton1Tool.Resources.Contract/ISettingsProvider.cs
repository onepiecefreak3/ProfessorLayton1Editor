namespace UI.Layton1Tool.Resources.Contract;

public interface ISettingsProvider
{
    string Locale { get; set; }

    string OpenDirectory { get; set; }
    string SaveDirectory { get; set; }
    string ExtractDirectory { get; set; }
    string ImportDirectory { get; set; }
    string PreviewDirectory { get; set; }

    bool ReplaceFontCharacters { get; set; }

    bool RenderTextBoxes { get; set; }
    bool RenderHintBoxes { get; set; }
    bool RenderObjectBoxes { get; set; }
    bool RenderMovementArrows { get; set; }
}