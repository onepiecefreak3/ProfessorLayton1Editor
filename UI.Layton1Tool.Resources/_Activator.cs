using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Localization;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Resources;

public class Layton1ToolResourcesActivator : IComponentActivator
{
    public void Activating()
    {
    }

    public void Activated()
    {
    }

    public void Deactivating()
    {
    }

    public void Deactivated()
    {
    }

    public void Register(ICoCoKernel kernel)
    {
        kernel.Register<ILocalizer, Localizer>(ActivationScope.Unique);
        kernel.Register<IFontFactory, FontFactory>(ActivationScope.Unique);

        kernel.Register<ISettingsProvider, SettingsProvider>(ActivationScope.Unique);
        kernel.Register<ILocalizationProvider, LocalizationProvider>(ActivationScope.Unique);
        kernel.Register<IImageProvider, ImageProvider>(ActivationScope.Unique);
        kernel.Register<IColorProvider, ColorProvider>(ActivationScope.Unique);

        kernel.RegisterConfiguration<Layton1ToolResourcesConfiguration>();
    }

    public void AddMessageSubscriptions(IEventBroker broker)
    {
    }

    public void Configure(IConfigurator configurator)
    {
    }
}