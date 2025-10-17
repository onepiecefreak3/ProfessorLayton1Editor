using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text.Editor;
using ImGui.Forms.Models;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Forms;

partial class GdsForm : Component
{
    private StackLayout _editorLayout;
    private StackLayout _infoLayout;

    private TextEditor _scriptEditor;

    private Label _instructionText;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _editorLayout.Update(contentRect);
    }

    private void InitializeComponent(ILocalizationProvider localizations)
    {
        _scriptEditor = new TextEditor();

        _instructionText = new Label();

        _infoLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.WidthAlign,
            ItemSpacing = 5,
            Items =
            {
                new StackLayout
                {
                    Alignment = Alignment.Vertical,
                    Size = Size.Content,
                    ItemSpacing = 5,
                    Items =
                    {
                        new Label(localizations.ScriptInstructionCaption),
                        _instructionText
                    }
                }
            }
        };

        _editorLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                _scriptEditor
            }
        };
    }

    private void ToggleInstructionInfo(bool toggle)
    {
        if (!toggle)
        {
            if (_editorLayout.Items.Count <= 1)
                return;

            _editorLayout.Items.RemoveAt(1);
        }
        else
        {
            var item = new StackItem(_infoLayout) { Size = Size.WidthAlign };

            if (_editorLayout.Items.Count <= 1)
                _editorLayout.Items.Add(item);
            else
                _editorLayout.Items[1] = item;
        }
    }
}