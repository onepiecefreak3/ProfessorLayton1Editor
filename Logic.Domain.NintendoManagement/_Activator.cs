using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Domain.NintendoManagement.Contract;
using Logic.Domain.NintendoManagement.InternalContract;

namespace Logic.Domain.NintendoManagement;

public class NintendoManagementActivator : IComponentActivator
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
        kernel.Register<INdsReader, NdsReader>(ActivationScope.Unique);
        kernel.Register<INdsWriter, NdsWriter>(ActivationScope.Unique);
        kernel.Register<INdsFntReader, NdsFntReader>(ActivationScope.Unique);
        kernel.Register<INdsFntWriter, NdsFntWriter>(ActivationScope.Unique);
        kernel.Register<INintendoDecompressor, NintendoDecompressor>(ActivationScope.Unique);

        kernel.RegisterConfiguration<NintendoManagementConfiguration>();
    }

    public void AddMessageSubscriptions(IEventBroker broker)
    {
    }

    public void Configure(IConfigurator configurator)
    {
    }
}