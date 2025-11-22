using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using ImGui.Forms;
using ImGui.Forms.Controls.Base;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.Files;
using UI.Layton1Tool.Forms.Puzzles;
using UI.Layton1Tool.Forms.Views;

namespace UI.Layton1Tool.Forms;

class FormFactory(ICoCoKernel kernel) : IFormFactory
{
    public Form CreateMainForm()
    {
        return kernel.Get<MainForm>();
    }

    public Component CreateNdsForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<NdsForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreateImageForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<ImageForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreateGdsForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<GdsForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePcmForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PcmForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreateAnimationForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<AnimationForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreateTextForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<TextForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreateFontForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<FontForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleTitleForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleTitleForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleTextForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleTextForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleHintForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleHintForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleScriptForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleScriptForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleTitleView(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleTitleView>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleIndexView(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleIndexView>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleDescriptionView(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleDescriptionView>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleCorrectView(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleCorrectView>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleIncorrectView(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleIncorrectView>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleHint1View(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleHint1View>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleHint2View(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleHint2View>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreatePuzzleHint3View(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleHint3View>(new ConstructorParameter("ndsInfo", ndsInfo));
    }
}