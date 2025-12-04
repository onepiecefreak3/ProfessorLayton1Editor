using ImGui.Forms.Controls.Base;
using UI.Layton1Tool.Forms.Enums;

namespace UI.Layton1Tool.Forms.DataClasses;

class Layton1NdsForm
{
    public required FormType Type { get; set; }
    public Component? NdsForm { get; set; }
    public Component? PuzzleForm { get; set; }
    public Component? RoomForm { get; set; }
}