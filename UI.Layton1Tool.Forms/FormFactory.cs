using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using ImGui.Forms;
using ImGui.Forms.Controls.Base;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;

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

    public Component CreatePuzzleForm(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleForm>(new ConstructorParameter("ndsInfo", ndsInfo));
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

    public Component CreatePuzzleInfo(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<PuzzleInfoForm>(new ConstructorParameter("ndsInfo", ndsInfo));
    }
}