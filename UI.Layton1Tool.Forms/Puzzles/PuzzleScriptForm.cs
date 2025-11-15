using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Forms.Puzzles;

internal class PuzzleScriptForm : ScriptForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;

    private Layton1PuzzleId? _puzzleId;

    public PuzzleScriptForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1ScriptInstructionManager instructionManager, ILayton1ScriptCodeUnitConverter codeUnitConverter,
        ILayton1ScriptFileConverter scriptFileConverter, ILevel5ScriptParser scriptParser, ILevel5ScriptComposer scriptComposer,
        ILevel5ScriptWhitespaceNormalizer whitespaceNormalizer, IPositionManager positionManager)
        : base(ndsInfo, instructionManager, codeUnitConverter, scriptFileConverter, scriptParser, scriptComposer, whitespaceNormalizer, positionManager)
    {
        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;

        eventBroker.Subscribe<PuzzleScriptUpdatedMessage>(ProcessPuzzleScriptUpdated);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<PuzzleScriptUpdatedMessage>(ProcessPuzzleScriptUpdated);
    }

    protected override void OnScriptChanged()
    {
        if (Script is null || _puzzleId is null)
            return;

        RaisePuzzleScriptModified(_puzzleId, Script);
    }

    private void ProcessPuzzleScriptUpdated(PuzzleScriptUpdatedMessage message)
    {
        if (message.Target != this)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        _puzzleId = message.PuzzleId;

        UpdateScript(message.Script);
    }

    private void RaisePuzzleScriptModified(Layton1PuzzleId puzzleId, GdsScriptFile script)
    {
        _eventBroker.Raise(new PuzzleScriptModifiedMessage(this, _ndsInfo.Rom, puzzleId, script));
    }
}