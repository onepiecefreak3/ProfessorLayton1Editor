using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Controls.Text.Editor;
using ImGui.Forms.Models;
using System.Numerics;
using ImGuiNET;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Forms;

partial class PuzzleInfoForm : Component
{
    private int _layoutIndex;
    private ArrowButton _prevButton;
    private ArrowButton _nextButton;

    private StackLayout _mainLayout;
    private StackLayout _infoLayout;
    private Component _logicForm;

    private TextBox _internalIdBox;
    private TextBox _numberBox;
    private TextBox _titleBox;
    private TextEditor _descriptionBox;
    private TextEditor _hint1Box;
    private TextEditor _hint2Box;
    private TextEditor _hint3Box;
    private TextEditor _correctBox;
    private TextEditor _incorrectBox;
    private TextBox _locationBox;
    private TextBox _typeBox;
    private TextBox _picarat1Box;
    private TextBox _picarat2Box;
    private TextBox _picarat3Box;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent(Layton1NdsInfo ndsInfo, IFormFactory forms, ILocalizationProvider localizations)
    {
        _prevButton = new ArrowButton(ImGuiDir.Left) { Enabled = false };
        _nextButton = new ArrowButton(ImGuiDir.Right) { Enabled = false };

        _internalIdBox = new TextBox { IsReadOnly = true };
        _numberBox = new TextBox { IsReadOnly = true };
        _titleBox = new TextBox { IsReadOnly = true };
        _descriptionBox = new TextEditor { IsReadOnly = true };
        _hint1Box = new TextEditor { IsReadOnly = true };
        _hint2Box = new TextEditor { IsReadOnly = true };
        _hint3Box = new TextEditor { IsReadOnly = true };
        _correctBox = new TextEditor { IsReadOnly = true };
        _incorrectBox = new TextEditor { IsReadOnly = true };
        _locationBox = new TextBox { IsReadOnly = true };
        _typeBox = new TextBox { IsReadOnly = true };
        _picarat1Box = new TextBox { IsReadOnly = true };
        _picarat2Box = new TextBox { IsReadOnly = true };
        _picarat3Box = new TextBox { IsReadOnly = true };

        _logicForm = forms.CreatePuzzleScriptForm(ndsInfo);

        _infoLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                new TableLayout
                {
                    Spacing = new Vector2(5, 5),
                    Size = Size.WidthAlign,
                    Rows =
                    {
                        new TableRow
                        {
                            Cells =
                            {
                                new Label(localizations.PuzzleInfoInternalIdText),
                                new Label(localizations.PuzzleInfoNumberText),
                                new Label(localizations.PuzzleInfoTitleText)
                            }
                        },
                        new TableRow
                        {
                            Cells =
                            {
                                _internalIdBox,
                                _numberBox,
                                _titleBox
                            }
                        }
                    }
                },
                new TableLayout
                {
                    Spacing = new Vector2(5, 5),
                    Size = Size.WidthAlign,
                    Rows =
                    {
                        new TableRow
                        {
                            Cells =
                            {
                                new Label(localizations.PuzzleInfoLocationText),
                                new Label(localizations.PuzzleInfoTypeText)
                            }
                        },
                        new TableRow
                        {
                            Cells =
                            {
                                _locationBox,
                                _typeBox
                            }
                        }
                    }
                },
                new TableLayout
                {
                    Spacing = new Vector2(5, 5),
                    Size = Size.WidthAlign,
                    Rows =
                    {
                        new TableRow
                        {
                            Cells =
                            {
                                new Label(localizations.PuzzleInfoPicaratText)
                            }
                        },
                        new TableRow
                        {
                            Cells =
                            {
                                _picarat1Box,
                                _picarat2Box,
                                _picarat3Box
                            }
                        }
                    }
                },
                new StackLayout
                {
                    Alignment = Alignment.Horizontal,
                    Size = Size.Parent,
                    ItemSpacing = 5,
                    Items =
                    {
                        new StackLayout
                        {
                            Alignment = Alignment.Vertical,
                            Size = Size.Parent,
                            ItemSpacing = 5,
                            Items =
                            {
                                new Label(localizations.PuzzleInfoDescriptionText),
                                _descriptionBox,
                                new Label(localizations.PuzzleInfoCorrectText),
                                _correctBox,
                                new Label(localizations.PuzzleInfoIncorrectText),
                                _incorrectBox
                            }
                        },
                        new StackLayout
                        {
                            Alignment = Alignment.Vertical,
                            Size = Size.Parent,
                            ItemSpacing = 5,
                            Items =
                            {
                                new Label(localizations.PuzzleInfoHintsText),
                                _hint1Box,
                                _hint2Box,
                                _hint3Box
                            }
                        }
                    }
                }
            }
        };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                new StackLayout
                {
                    Alignment = Alignment.Horizontal,
                    Size = Size.WidthAlign,
                    ItemSpacing = 5,
                    Items =
                    {
                        _prevButton,
                        _nextButton
                    }
                },
                _infoLayout
            }
        };
    }

    private void UpdateLayout()
    {
        switch (_layoutIndex)
        {
            case 0:
                _mainLayout.Items[1] = _infoLayout;
                break;

            case 1:
                _mainLayout.Items[1] = _logicForm;
                break;
        }
    }
}