using ImGui.Forms;
using ImGui.Forms.Controls.Base;
using UI.Layton1Tool.Forms.Contract.DataClasses;

namespace UI.Layton1Tool.Forms.Contract;

public interface IFormFactory
{
    Form CreateMainForm();
    Component CreateNdsForm(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleForm(Layton1NdsInfo ndsInfo);
    Component CreateImageForm(Layton1NdsInfo ndsInfo);
    Component CreateGdsForm(Layton1NdsInfo ndsInfo);
    Component CreatePcmForm(Layton1NdsInfo ndsInfo);
    Component CreateAnimationForm(Layton1NdsInfo ndsInfo);
    Component CreateTextForm(Layton1NdsInfo ndsInfo);
    Component CreateFontForm(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleInfo(Layton1NdsInfo ndsInfo);
}