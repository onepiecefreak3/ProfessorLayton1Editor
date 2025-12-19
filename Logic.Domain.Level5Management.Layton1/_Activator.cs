using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Domain.Level5Management.Contract.Script.Gds;
using Logic.Domain.Level5Management.Layton1.Script.Gds;

namespace Logic.Domain.Level5Management.Layton1;

public class Level5ManagementLayton1Activator : IComponentActivator
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
        kernel.Register<IGdsScriptParser, GdsScriptParser>(ActivationScope.Unique);
        kernel.Register<IGdsScriptComposer, GdsScriptComposer>(ActivationScope.Unique);

        kernel.RegisterConfiguration<Level5ManagementLayton1Configuration>();
    }

    public void AddMessageSubscriptions(IEventBroker broker)
    {
    }

    public void Configure(IConfigurator configurator)
    {
    }
}