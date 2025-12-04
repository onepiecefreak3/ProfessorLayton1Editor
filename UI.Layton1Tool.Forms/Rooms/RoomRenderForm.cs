using CrossCutting.Core.Contract.EventBrokerage;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Messages.DataClasses;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms.Rooms;

internal partial class RoomRenderForm
{
    private readonly RoomRenderSettings _renderSettings;
    private readonly IEventBroker _eventBroker;
    private readonly ISettingsProvider _settings;

    public RoomRenderForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ISettingsProvider settings, IFormFactory forms, ILocalizationProvider localizations)
    {
        InitializeComponent(ndsInfo, settings, forms, localizations);

        _renderSettings = new RoomRenderSettings
        {
            RenderTextBoxes = settings.RenderTextBoxes,
            RenderHintBoxes = settings.RenderHintBoxes,
            RenderObjectBoxes = settings.RenderObjectBoxes,
            RenderMovementArrows = settings.RenderMovementArrows
        };
        _eventBroker = eventBroker;
        _settings = settings;

        _renderTextBox!.CheckChanged += _renderTextBox_CheckChanged;
        _renderHintBox!.CheckChanged += _renderHintBox_CheckChanged;
        _renderObjectBox!.CheckChanged += _renderObjectBox_CheckChanged;
        _renderMovementBox!.CheckChanged += _renderMovementBox_CheckChanged;

        eventBroker.Subscribe<SelectedRoomRenderSettingsModifiedMessage>(ProcessSelectedRoomRenderSettingsModified);

        RaiseSelectedRoomRenderSettingsModified();
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedRoomRenderSettingsModifiedMessage>(ProcessSelectedRoomRenderSettingsModified);
    }

    private void _renderMovementBox_CheckChanged(object? sender, EventArgs e)
    {
        _settings.RenderMovementArrows = _renderSettings.RenderMovementArrows = _renderMovementBox.Checked;

        RaiseSelectedRoomRenderSettingsModified();
    }

    private void _renderObjectBox_CheckChanged(object? sender, EventArgs e)
    {
        _settings.RenderObjectBoxes = _renderSettings.RenderObjectBoxes = _renderObjectBox.Checked;

        RaiseSelectedRoomRenderSettingsModified();
    }

    private void _renderHintBox_CheckChanged(object? sender, EventArgs e)
    {
        _settings.RenderHintBoxes = _renderSettings.RenderHintBoxes = _renderHintBox.Checked;

        RaiseSelectedRoomRenderSettingsModified();
    }

    private void _renderTextBox_CheckChanged(object? sender, EventArgs e)
    {
        _settings.RenderTextBoxes = _renderSettings.RenderTextBoxes = _renderTextBox.Checked;

        RaiseSelectedRoomRenderSettingsModified();
    }

    private void ProcessSelectedRoomRenderSettingsModified(SelectedRoomRenderSettingsModifiedMessage message)
    {
        if (message.Source == this)
            return;

        _renderTextBox.CheckChanged -= _renderTextBox_CheckChanged;
        _renderHintBox.CheckChanged -= _renderHintBox_CheckChanged;
        _renderObjectBox.CheckChanged -= _renderObjectBox_CheckChanged;
        _renderMovementBox.CheckChanged -= _renderMovementBox_CheckChanged;

        _renderTextBox.Checked = message.Settings.RenderTextBoxes;
        _renderHintBox.Checked = message.Settings.RenderHintBoxes;
        _renderObjectBox.Checked = message.Settings.RenderObjectBoxes;
        _renderMovementBox.Checked = message.Settings.RenderMovementArrows;

        _renderTextBox.CheckChanged += _renderTextBox_CheckChanged;
        _renderHintBox.CheckChanged += _renderHintBox_CheckChanged;
        _renderObjectBox.CheckChanged += _renderObjectBox_CheckChanged;
        _renderMovementBox.CheckChanged += _renderMovementBox_CheckChanged;
    }

    private void RaiseSelectedRoomRenderSettingsModified()
    {
        _eventBroker.Raise(new SelectedRoomRenderSettingsModifiedMessage(this, _renderSettings));
    }
}