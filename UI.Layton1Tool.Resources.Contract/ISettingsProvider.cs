namespace UI.Layton1Tool.Resources.Contract;

public interface ISettingsProvider
{
    string GetLocale();
    void SetLocale(string locale);

    string GetOpenDirectory();
    void SetOpenDirectory(string directory);

    string GetSaveDirectory();
    void SetSaveDirectory(string directory);

    string GetExtractDirectory();
    void SetExtractDirectory(string directory);
}