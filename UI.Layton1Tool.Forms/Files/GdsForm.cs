using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Forms.Files;

internal class GdsForm : ScriptForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsFileManager _fileManager;

    private Layton1NdsFile? _selectedFile;

    public GdsForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1NdsFileManager fileManager, ILayton1ScriptInstructionManager instructionManager,
        ILayton1ScriptCodeUnitConverter codeUnitConverter, ILayton1ScriptFileConverter scriptFileConverter, ILevel5ScriptParser scriptParser,
        ILevel5ScriptComposer scriptComposer, ILevel5ScriptWhitespaceNormalizer whitespaceNormalizer, IPositionManager positionManager)
        : base(ndsInfo, instructionManager, codeUnitConverter, scriptFileConverter, scriptParser, scriptComposer, whitespaceNormalizer, positionManager)
    {
        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;
        _fileManager = fileManager;

        eventBroker.Subscribe<SelectedFileChangedMessage>(UpdateScript);
        eventBroker.Subscribe<FileContentModifiedMessage>(UpdateScript);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedFileChangedMessage>(UpdateScript);
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(UpdateScript);
    }

    protected override void OnScriptChanged()
    {
        if (Script is null || _selectedFile is null)
            return;

        _fileManager.Compose(_selectedFile, Script);

        RaiseFileContentModified(_selectedFile, Script);
    }

    private void RaiseFileContentModified(Layton1NdsFile file, GdsScriptFile script)
    {
        _eventBroker.Raise(new FileContentModifiedMessage(this, file, script));
    }

    private void UpdateScript(SelectedFileChangedMessage message)
    {
        if (message.Target != this)
            return;

        if (message.Content is not GdsScriptFile script)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        _selectedFile = message.File;

        UpdateScript(script);
    }

    private void UpdateScript(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (message.File != _selectedFile)
            return;

        if (message.Content is not GdsScriptFile script)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        UpdateScript(script);
    }
}