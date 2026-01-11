using CrossCutting.Core.Contract.EventBrokerage;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms.Events;

internal class EventFlagsForm : FlagsForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;

    private int? _eventId;

    public EventFlagsForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILocalizationProvider localizations) : base(localizations)
    {
        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;

        eventBroker.Subscribe<EventScriptUpdatedMessage>(ProcessEventScriptUpdated);
        eventBroker.Subscribe<SelectedEventFlagsUpdatedMessage>(ProcessSelectedEventFlagsUpdated);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<EventScriptUpdatedMessage>(ProcessEventScriptUpdated);
        _eventBroker.Unsubscribe<SelectedEventFlagsUpdatedMessage>(ProcessSelectedEventFlagsUpdated);
    }

    private void ProcessEventScriptUpdated(EventScriptUpdatedMessage message)
    {
        if (_ndsInfo.Rom != message.Rom)
            return;

        UpdateStates(message.States);

        _eventId = message.Event;
        States = message.States;
    }

    private void ProcessSelectedEventFlagsUpdated(SelectedEventFlagsUpdatedMessage message)
    {
        if (!_eventId.HasValue)
            return;

        if (_eventId.Value != message.Event)
            return;

        if (_ndsInfo.Rom != message.Rom)
            return;

        UpdateStates(message.States);

        States = message.States;
    }

    private void RaiseSelectedEventFlagsModified(int eventId)
    {
        _eventBroker.Raise(new SelectedEventFlagsModifiedMessage(_ndsInfo.Rom, eventId));
    }

    protected override void OnFlagsChanged()
    {
        if (!_eventId.HasValue)
            return;

        RaiseSelectedEventFlagsModified(_eventId.Value);
    }
}