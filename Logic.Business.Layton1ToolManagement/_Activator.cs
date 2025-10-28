using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.Layton1ToolManagement.Compression;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Business.Layton1ToolManagement.Contract.Validation;
using Logic.Business.Layton1ToolManagement.Files;
using Logic.Business.Layton1ToolManagement.InternalContract.Compression;
using Logic.Business.Layton1ToolManagement.InternalContract.Files;
using Logic.Business.Layton1ToolManagement.InternalContract.Scripts;
using Logic.Business.Layton1ToolManagement.InternalContract.Validation;
using Logic.Business.Layton1ToolManagement.Scripts;
using Logic.Business.Layton1ToolManagement.Validation;

namespace Logic.Business.Layton1ToolManagement;

public class Layton1ToolManagementActivator : IComponentActivator
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
        kernel.Register<ILayton1NdsParser, Layton1NdsParser>(ActivationScope.Unique);
        kernel.Register<ILayton1NdsComposer, Layton1NdsComposer>(ActivationScope.Unique);
        kernel.Register<ILayton1CompressionDetector, Layton1CompressionDetector>(ActivationScope.Unique);
        kernel.Register<ILayton1GameCodeValidator, Layton1GameCodeValidator>(ActivationScope.Unique);
        kernel.Register<ILayton1NdsFileManager, Layton1NdsFileManager>(ActivationScope.Unique);
        kernel.Register<ILayton1PcmFileManager, Layton1PcmFileManager>(ActivationScope.Unique);
        kernel.Register<ILayton1NdsValidator, Layton1NdsValidator>(ActivationScope.Unique);
        kernel.Register<ILayton1FileTypeDetector, Layton1FileTypeDetector>(ActivationScope.Unique);
        kernel.Register<ILayton1Compressor, Layton1Compressor>(ActivationScope.Unique);
        kernel.Register<ILayton1FileParser, Layton1FileParser>(ActivationScope.Unique);
        kernel.Register<ILayton1FileComposer, Layton1FileComposer>(ActivationScope.Unique);

        kernel.Register<ILayton1ScriptFileConverter, Layton1ScriptFileConverter>(ActivationScope.Unique);
        kernel.Register<ILayton1ScriptCodeUnitConverter, Layton1ScriptCodeUnitConverter>(ActivationScope.Unique);
        kernel.Register<ILayton1ScriptConverter, Layton1ScriptConverter>(ActivationScope.Unique);

        kernel.Register<ILayton1ScriptInstructionDescriptionProvider, Layton1ScriptInstructionDescriptionProvider>(ActivationScope.Unique);
        kernel.Register<ILayton1ScriptInstructionManager, Layton1ScriptInstructionManager>(ActivationScope.Unique);

        kernel.RegisterConfiguration<Layton1ToolManagementConfiguration>();
    }

    public void AddMessageSubscriptions(IEventBroker broker)
    {
    }

    public void Configure(IConfigurator configurator)
    {
    }
}