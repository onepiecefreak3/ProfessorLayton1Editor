using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace Logic.Business.Layton1ToolManagement;

public class Layton1ToolManagementConfiguration
{
    [ConfigMap("Logic.Business.Layton1ToolManagement", "MethodMappingPath")]
    public virtual string MethodMappingPath { get; set; } = "methodDescriptions.json";
}