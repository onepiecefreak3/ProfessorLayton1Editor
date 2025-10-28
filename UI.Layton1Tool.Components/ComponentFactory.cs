using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using ImGui.Forms.Controls.Base;
using Konnect.Contract.DataClasses.Plugin.File.Font;
using UI.Layton1Tool.Components.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;

namespace UI.Layton1Tool.Components;

class ComponentFactory(ICoCoKernel kernel) : IComponentFactory
{
    public Component CreateAnimationViewer(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<AnimationViewer>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreateAnimationPlayer(Layton1NdsInfo ndsInfo)
    {
        return kernel.Get<AnimationPlayer>(new ConstructorParameter("ndsInfo", ndsInfo));
    }

    public Component CreateZoomableCharacterInfo()
    {
        return kernel.Get<ZoomableCharacterInfo>();
    }

    public Component CreateZoomablePaddedGlyph()
    {
        return kernel.Get<ZoomablePaddedGlyph>();
    }

    public Component CreateGlyphElement(CharacterInfo charInfo)
    {
        return kernel.Get<GlyphElement>(new ConstructorParameter("charInfo", charInfo));
    }
}