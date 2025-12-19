using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Models;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Forms.Rooms;

internal partial class RoomFlagsForm : Component
{
    private StackLayout _mainLayout;

    private CheckBox _scriptReturnBox;
    private CheckBox _scriptSolvedBox;
    private TextBox _solvedCountText;
    private TextBox _stateText;
    private TextBox _dialogIndexText;
    private ImGui.Forms.Controls.Lists.List<StackLayout> _puzzleChecks;
    private ImGui.Forms.Controls.Lists.List<CheckBox> _byteFlags;
    private ImGui.Forms.Controls.Lists.List<CheckBox> _bitFlags;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent(ILocalizationProvider localizations)
    {
        _scriptReturnBox = new CheckBox
        {
            Enabled = false,
            Text = localizations.ScriptReturnText
        };
        _scriptSolvedBox = new CheckBox
        {
            Enabled = false,
            Text = localizations.ScriptSolvedText
        };
        _solvedCountText = new TextBox
        {
            AllowedCharacters = CharacterRestriction.Decimal,
            IsReadOnly = true
        };
        _stateText = new TextBox
        {
            AllowedCharacters = CharacterRestriction.Decimal,
            IsReadOnly = true
        };
        _dialogIndexText = new TextBox
        {
            AllowedCharacters = CharacterRestriction.Decimal,
            IsReadOnly = true
        };
        _puzzleChecks = new ImGui.Forms.Controls.Lists.List<StackLayout>
        {
            Alignment = Alignment.Vertical,
            Size = Size.HeightAlign,
            ItemSpacing = 5,
            IsSelectable = false
        };
        _byteFlags = new ImGui.Forms.Controls.Lists.List<CheckBox>
        {
            Alignment = Alignment.Vertical,
            Size = Size.HeightAlign,
            ItemSpacing = 5,
            IsSelectable = false
        };
        _bitFlags = new ImGui.Forms.Controls.Lists.List<CheckBox>
        {
            Alignment = Alignment.Vertical,
            Size = Size.HeightAlign,
            ItemSpacing = 5,
            IsSelectable = false
        };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                new TableLayout
                {
                    Size = new Size(SizeValue.Absolute(200), SizeValue.Parent),
                    Spacing = new(5, 5),
                    Rows =
                    {
                        new TableRow
                        {
                            Cells =
                            {
                                new Label(localizations.ScriptPuzzleSolvedCountText),
                                _solvedCountText
                            }
                        },
                        new TableRow
                        {
                            Cells =
                            {
                                new Label(localizations.ScriptStateText),
                                _stateText
                            }
                        },
                        new TableRow
                        {
                            Cells =
                            {
                                new Label(localizations.ScriptDialogIndexText),
                                _dialogIndexText
                            }
                        }
                    }
                },
                new StackLayout
                {
                    Alignment = Alignment.Vertical,
                    Size = Size.HeightAlign,
                    ItemSpacing = 5,
                    Items =
                    {
                        _scriptReturnBox,
                        _scriptSolvedBox
                    }
                },
                _puzzleChecks,
                _byteFlags,
                _bitFlags
            }
        };
    }
}