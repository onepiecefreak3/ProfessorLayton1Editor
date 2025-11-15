using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Text.Editor;
using ImGui.Forms.Models;
using Veldrid;

namespace UI.Layton1Tool.Forms;

partial class TextForm : Component
{
    private TextEditor _textEditor;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _textEditor.Update(contentRect);
    }

    private void InitializeComponent()
    {
        _textEditor = new TextEditor { IsReadOnly = true };
    }
}