using ImGui.Forms.Controls;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Forms.DataClasses;

class Layton1NdsTabPage
{
    public required string Path { get; set; }
    public required Stream Stream { get; set; }
    public required Layton1NdsRom Rom { get; set; }
    public required TabPage Page { get; set; }
}