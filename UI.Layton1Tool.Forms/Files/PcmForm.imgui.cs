using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text.Editor;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Models;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using Veldrid;

namespace UI.Layton1Tool.Forms;

partial class PcmForm : Component
{
    private StackLayout _fileLayout;

    private TreeView<PcmFile> _fileTree;
    private Panel _contentPanel;

    private TextEditor _textEditor;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _fileLayout.Update(contentRect);
    }

    private void InitializeComponent()
    {
        _textEditor = new TextEditor { IsReadOnly = true };

        _fileTree = new TreeView<PcmFile> { Size = new Size(SizeValue.Relative(.3f), SizeValue.Parent) };
        _contentPanel = new Panel { Size = Size.Parent };

        _fileLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                _fileTree,
                _contentPanel
            }
        };
    }

    protected override void SetTabInactiveCore()
    {
        _fileTree.SetTabInactive();
    }
}