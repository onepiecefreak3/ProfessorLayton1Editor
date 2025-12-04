using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Forms.Rooms;

internal class RoomParamsScriptForm : ScriptForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;

    private int? _roomId;

    public RoomParamsScriptForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1ScriptInstructionManager instructionManager, ILayton1ScriptCodeUnitConverter codeUnitConverter,
        ILayton1ScriptFileConverter scriptFileConverter, ILevel5ScriptParser scriptParser, ILevel5ScriptComposer scriptComposer,
        ILevel5ScriptWhitespaceNormalizer whitespaceNormalizer, IPositionManager positionManager)
        : base(ndsInfo, instructionManager, codeUnitConverter, scriptFileConverter, scriptParser, scriptComposer, whitespaceNormalizer, positionManager)
    {
        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;

        eventBroker.Subscribe<RoomScriptUpdatedMessage>(ProcessRoomScriptUpdated);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<RoomScriptUpdatedMessage>(ProcessRoomScriptUpdated);
    }

    protected override void OnScriptChanged()
    {
        if (Script is null || !_roomId.HasValue)
            return;

        RaiseRoomScriptModified(_roomId.Value, Script);
    }

    private void ProcessRoomScriptUpdated(RoomScriptUpdatedMessage message)
    {
        if (message.Target != this)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        _roomId = message.Room;

        UpdateScript(message.Script);
    }

    private void RaiseRoomScriptModified(int roomId, GdsScriptFile script)
    {
        _eventBroker.Raise(new SelectedRoomParamsModifiedMessage(_ndsInfo.Rom, roomId, script));
    }
}