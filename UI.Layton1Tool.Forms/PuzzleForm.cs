using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Tree;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.InternalContract;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms;

partial class PuzzleForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1PuzzleIdProvider _puzzleIdProvider;

    public PuzzleForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1PuzzleIdProvider puzzleIdProvider, ILocalizationProvider localizations,
        IFormFactory formFactory, IImageProvider images)
    {
        InitializeComponent(ndsInfo, localizations, formFactory, images);

        _eventBroker = eventBroker;
        _ndsInfo = ndsInfo;
        _puzzleIdProvider = puzzleIdProvider;

        _saveButton!.Clicked += _saveButton_Clicked;
        _saveAsButton!.Clicked += _saveAsButton_Clicked;
        _puzzleTree!.SelectedNodeChanged += _puzzleTree_SelectedNodeChanged;
        _languageCombo!.SelectedItemChanged += _languageCombo_SelectedItemChanged;

        eventBroker.Subscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        eventBroker.Subscribe<NdsFileSavedMessage>(ProcessNdsFileSaved);

        InitializePuzzleList();

        UpdateSaveButtons();

        RaiseSelectedPuzzleLanguageChanged(_languageCombo.SelectedItem.Content);
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
        if (message.File.Rom != _ndsInfo.Rom)
            return;

        UpdateSaveButtons();
    }

    private void ProcessNdsFileSaved(NdsFileSavedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        UpdateSaveButtons();
    }

    private async Task Save(bool saveAs)
    {
        if (_ndsInfo.Rom.Files.All(f => !f.IsChanged))
            return;

        await RaiseNdsFileSaveRequested(saveAs);
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

        var puzzleData = _puzzleTree.SelectedNode.Data;

        RaiseSelectedPuzzleChanged(puzzleData);

        _contentPanel.Content = _puzzleInfo;
    }

    private void RaiseSelectedPuzzleChanged(Layton1PuzzleId puzzle)
    {
        _eventBroker.Raise(new SelectedPuzzleChangedMessage(_ndsInfo.Rom, puzzle));
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

        puzzleIds = _puzzleIdProvider.GetWifi(_ndsInfo.Rom);

        foreach (Layton1PuzzleId puzzleId in puzzleIds.OrderBy(p => p.Number))
        {
            remainingIds.Remove(puzzleId.InternalId);

            _puzzleTree.Nodes.Add(new TreeNode<Layton1PuzzleId>
            {
                Text = GetPuzzleNumberText(puzzleId),
                Data = puzzleId
            });
        }

        foreach (int puzzleId in remainingIds)
        {
            _puzzleTree.Nodes.Add(new TreeNode<Layton1PuzzleId>
            {
                Text = "???",
                Data = new Layton1PuzzleId
                {
                    InternalId = puzzleId,
                    Number = 0,
                    IsWifi = false
                }
            });
        }
    }

    private string GetPuzzleNumberText(Layton1PuzzleId puzzleId)
    {
        if (puzzleId.IsWifi)
            return $"W{puzzleId.Number:00}";

        return $"{puzzleId.Number:000}";
    }
}