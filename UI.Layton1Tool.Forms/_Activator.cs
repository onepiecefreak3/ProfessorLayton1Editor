using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.InternalContract;

namespace UI.Layton1Tool.Forms;

public class Layton1ToolFormsActivator : IComponentActivator
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
        kernel.Register<IFormFactory, FormFactory>(ActivationScope.Unique);
        kernel.Register<IPositionManager, PositionManager>(ActivationScope.Unique);
        kernel.Register<IUnicodeCharacterParser, UnicodeCharacterParser>(ActivationScope.Unique);
        kernel.Register<IFileHistory, FileHistory>();

        kernel.RegisterToSelf<MainForm>();
        kernel.RegisterToSelf<NdsForm>();
        kernel.RegisterToSelf<PuzzleForm>();
        kernel.RegisterToSelf<ImageForm>();
        kernel.RegisterToSelf<GdsForm>();
        kernel.RegisterToSelf<PcmForm>();
        kernel.RegisterToSelf<AnimationForm>();
        kernel.RegisterToSelf<TextForm>();
        kernel.RegisterToSelf<FontForm>();
        kernel.RegisterToSelf<PuzzleInfoForm>();
        kernel.RegisterToSelf<PuzzleScriptForm>();

        kernel.RegisterConfiguration<Layton1ToolFormsConfiguration>();
    }

    public void AddMessageSubscriptions(IEventBroker broker)
    {
    }

    public void Configure(IConfigurator configurator)
    {
    }
}