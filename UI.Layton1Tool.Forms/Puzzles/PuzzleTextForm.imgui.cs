using System.Numerics;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text.Editor;
using ImGui.Forms.Models;
using ImGui.Forms.Controls;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Forms.Puzzles;

internal partial class PuzzleTextForm : Component
{
    private StackLayout _mainLayout;

    private TextEditor _descriptionBox;
    private TextEditor _correctBox;
    private TextEditor _incorrectBox;

    private Component _descriptionView;
    private Component _correctView;
    private Component _incorrectView;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent(Layton1NdsInfo ndsInfo, IFormFactory forms, ILocalizationProvider localizations)
    {
        _descriptionView = forms.CreatePuzzleDescriptionView(ndsInfo);
        _correctView = forms.CreatePuzzleCorrectView(ndsInfo);
        _incorrectView = forms.CreatePuzzleIncorrectView(ndsInfo);

        _descriptionBox = new TextEditor { IsReadOnly = true };
        _correctBox = new TextEditor { IsReadOnly = true };
        _incorrectBox = new TextEditor { IsReadOnly = true };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                new TableLayout
                {
                    Size = new Size(SizeValue.Parent, .3f),
                    Spacing = new Vector2(5, 5),
                    Rows =
                    {
                        new TableRow
                        {
                            Cells =
                            {
                                new Label(localizations.PuzzleInfoDescriptionText),
                                new Label(localizations.PuzzleInfoCorrectText),
                                new Label(localizations.PuzzleInfoIncorrectText)
                            }
                        },
                        new TableRow
                        {
                            Cells =
                            {
                                _descriptionBox,
                                _correctBox,
                                _incorrectBox
                            }
                        }
                    }
                },
                new StackLayout
                {
                    Alignment = Alignment.Horizontal,
                    Size = new Size(SizeValue.Parent, .7f),
                    ItemSpacing = 5,
                    Items =
                    {
                        _descriptionView,
                        _correctView,
                        _incorrectView
                    }
                }
            }
        };
    }
}