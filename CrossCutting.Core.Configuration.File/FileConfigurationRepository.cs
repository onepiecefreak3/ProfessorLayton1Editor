using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace CrossCutting.Core.Configuration.File;

public class FileConfigurationRepository : IConfigurationRepository
{
    public IEnumerable<ConfigCategory> Load()
    {
        string cfgPath = GetConfigPath();
        if (!System.IO.File.Exists(cfgPath))
            yield break;

        string json = System.IO.File.ReadAllText(cfgPath);
        if (string.IsNullOrEmpty(json))
            yield break;

        var result = JsonSerializer.Deserialize<List<ConfigCategory>>(json);

        foreach (ConfigCategory category in result)
        {
            foreach (ConfigEntry entry in category.Entries)
                entry.Category = category;

            yield return category;
        }
    }

    private string GetConfigPath()
    {
        return Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "config.json");
    }
}