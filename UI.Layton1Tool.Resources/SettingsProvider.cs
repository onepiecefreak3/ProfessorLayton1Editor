using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Resources;

class SettingsProvider(CrossCutting.Core.Contract.Settings.ISettingsProvider settings) : ISettingsProvider
{
    public string GetLocale()
    {
        return settings.Get("Layton1Tool.Settings.Locale", string.Empty);
    }

    public void SetLocale(string locale)
    {
        settings.Set("Layton1Tool.Settings.Locale", locale);
    }

    public string GetOpenDirectory()
    {
        return settings.Get("Layton1Tool.Settings.OpenDirectory", string.Empty);
    }

    public void SetOpenDirectory(string directory)
    {
        settings.Set("Layton1Tool.Settings.OpenDirectory", directory);
    }

    public string GetSaveDirectory()
    {
        return settings.Get("Layton1Tool.Settings.SaveDirectory", string.Empty);
    }

    public void SetSaveDirectory(string directory)
    {
        settings.Set("Layton1Tool.Settings.SaveDirectory", directory);
    }

    public string GetExtractDirectory()
    {
        return settings.Get("Layton1Tool.Settings.NdsDirectory", string.Empty);
    }

    public void SetExtractDirectory(string directory)
    {
        settings.Set("Layton1Tool.Settings.NdsDirectory", directory);
    }
}