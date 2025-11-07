using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Domain.Level5Management.Animations;
using Logic.Domain.Level5Management.Archives;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.Level5Management.Contract.Animations;
using Logic.Domain.Level5Management.Contract.Archives;
using Logic.Domain.Level5Management.Contract.Images;
using Logic.Domain.Level5Management.Contract.Script.Gds;
using Logic.Domain.Level5Management.Images;
using Logic.Domain.Level5Management.Script.Gds;

namespace Logic.Domain.Level5Management;

public class Level5ManagementActivator : IComponentActivator
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
        kernel.Register<IGdsScriptReader, GdsScriptReader>(ActivationScope.Unique);
        kernel.Register<IGdsScriptParser, GdsScriptParser>(ActivationScope.Unique);
        kernel.Register<IGdsScriptComposer, GdsScriptComposer>(ActivationScope.Unique);
        kernel.Register<IGdsScriptWriter, GdsScriptWriter>(ActivationScope.Unique);

        kernel.Register<IBgxReader, BgxReader>(ActivationScope.Unique);
        kernel.Register<IBgxParser, BgxParser>(ActivationScope.Unique);

        kernel.Register<IPcmReader, PcmReader>(ActivationScope.Unique);
        kernel.Register<IPcmWriter, PcmWriter>(ActivationScope.Unique);

        kernel.Register<IFrame1Reader, Frame1Reader>(ActivationScope.Unique);
        kernel.Register<IFrame1Parser, Frame1Parser>(ActivationScope.Unique);

        kernel.Register<IFrame2Reader, Frame2Reader>(ActivationScope.Unique);
        kernel.Register<IFrame2Parser, Frame2Parser>(ActivationScope.Unique);

        kernel.Register<IFrame3Reader, Frame3Reader>(ActivationScope.Unique);
        kernel.Register<IFrame3Parser, Frame3Parser>(ActivationScope.Unique);

        kernel.Register<ILevel5Decompressor, Level5Decompressor>(ActivationScope.Unique);

        kernel.RegisterConfiguration<Level5ManagementConfiguration>();
    }

    public void AddMessageSubscriptions(IEventBroker broker)
    {
    }

    public void Configure(IConfigurator configurator)
    {
    }
}