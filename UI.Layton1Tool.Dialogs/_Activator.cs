using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using UI.Layton1Tool.Dialogs.Contract;

namespace UI.Layton1Tool.Dialogs;

public class Layton1ToolDialogsActivator : IComponentActivator
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
        kernel.Register<IDialogFactory, DialogFactory>(ActivationScope.Unique);

        kernel.RegisterToSelf<ValidationDialog>();
        kernel.RegisterToSelf<SearchDialog>();
        kernel.RegisterToSelf<FontPreviewSettingsDialog>();
        kernel.RegisterToSelf<FontGenerationDialog>();
        kernel.RegisterToSelf<FontRemappingDialog>();

        kernel.RegisterConfiguration<Layton1ToolDialogsConfiguration>();
    }

    public void AddMessageSubscriptions(IEventBroker broker)
    {
    }

    public void Configure(IConfigurator configurator)
    {
    }
}