using ImGui.Forms.Controls.Base;
using Konnect.Contract.DataClasses.Plugin.File.Font;
using UI.Layton1Tool.Forms.Contract.DataClasses;

namespace UI.Layton1Tool.Components.Contract;

public interface IComponentFactory
{
    Component CreateAnimationViewer(Layton1NdsInfo ndsInfo);

    Component CreateAnimationPlayer(Layton1NdsInfo ndsInfo);

    Component CreateZoomableCharacterInfo();

    Component CreateZoomablePaddedGlyph();

    Component CreateGlyphElement(CharacterInfo charInfo);
}