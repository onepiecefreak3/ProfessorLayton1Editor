using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Tree;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms.Puzzles;

partial class PuzzleForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PathProvider _pathProvider;
    private readonly ILayton1PuzzleIdProvider _puzzleIdProvider;
    private readonly IColorProvider _colors;

    private readonly HashSet<Layton1PuzzleId> _changedPuzzles = [];

    private Layton1PuzzleId[]? _puzzleIds;
    private Layton1PuzzleId? _selectedPuzzleId;
    private Layton1NdsFile? _scriptFile;

    public PuzzleForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1PuzzleIdProvider puzzleIdProvider, ILayton1NdsFileManager fileManager,
        ILayton1PathProvider pathProvider, ILocalizationProvider localizations, IFormFactory formFactory, IImageProvider images, IColorProvider colors)
    {
        InitializeComponent(ndsInfo, localizations, formFactory, images);

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;
        _fileManager = fileManager;
        _pathProvider = pathProvider;
        _puzzleIdProvider = puzzleIdProvider;
        _colors = colors;

        _saveButton!.Clicked += _saveButton_Clicked;
        _saveAsButton!.Clicked += _saveAsButton_Clicked;
        _prevButton!.Clicked += _prevButton_Clicked;
        _nextButton!.Clicked += _nextButton_Clicked;
        _puzzleTree!.SelectedNodeChanged += _puzzleTree_SelectedNodeChanged;
        _languageCombo!.SelectedItemChanged += _languageCombo_SelectedItemChanged;

        eventBroker.Subscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        eventBroker.Subscribe<FileAddedMessage>(ProcessFileAdded);
        eventBroker.Subscribe<NdsFileSavedMessage>(ProcessNdsFileSaved);
        eventBroker.Subscribe<SelectedPuzzleIdModifiedMessage>(ProcessPuzzleIdModified);
        eventBroker.Subscribe<SelectedPuzzleModifiedMessage>(ProcessPuzzleModified);
        eventBroker.Subscribe<SelectedPuzzleScriptModifiedMessage>(ProcessPuzzleScriptModified);

        InitializePuzzleList();
        UpdateSaveButtons();
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        _eventBroker.Unsubscribe<FileAddedMessage>(ProcessFileAdded);
        _eventBroker.Unsubscribe<NdsFileSavedMessage>(ProcessNdsFileSaved);
        _eventBroker.Unsubscribe<SelectedPuzzleIdModifiedMessage>(ProcessPuzzleIdModified);
        _eventBroker.Unsubscribe<SelectedPuzzleModifiedMessage>(ProcessPuzzleModified);
        _eventBroker.Unsubscribe<SelectedPuzzleScriptModifiedMessage>(ProcessPuzzleScriptModified);
    }

    private void _nextButton_Clicked(object? sender, EventArgs e)
    {
        _layoutIndex = Math.Min(_layoutIndex + 1, 3);

        UpdateLayout();
        UpdateButtons();
    }

    private void _prevButton_Clicked(object? sender, EventArgs e)
    {
        _layoutIndex = Math.Max(_layoutIndex - 1, 0);

        UpdateLayout();
        UpdateButtons();
    }

    private async void _saveAsButton_Clicked(object? sender, EventArgs e)
    {
        await Save(true);
    }

    private async void _saveButton_Clicked(object? sender, EventArgs e)
    {
        await Save(false);
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (_selectedPuzzleId is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        string filePath = _pathProvider.GetFullDirectory($"script/qscript/q{_selectedPuzzleId.InternalId}_param.gds", _ndsInfo.Rom.Version);
        if (message.File.Path == filePath)
            UpdateLogic(_selectedPuzzleId);

        UpdateSaveButtons();
    }

    private void ProcessFileAdded(FileAddedMessage message)
    {
        if (_selectedPuzzleId is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        string filePath = _pathProvider.GetFullDirectory($"script/qscript/q{_selectedPuzzleId.InternalId}_param.gds", _ndsInfo.Rom.Version);
        if (message.File.Path == filePath)
            UpdateLogic(_selectedPuzzleId);

        UpdateSaveButtons();
    }

    private void ProcessPuzzleScriptModified(SelectedPuzzleScriptModifiedMessage message)
    {
        if (_selectedPuzzleId is null)
            return;

        if (message.Source != _puzzleScript)
            return;

        if (_scriptFile is null)
        {
            string filePath = _pathProvider.GetFullDirectory($"script/qscript/q{_selectedPuzzleId.InternalId}_param.gds", _ndsInfo.Rom.Version);

            _scriptFile = _fileManager.Add(_ndsInfo.Rom, filePath, message.Script, FileType.Gds, CompressionType.None);
            RaiseFileAdded(_scriptFile);
        }
        else
        {
            _fileManager.Compose(_scriptFile, message.Script, FileType.Gds);
            RaiseFileContentModified(_scriptFile, message.Script);
        }

        _changedPuzzles.Add(message.PuzzleId);

        UpdatePuzzleList();
    }

    private void ProcessNdsFileSaved(NdsFileSavedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        _changedPuzzles.Clear();

        UpdateSaveButtons();
        UpdatePuzzleList();
    }

    private void ProcessPuzzleIdModified(SelectedPuzzleIdModifiedMessage message)
    {
        if (_puzzleIds is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (!_fileManager.TryGet(_ndsInfo.Rom, "sys/arm9.bin", out Layton1NdsFile? arm9File))
            return;

        _puzzleIdProvider.Set(_ndsInfo.Rom, _puzzleIds);

        RaiseFileContentModified(arm9File, _fileManager.GetUncompressedStream(arm9File));

        _changedPuzzles.Add(message.PuzzleId);

        UpdatePuzzleList();
    }

    private void ProcessPuzzleModified(SelectedPuzzleModifiedMessage message)
    {
        if (_selectedPuzzleId is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        _changedPuzzles.Add(_selectedPuzzleId);

        UpdatePuzzleList();
    }

    private async Task Save(bool saveAs)
    {
        if (_ndsInfo.Rom.Files.All(f => !f.IsChanged))
            return;

        await RaiseNdsFileSaveRequested(saveAs);
    }

    private void RaiseFileContentModified(Layton1NdsFile file, object? content)
    {
        _eventBroker.Raise(new FileContentModifiedMessage(this, file, content));
    }

    private void RaiseFileAdded(Layton1NdsFile file)
    {
        _eventBroker.Raise(new FileAddedMessage(this, file));
    }

    private async Task RaiseNdsFileSaveRequested(bool saveAs)
    {
        await _eventBroker.RaiseAsync(new NdsFileSaveRequestedMessage(_ndsInfo.Path, saveAs));
    }

    private void UpdateSaveButtons()
    {
        _saveButton.Enabled = _saveAsButton.Enabled = _ndsInfo.Rom.Files.Any(f => f.IsChanged);
    }

    private void _languageCombo_SelectedItemChanged(object? sender, EventArgs e)
    {
        RaiseSelectedPuzzleLanguageChanged(_languageCombo.SelectedItem.Content);
    }

    private void _puzzleTree_SelectedNodeChanged(object? sender, EventArgs e)
    {
        if (_puzzleTree.SelectedNode is null)
            return;

        _selectedPuzzleId = _puzzleTree.SelectedNode.Data;

        RaiseSelectedPuzzleChanged(_selectedPuzzleId, _languageCombo.SelectedItem.Content);

        UpdateLogic(_selectedPuzzleId);
        UpdateLayout();
    }

    private void RaiseSelectedPuzzleChanged(Layton1PuzzleId puzzle, TextLanguage language)
    {
        _eventBroker.Raise(new SelectedPuzzleChangedMessage(_ndsInfo.Rom, puzzle, language));
    }

    private void RaisePuzzleScriptUpdated(Layton1PuzzleId puzzle, GdsScriptFile script)
    {
        _eventBroker.Raise(new PuzzleScriptUpdatedMessage(_puzzleScript, _ndsInfo.Rom, puzzle, script));
    }

    private void RaiseSelectedPuzzleLanguageChanged(TextLanguage language)
    {
        _eventBroker.Raise(new SelectedPuzzleLanguageChangedMessage(_ndsInfo.Rom, language));
    }

    private void InitializePuzzleList()
    {
        List<int> remainingIds = Enumerable.Range(0, _puzzleIdProvider.MaxPuzzleSlots).ToList();

        Layton1PuzzleId[] puzzleIds = _puzzleIdProvider.Get(_ndsInfo.Rom);

        foreach (Layton1PuzzleId puzzleId in puzzleIds.OrderBy(p => p.Number))
        {
            remainingIds.Remove(puzzleId.InternalId);

            _puzzleTree.Nodes.Add(new TreeNode<Layton1PuzzleId>
            {
                Text = GetPuzzleNumberText(puzzleId),
                Data = puzzleId
            });
        }

        Layton1PuzzleId[] wifiPuzzleIds = _puzzleIdProvider.GetWifi(_ndsInfo.Rom);

        foreach (Layton1PuzzleId puzzleId in wifiPuzzleIds.OrderBy(p => p.Number))
        {
            remainingIds.Remove(puzzleId.InternalId);

            _puzzleTree.Nodes.Add(new TreeNode<Layton1PuzzleId>
            {
                Text = GetPuzzleNumberText(puzzleId),
                Data = puzzleId
            });
        }

        List<Layton1PuzzleId> emptyPuzzleIds = [];

        foreach (int puzzleId in remainingIds)
        {
            emptyPuzzleIds.Add(new Layton1PuzzleId
            {
                InternalId = puzzleId,
                Number = 0,
                IsWifi = false
            });

            _puzzleTree.Nodes.Add(new TreeNode<Layton1PuzzleId>
            {
                Text = "???",
                Data = emptyPuzzleIds[^1]
            });
        }

        _puzzleIds = puzzleIds.Concat(wifiPuzzleIds).Concat(emptyPuzzleIds).ToArray();
    }

    private void UpdateLogic(Layton1PuzzleId puzzleId)
    {
        string filePath = _pathProvider.GetFullDirectory($"script/qscript/q{puzzleId.InternalId}_param.gds", _ndsInfo.Rom.Version);

        GdsScriptFile script;
        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out _scriptFile))
        {
            script = new GdsScriptFile { Instructions = [new GdsScriptInstruction { Type = 12 }] };
        }
        else
        {
            if (_fileManager.Parse(_scriptFile, FileType.Gds) is not GdsScriptFile logicScript)
                return;

            script = logicScript;
        }

        RaisePuzzleScriptUpdated(puzzleId, script);
    }

    private void UpdatePuzzleList()
    {
        TreeNode<Layton1PuzzleId>[] nodes = _puzzleTree.Nodes.ToArray();

        _puzzleTree.Nodes.Clear();

        foreach (TreeNode<Layton1PuzzleId> node in nodes.Where(p => p.Data is { IsWifi: false, Number: > 0 }).OrderBy(n => n.Data.Number))
        {
            _puzzleTree.Nodes.Add(node);

            node.Text = node.Data.Number <= 0 ? "???" : GetPuzzleNumberText(node.Data);
            node.TextColor = _changedPuzzles.Contains(node.Data) ? _colors.Changed : _colors.Default;
        }

        foreach (TreeNode<Layton1PuzzleId> node in nodes.Where(p => p.Data is { IsWifi: true }).OrderBy(n => n.Data.Number))
        {
            _puzzleTree.Nodes.Add(node);

            node.Text = node.Data.Number <= 0 ? "???" : GetPuzzleNumberText(node.Data);
            node.TextColor = _changedPuzzles.Contains(node.Data) ? _colors.Changed : _colors.Default;
        }

        foreach (TreeNode<Layton1PuzzleId> node in nodes.Where(p => p.Data is { IsWifi: false, Number: <= 0 }).OrderBy(n => n.Data.Number))
        {
            _puzzleTree.Nodes.Add(node);

            node.Text = node.Data.Number <= 0 ? "???" : GetPuzzleNumberText(node.Data);
            node.TextColor = _changedPuzzles.Contains(node.Data) ? _colors.Changed : _colors.Default;
        }
    }

    private string GetPuzzleNumberText(Layton1PuzzleId puzzleId)
    {
        if (puzzleId.Number <= 0)
            return "???";

        if (puzzleId.IsWifi)
            return $"W{puzzleId.Number:00}";

        return $"{puzzleId.Number:000}";
    }
}