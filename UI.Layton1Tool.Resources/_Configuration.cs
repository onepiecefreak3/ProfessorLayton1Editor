using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace UI.Layton1Tool.Resources;

public class Layton1ToolResourcesConfiguration
{
    [ConfigMap("UI.Layton1Tool.Resources", "DefaultLocale")]
    public virtual string DefaultLocale { get; set; } = "en";

    [ConfigMap("UI.Layton1Tool.Resources", "LocalizationPath")]
    public virtual string LocalizationPath { get; set; } = "resources/langs";
}