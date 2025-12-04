using CrossCutting.Core.Contract.Settings;

namespace UI.Layton1Tool.Resources;

class SettingsProvider(ISettingsProvider settings) : Contract.ISettingsProvider
{
    private const string LocaleName_ = "Layton1Tool.Settings.Locale";
    private const string OpenDirectoryName_ = "Layton1Tool.Settings.OpenDirectory";
    private const string SaveDirectoryName_ = "Layton1Tool.Settings.SaveDirectory";
    private const string ExtractDirectoryName_ = "Layton1Tool.Settings.Extract.Directory";
    private const string ImportDirectoryName_ = "Layton1Tool.Settings.Import.Directory";
    private const string PreviewDirectoryName_ = "Layton1Tool.Settings.PreviewDirectory";
    private const string ReplaceFontCharactersName_ = "Layton1Tool.Settings.ReplaceFontCharacters";
    private const string RenderTextBoxesName_ = "Layton1Tool.Settings.RenderTextBoxes";
    private const string RenderHintBoxesName_ = "Layton1Tool.Settings.RenderHintBoxes";
    private const string RenderObjectBoxesName_ = "Layton1Tool.Settings.RenderObjectBoxes";
    private const string RenderMovementArrowsName_ = "Layton1Tool.Settings.RenderMovementArrows";

    public string Locale
    {
        get => settings.Get(LocaleName_, string.Empty);
        set => settings.Set(LocaleName_, value);
    }

    public string OpenDirectory
    {
        get => settings.Get(OpenDirectoryName_, string.Empty);
        set => settings.Set(OpenDirectoryName_, value);
    }

    public string SaveDirectory
    {
        get => settings.Get(SaveDirectoryName_, string.Empty);
        set => settings.Set(SaveDirectoryName_, value);
    }

    public string ExtractDirectory
    {
        get => settings.Get(ExtractDirectoryName_, string.Empty);
        set => settings.Set(ExtractDirectoryName_, value);
    }

    public string ImportDirectory
    {
        get => settings.Get(ImportDirectoryName_, string.Empty);
        set => settings.Set(ImportDirectoryName_, value);
    }

    public string PreviewDirectory
    {
        get => settings.Get(PreviewDirectoryName_, string.Empty);
        set => settings.Set(PreviewDirectoryName_, value);
    }

    public bool ReplaceFontCharacters
    {
        get => settings.Get(ReplaceFontCharactersName_, false);
        set => settings.Set(ReplaceFontCharactersName_, value);
    }

    public bool RenderTextBoxes
    {
        get => settings.Get(RenderTextBoxesName_, true);
        set => settings.Set(RenderTextBoxesName_, value);
    }

    public bool RenderHintBoxes
    {
        get => settings.Get(RenderHintBoxesName_, true);
        set => settings.Set(RenderHintBoxesName_, value);
    }

    public bool RenderObjectBoxes
    {
        get => settings.Get(RenderObjectBoxesName_, true);
        set => settings.Set(RenderObjectBoxesName_, value);
    }

    public bool RenderMovementArrows
    {
        get => settings.Get(RenderMovementArrowsName_, true);
        set => settings.Set(RenderMovementArrowsName_, value);
    }
}