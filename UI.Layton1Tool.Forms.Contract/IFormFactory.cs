using ImGui.Forms;
using ImGui.Forms.Controls.Base;
using UI.Layton1Tool.Forms.Contract.DataClasses;

namespace UI.Layton1Tool.Forms.Contract;

public interface IFormFactory
{
    Form CreateMainForm();

    Component CreateNdsForm(Layton1NdsInfo ndsInfo);
    Component CreateImageForm(Layton1NdsInfo ndsInfo);
    Component CreateGdsForm(Layton1NdsInfo ndsInfo);
    Component CreatePcmForm(Layton1NdsInfo ndsInfo);
    Component CreateAnimationForm(Layton1NdsInfo ndsInfo);
    Component CreateTextForm(Layton1NdsInfo ndsInfo);
    Component CreateFontForm(Layton1NdsInfo ndsInfo);

    Component CreatePuzzleForm(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleTitleForm(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleTextForm(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleHintForm(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleScriptForm(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleTitleView(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleIndexView(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleDescriptionView(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleCorrectView(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleIncorrectView(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleHint1View(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleHint2View(Layton1NdsInfo ndsInfo);
    Component CreatePuzzleHint3View(Layton1NdsInfo ndsInfo);

    Component CreateRoomForm(Layton1NdsInfo ndsInfo);
    Component CreateRoomParamsScriptForm(Layton1NdsInfo ndsInfo);
    Component CreateRoomFlagsForm(Layton1NdsInfo ndsInfo);
    Component CreateRoomRenderForm(Layton1NdsInfo ndsInfo);
    Component CreateRoomView(Layton1NdsInfo ndsInfo);
}