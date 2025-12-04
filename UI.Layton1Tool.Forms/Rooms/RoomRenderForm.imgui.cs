using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Models;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Forms.Rooms;

internal partial class RoomRenderForm : Component
{
    private TableLayout _settingsLayout;
    private StackLayout _mainLayout;

    private CheckBox _renderTextBox;
    private CheckBox _renderHintBox;
    private CheckBox _renderObjectBox;
    private CheckBox _renderMovementBox;

    private Component _viewForm;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent(Layton1NdsInfo ndsInfo, ISettingsProvider settings, IFormFactory forms, ILocalizationProvider localizations)
    {
        _viewForm = forms.CreateRoomView(ndsInfo);

        _renderTextBox = new CheckBox(localizations.RoomRenderTextBoxes) { Checked = settings.RenderTextBoxes };
        _renderHintBox = new CheckBox(localizations.RoomRenderHintBoxes) { Checked = settings.RenderHintBoxes };
        _renderObjectBox = new CheckBox(localizations.RoomRenderObjectBoxes) { Checked = settings.RenderObjectBoxes };
        _renderMovementBox = new CheckBox(localizations.RoomRenderMovementArrows) { Checked = settings.RenderMovementArrows };

        _settingsLayout = new TableLayout
        {
            Spacing = new(5),
            Size = Size.WidthAlign,
            Rows =
            {
                new TableRow
                {
                    Cells =
                    {
                        new TableCell(_renderTextBox),
                        new TableCell(_renderHintBox)
                    }
                },
                new TableRow
                {
                    Cells =
                    {
                        new TableCell(_renderObjectBox),
                        new TableCell(_renderMovementBox)
                    }
                }
            }
        };

        _mainLayout = new StackLayout
        {
            Items =
            {
                _settingsLayout,
                _viewForm
            }
        };
    }
}