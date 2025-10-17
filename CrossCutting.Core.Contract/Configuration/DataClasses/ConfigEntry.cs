using System.Text.Json.Serialization;

namespace CrossCutting.Core.Contract.Configuration.DataClasses;

public class ConfigEntry
{
    [JsonIgnore]
    public ConfigCategory Category { get; set; }

    public string Key { get; set; }
    public object Value { get; set; }

    public ConfigEntry(ConfigCategory category)
    {
        Category = category;
    }
}