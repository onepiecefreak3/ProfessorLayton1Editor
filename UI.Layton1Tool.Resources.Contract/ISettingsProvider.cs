namespace UI.Layton1Tool.Resources.Contract;

public interface ISettingsProvider
{
    string Locale { get; set; }
    string OpenDirectory { get; set; }
    string SaveDirectory { get; set; }
    string ExtractDirectory { get; set; }
    string PreviewDirectory { get; set; }
    bool ReplaceFontCharacters { get; set; }
}