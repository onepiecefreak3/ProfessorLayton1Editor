﻿using CrossCutting.Core.Contract.Settings;

namespace UI.Layton1Tool.Resources;

class SettingsProvider(ISettingsProvider settings) : Contract.ISettingsProvider
{
    private const string LocaleName_ = "Layton1Tool.Settings.Locale";
    private const string OpenDirectoryName_ = "Layton1Tool.Settings.OpenDirectory";
    private const string SaveDirectoryName_ = "Layton1Tool.Settings.SaveDirectory";
    private const string ExtractDirectoryName_ = "Layton1Tool.Settings.NdsDirectory";
    private const string PreviewDirectoryName_ = "Layton1Tool.Settings.PreviewDirectory";
    private const string ReplaceFontCharactersName_ = "Layton1Tool.Settings.ReplaceFontCharacters";

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
}