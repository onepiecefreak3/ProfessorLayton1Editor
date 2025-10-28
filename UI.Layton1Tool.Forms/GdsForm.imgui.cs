using System.Numerics;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text.Editor;
using ImGui.Forms.Models;
using ImGuiNET;
using Veldrid;

namespace UI.Layton1Tool.Forms;

partial class GdsForm : Component
{
    private StackLayout _editorLayout;
    private TableLayout _infoLayout;

    private TextEditor _scriptEditor;

    private ArrowButton _prevParameterButton;
    private ArrowButton _nextParameterButton;

    private Label _instructionNameLabel;
    private Label _instructionDescriptionLabel;
    private Label _parameterNameLabel;
    private Label _parameterDescriptionLabel;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _editorLayout.Update(contentRect);
    }

    private void InitializeComponent()
    {
        _scriptEditor = new TextEditor();

        _instructionNameLabel = new Label();
        _instructionDescriptionLabel = new Label();
        _parameterNameLabel = new Label();
        _parameterDescriptionLabel = new Label();

        _prevParameterButton = new ArrowButton(ImGuiDir.Left) { Enabled = false };
        _nextParameterButton = new ArrowButton(ImGuiDir.Right) { Enabled = false };

        _infoLayout = new TableLayout
        {
            Size = Size.WidthAlign,
            Spacing = new Vector2(5),
            Rows =
            {
                new TableRow
                {
                    Cells =
                    {
                        null!,
                        new StackLayout
                        {
                            Alignment = Alignment.Horizontal,
                            Size = Size.WidthAlign,
                            ItemSpacing = 5,
                            Items =
                            {
                                _prevParameterButton,
                                _nextParameterButton
                            }
                        }
                    }
                },
                new TableRow
                {
                    Cells =
                    {
                        _instructionNameLabel,
                        _parameterNameLabel
                    }
                },
                new TableRow
                {
                    Cells =
                    {
                        _instructionDescriptionLabel,
                        _parameterDescriptionLabel
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