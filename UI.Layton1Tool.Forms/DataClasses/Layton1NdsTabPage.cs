using ImGui.Forms.Controls;
using UI.Layton1Tool.Forms.Contract.DataClasses;

namespace UI.Layton1Tool.Forms.DataClasses;

class Layton1NdsTabPage
{
    public required Layton1NdsInfo Info { get; set; }
    public required Stream Stream { get; set; }
    public required TabPage Page { get; set; }
    public required Layton1NdsForm Form { get; set; }
}