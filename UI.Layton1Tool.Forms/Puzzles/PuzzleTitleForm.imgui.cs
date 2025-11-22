using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Models;
using System.Numerics;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Forms.Puzzles;

internal partial class PuzzleTitleForm : Component
{
    private StackLayout _mainLayout;

    private TextBox _internalIdBox;
    private TextBox _numberBox;
    private TextBox _titleBox;
    private TextBox _locationBox;
    private TextBox _typeBox;
    private TextBox _picarat1Box;
    private TextBox _picarat2Box;
    private TextBox _picarat3Box;

    private Component _titleView;
    private Component _indexView;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent(Layton1NdsInfo ndsInfo, IFormFactory forms, ILocalizationProvider localizations)
    {
        _titleView = forms.CreatePuzzleTitleView(ndsInfo);
        _indexView = forms.CreatePuzzleIndexView(ndsInfo);

        _internalIdBox = new TextBox { IsReadOnly = true };
        _numberBox = new TextBox { IsReadOnly = true };
        _titleBox = new TextBox { IsReadOnly = true };
        _locationBox = new TextBox { IsReadOnly = true };
        _typeBox = new TextBox { IsReadOnly = true };
        _picarat1Box = new TextBox { IsReadOnly = true };
        _picarat2Box = new TextBox { IsReadOnly = true };
        _picarat3Box = new TextBox { IsReadOnly = true };

        _mainLayout = new StackLayout
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
                        _titleView,
                        _indexView
                    }
                }
            }
        };
    }
}