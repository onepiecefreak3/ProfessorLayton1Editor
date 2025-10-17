using System.Text.Json;
using ImGui.Forms.Localization;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Resources;

internal class Localizer : BaseLocalizer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private const string NameValue_ = "Name";

    private readonly Layton1ToolResourcesConfiguration _config;
    private readonly ISettingsProvider _settings;

    protected override string UndefinedValue => "<undefined>";
    protected override string DefaultLocale => _config.DefaultLocale;

    public Localizer(Layton1ToolResourcesConfiguration config, ISettingsProvider settings)
    {
        _config = config;
        _settings = settings;

        Initialize();
    }

    protected override IList<LanguageInfo> InitializeLocalizations()
    {
        string? applicationDirectory = Path.GetDirectoryName(Environment.ProcessPath);
        if (string.IsNullOrEmpty(applicationDirectory))
            return Array.Empty<LanguageInfo>();

        string localeDirectory = Path.Combine(applicationDirectory, _config.LocalizationPath);
        if (!Directory.Exists(localeDirectory))
            return Array.Empty<LanguageInfo>();

        string[] localeFiles = Directory.GetFiles(localeDirectory);

        var result = new List<LanguageInfo>();
        foreach (string localeFile in localeFiles)
        {
            // Read text from stream
            string json = File.ReadAllText(localeFile);

            // Deserialize JSON
            var entries = JsonSerializer.Deserialize<IDictionary<string, string>>(json, JsonOptions);
            if (entries == null || !entries.TryGetValue(NameValue_, out string? name))
                continue;

            string locale = Path.GetFileNameWithoutExtension(localeFile);
            result.Add(new LanguageInfo(locale, name, entries));
        }

        return result;
    }

    protected override string InitializeLocale()
    {
        return _settings.GetLocale();
    }

    protected override void SetCurrentLocale(string locale)
    {
        base.SetCurrentLocale(locale);

        _settings.SetLocale(locale);
    }
}