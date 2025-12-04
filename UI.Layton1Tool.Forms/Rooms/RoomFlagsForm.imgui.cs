using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Models;
using Veldrid;

namespace UI.Layton1Tool.Forms.Rooms;

internal partial class RoomFlagsForm : Component
{
    private StackLayout _mainLayout;

    private ImGui.Forms.Controls.Lists.List<CheckBox> _flag1List;
    private ImGui.Forms.Controls.Lists.List<CheckBox> _flag2List;
    private TextBox _stateText;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent()
    {
        _flag1List = new ImGui.Forms.Controls.Lists.List<CheckBox>
        {
            Alignment = Alignment.Vertical,
            ItemSpacing = 5,
            Size = new Size(.33f, SizeValue.Content),
            IsSelectable = false
        };
        _flag2List = new ImGui.Forms.Controls.Lists.List<CheckBox>
        {
            Alignment = Alignment.Vertical,
            ItemSpacing = 5,
            Size = new Size(.33f, SizeValue.Content),
            IsSelectable = false
        };
        _stateText = new TextBox
        {
            AllowedCharacters = CharacterRestriction.Decimal,
            Width = .33f,
            IsReadOnly = true
        };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                _flag1List,
                _flag2List,
                _stateText
            }
        };
    }
}