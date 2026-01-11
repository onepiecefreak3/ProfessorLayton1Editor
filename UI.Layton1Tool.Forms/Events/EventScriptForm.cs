using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Forms.Events;

internal class EventScriptForm : ScriptForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;

    private int? _eventId;

    public EventScriptForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1ScriptInstructionManager instructionManager, ILayton1ScriptCodeUnitConverter codeUnitConverter,
        ILayton1ScriptFileConverter scriptFileConverter, ILayton1ScriptParser scriptParser, ILayton1ScriptComposer scriptComposer,
        ILayton1ScriptWhitespaceNormalizer whitespaceNormalizer, IPositionManager positionManager)
        : base(ndsInfo, instructionManager, codeUnitConverter, scriptFileConverter, scriptParser, scriptComposer, whitespaceNormalizer, positionManager)
    {
        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;

        eventBroker.Subscribe<EventScriptUpdatedMessage>(ProcessEventScriptUpdated);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<EventScriptUpdatedMessage>(ProcessEventScriptUpdated);
    }

    protected override void OnScriptChanged()
    {
        if (Script is null || !_eventId.HasValue)
            return;

        RaiseEventScriptModified(_eventId.Value, Script);
    }

    private void ProcessEventScriptUpdated(EventScriptUpdatedMessage message)
    {
        if (message.Target != this)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        _eventId = message.Event;

        UpdateScript(message.Script);
    }

    private void RaiseEventScriptModified(int roomId, GdsScriptFile script)
    {
        _eventBroker.Raise(new SelectedEventModifiedMessage(_ndsInfo.Rom, roomId, script));
    }
}