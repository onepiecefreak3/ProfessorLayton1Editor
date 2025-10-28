using CrossCutting.Core.Bootstrapping;
using CrossCutting.Core.Configuration;
using CrossCutting.Core.Configuration.CommandLine;
using CrossCutting.Core.Configuration.ConfigObjects;
using CrossCutting.Core.Configuration.File;
using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Logging;
using CrossCutting.Core.Contract.Settings;
using CrossCutting.Core.DI.AutofacAdapter;
using CrossCutting.Core.EventBrokerage;
using CrossCutting.Core.Logging.NLogAdapter;
using CrossCutting.Core.Settings;
using Logic.Business.Layton1ToolManagement;
using Logic.Domain.CodeAnalysisManagement;
using Logic.Domain.Level5Management;
using Logic.Domain.NintendoManagement;
using UI.Layton1Tool.Components;
using UI.Layton1Tool.Dialogs;
using UI.Layton1Tool.Forms;
using UI.Layton1Tool.Resources;

namespace Mappings.Layton1Tool;

public class KernelInitializer : IKernelInitializer
{
    private IKernelContainer _kernelContainer;

    public IKernelContainer CreateKernelContainer()
    {
        if (_kernelContainer == null)
        {
            _kernelContainer = new KernelContainer();
        }
        return _kernelContainer;
    }

    public void Initialize()
    {
        RegisterCoreComponents(_kernelContainer.Kernel);
        ActivateComponents(_kernelContainer.Kernel);
    }

    private void RegisterCoreComponents(ICoCoKernel kernel)
    {
        kernel.Register<IBootstrapper, Bootstrapper>(ActivationScope.Unique);
        kernel.Register<IEventBroker, EventBroker>(ActivationScope.Unique);
        kernel.Register<IConfigurationRepository, FileConfigurationRepository>();
        kernel.Register<IConfigurationRepository, CommandLineConfigurationRepository>();
        kernel.Register<IConfigurator, Configurator>(ActivationScope.Unique);
        kernel.Register<IConfigObjectProvider, ConfigObjectProvider>(ActivationScope.Unique);
        kernel.Register<ILogger, Logger>(ActivationScope.Unique);
        kernel.Register<ISettingsProvider, SettingsProvider>(ActivationScope.Unique);
    }

    private void ActivateComponents(ICoCoKernel kernel)
    {
        kernel.RegisterComponent<Layton1ToolManagementActivator>();
        kernel.RegisterComponent<CodeAnalysisManagementActivator>();
        kernel.RegisterComponent<Level5ManagementActivator>();
        kernel.RegisterComponent<NintendoManagementActivator>();
        kernel.RegisterComponent<Layton1ToolFormsActivator>();
        kernel.RegisterComponent<Layton1ToolDialogsActivator>();
        kernel.RegisterComponent<Layton1ToolComponentsActivator>();
        kernel.RegisterComponent<Layton1ToolResourcesActivator>();
    }
}