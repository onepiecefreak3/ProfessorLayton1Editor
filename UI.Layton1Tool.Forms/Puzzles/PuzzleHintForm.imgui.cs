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

internal partial class PuzzleHintForm : Component
{
    private StackLayout _mainLayout;

    private TextEditor _hint1Box;
    private TextEditor _hint2Box;
    private TextEditor _hint3Box;

    private Component _hint1View;
    private Component _hint2View;
    private Component _hint3View;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent(Layton1NdsInfo ndsInfo, IFormFactory forms, ILocalizationProvider localizations)
    {
        _hint1View = forms.CreatePuzzleHint1View(ndsInfo);
        _hint2View = forms.CreatePuzzleHint2View(ndsInfo);
        _hint3View = forms.CreatePuzzleHint3View(ndsInfo);

        _hint1Box = new TextEditor { IsReadOnly = true };
        _hint2Box = new TextEditor { IsReadOnly = true };
        _hint3Box = new TextEditor { IsReadOnly = true };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                new Label(localizations.PuzzleInfoHintsText),
                new StackLayout
                {
                    Alignment = Alignment.Horizontal,
                    Size = new Size(SizeValue.Parent, .3f),
                    ItemSpacing = 5,
                    Items =
                    {
                        _hint1Box,
                        _hint2Box,
                        _hint3Box
                    }
                },
                new StackLayout
                {
                    Alignment = Alignment.Horizontal,
                    Size = new Size(SizeValue.Parent, .7f),
                    ItemSpacing = 5,
                    Items =
                    {
                        _hint1View,
                        _hint2View,
                        _hint3View
                    }
                }
            }
        };
    }
}