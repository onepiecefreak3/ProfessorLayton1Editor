using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Files;
using UI.Layton1Tool.Forms.InternalContract;
using UI.Layton1Tool.Forms.Puzzles;
using UI.Layton1Tool.Forms.Puzzles.Views;
using UI.Layton1Tool.Forms.Rooms;
using UI.Layton1Tool.Forms.Rooms.Views;
using UI.Layton1Tool.Forms.Text;

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
        kernel.Register<IFontProvider, FontProvider>(ActivationScope.Unique);

        kernel.RegisterToSelf<MainForm>();
        kernel.RegisterToSelf<NdsForm>();
        kernel.RegisterToSelf<ImageForm>();
        kernel.RegisterToSelf<GdsForm>();
        kernel.RegisterToSelf<PcmForm>();
        kernel.RegisterToSelf<AnimationForm>();
        kernel.RegisterToSelf<TextForm>();
        kernel.RegisterToSelf<FontForm>();

        kernel.RegisterToSelf<PuzzleForm>();
        kernel.RegisterToSelf<PuzzleTitleForm>();
        kernel.RegisterToSelf<PuzzleTextForm>();
        kernel.RegisterToSelf<PuzzleHintForm>();
        kernel.RegisterToSelf<PuzzleScriptForm>();
        kernel.RegisterToSelf<PuzzleTitleView>();
        kernel.RegisterToSelf<PuzzleIndexView>();
        kernel.RegisterToSelf<PuzzleDescriptionView>();
        kernel.RegisterToSelf<PuzzleCorrectView>();
        kernel.RegisterToSelf<PuzzleIncorrectView>();
        kernel.RegisterToSelf<PuzzleHint1View>();
        kernel.RegisterToSelf<PuzzleHint2View>();
        kernel.RegisterToSelf<PuzzleHint3View>();

        kernel.RegisterToSelf<RoomForm>();
        kernel.RegisterToSelf<RoomParamsScriptForm>();
        kernel.RegisterToSelf<RoomFlagsForm>();
        kernel.RegisterToSelf<RoomRenderForm>();
        kernel.RegisterToSelf<RoomView>();

        kernel.RegisterConfiguration<Layton1ToolFormsConfiguration>();
    }

    public void AddMessageSubscriptions(IEventBroker broker)
    {
    }

    public void Configure(IConfigurator configurator)
    {
    }
}