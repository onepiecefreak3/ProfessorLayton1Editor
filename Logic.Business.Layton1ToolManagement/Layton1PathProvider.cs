using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;

namespace Logic.Business.Layton1ToolManagement;

internal class Layton1PathProvider : ILayton1PathProvider
{
    public string GetFullDirectory(string path, GameVersion version, TextLanguage language)
    {
        string result = GetFullDirectory(path, version);

        string? languageDir = GetLanguageDirectory(version, language);
        if (!string.IsNullOrEmpty(languageDir))
            return result + languageDir;

        return result;
    }

    public string GetFullDirectory(string path, GameVersion version)
    {
        return GetDataDirectory(version) + path;
    }

    private static string? GetLanguageDirectory(GameVersion version, TextLanguage language)
    {
        if (version is GameVersion.Europe or GameVersion.EuropeDemo)
        {
            if (language is TextLanguage.Korean or TextLanguage.Japanese)
                return GetLanguageDirectory(TextLanguage.English);

            return GetLanguageDirectory(language);
        }

        return version switch
        {
            GameVersion.Usa or GameVersion.UsaDemo => GetLanguageDirectory(TextLanguage.English),
            GameVersion.Korea => GetLanguageDirectory(TextLanguage.Korean),
            GameVersion.JapanFriendly => GetLanguageDirectory(TextLanguage.Japanese),
            GameVersion.Japan => null,
            _ => throw new InvalidOperationException($"Unknown game version {version}.")
        };
    }

    private static string GetDataDirectory(GameVersion version)
    {
        return version switch
        {
            GameVersion.Europe => "data/",
            GameVersion.EuropeDemo => "data/",
            GameVersion.Usa => "data/",
            GameVersion.UsaDemo => "data/",
            GameVersion.Korea => "data/",
            GameVersion.Japan => "data/",
            GameVersion.JapanFriendly => "data_fr1/",
            _ => throw new InvalidOperationException($"Unknown game version {version}.")
        };
    }

    private static string GetLanguageDirectory(TextLanguage language)
    {
        return language switch
        {
            TextLanguage.English => "en/",
            TextLanguage.German => "de/",
            TextLanguage.Spanish => "es/",
            TextLanguage.French => "fr/",
            TextLanguage.Italian => "it/",
            TextLanguage.Korean => "ko/",
            TextLanguage.Japanese => "jp/",
            _ => throw new InvalidOperationException($"Unknown language {language}.")
        };
    }
}