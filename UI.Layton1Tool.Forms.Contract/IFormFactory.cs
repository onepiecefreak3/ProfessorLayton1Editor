using ImGui.Forms;
using ImGui.Forms.Controls.Base;
using UI.Layton1Tool.Forms.Contract.DataClasses;

namespace UI.Layton1Tool.Forms.Contract;

public interface IFormFactory
{
    Form CreateMainForm();
    Component CreateNdsForm(Layton1NdsInfo ndsInfo);
    Component CreateBgxForm(Layton1NdsInfo ndsInfo);
    Component CreateGdsForm(Layton1NdsInfo ndsInfo);
    Component CreatePcmForm(Layton1NdsInfo ndsInfo);
    Component CreateAnimationForm(Layton1NdsInfo ndsInfo);
    Component CreateTextForm(Layton1NdsInfo ndsInfo);
}