using ImGui.Forms.Controls.Lists;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using System.Text.RegularExpressions;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using UI.Layton1Tool.Resources.Contract;
using UI.Layton1Tool.Dialogs.DataClasses;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Dialogs;

partial class SearchDialog
{
    private CancellationTokenSource? _cancelSource = new();

    private readonly Layton1NdsRom _ndsRom;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PcmFileManager _pcmManager;
    private readonly ILocalizationProvider _localizations;

    public SearchDialog(Layton1NdsRom ndsRom, IEventBroker eventBroker, ILayton1NdsFileManager fileManager, ILayton1PcmFileManager pcmManager,
        ILocalizationProvider localizations, IColorProvider colors)
    {
        InitializeComponent(localizations, colors);

        _ndsRom = ndsRom;

        _eventBroker = eventBroker;
        _fileManager = fileManager;
        _pcmManager = pcmManager;
        _localizations = localizations;

        _inputText!.TextChanged += _inputText_TextChanged;
        _searchButton!.Clicked += _searchButton_Clicked;
        _cancelButton!.Clicked += _cancelButton_Clicked;
        _matchTable!.DoubleClicked += _matchTable_DoubleClicked;
    }

    private void _matchTable_DoubleClicked(object? sender, EventArgs e)
    {
        if (_matchTable.SelectedRows.Count <= 0)
            return;

        DataTableRow<SearchResult> row = _matchTable.SelectedRows[0];

        _eventBroker.Raise(new SelectedNdsFileChangedMessage(_ndsRom, row.Data.File));

        if(row.Data.SubFile is null)
            return;

        _eventBroker.Raise(new SelectedPcmFileChangedMessage(_ndsRom, row.Data.SubFile));
    }

    protected override Task CloseInternal()
    {
        _cancelSource?.Cancel();
        return Task.CompletedTask;
    }

    private void _cancelButton_Clicked(object? sender, EventArgs e)
    {
        _cancelSource?.Cancel();
    }

    private void _searchButton_Clicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_inputText.Text))
            return;

        _cancelSource = new CancellationTokenSource();

        _inputText.IsReadOnly = true;
        _searchButton.Enabled = false;
        _cancelButton.Enabled = true;

        Task.Run(Search, _cancelSource.Token);
    }

    private void _inputText_TextChanged(object? sender, EventArgs e)
    {
        _searchButton.Enabled = !string.IsNullOrEmpty(_inputText.Text);
    }

    private void Search()
    {
        _progressBar.Value = 0;
        _progressBar.Minimum = 0;
        _progressBar.Maximum = _ndsRom.Files.Length;

        _matchTable.Rows.Clear();

        Regex matchRegex = GetSearchTerm();

        foreach (Layton1NdsFile file in _ndsRom.Files)
        {
            IncrementSearchProgress();

            if (_cancelSource?.IsCancellationRequested ?? false)
                break;

            FileType fileType = _fileManager.Detect(file);

            switch (fileType)
            {
                case FileType.Text:
                    var text = (string?)_fileManager.Parse(file, fileType);

                    if (IsMatchText(text, matchRegex))
                        _matchTable.Rows.Add(new DataTableRow<SearchResult>(new SearchResult { File = file }));

                    break;

                case FileType.Pcm:
                    var pcmFiles = (PcmFile[]?)_fileManager.Parse(file, fileType);

                    MatchPcm(file, pcmFiles, matchRegex);

                    break;
            }
        }

        _progressBar.Value = _ndsRom.Files.Length - 1;
        IncrementSearchProgress();

        _inputText.IsReadOnly = false;
        _searchButton.Enabled = true;
        _cancelButton.Enabled = false;
    }

    private static bool IsMatchText(string? data, Regex matchRegex)
    {
        return !string.IsNullOrEmpty(data) && matchRegex.IsMatch(data);
    }

    private void MatchPcm(Layton1NdsFile ndsFile, PcmFile[]? files, Regex matchRegex)
    {
        if (files is null)
            return;

        foreach (PcmFile file in files)
        {
            FileType fileType = _pcmManager.Detect(file);

            if (fileType is not FileType.Text)
                continue;

            var text = (string?)_pcmManager.Parse(file, fileType);

            if (IsMatchText(text, matchRegex))
                _matchTable.Rows.Add(new DataTableRow<SearchResult>(new SearchResult { File = ndsFile, SubFile = file }));
        }
    }

    private void IncrementSearchProgress()
    {
        _progressBar.Value++;

        float completion = (float)_progressBar.Value / _ndsRom.Files.Length * 100;
        _progressBar.Text = _localizations.DialogSearchProgress(completion);
    }

    private Regex GetSearchTerm()
    {
        if (string.IsNullOrEmpty(_inputText.Text))
            return new(".*");

        string escapedSearchTerm = _inputText.Text.Replace(".", "\\.").Replace("*", ".*");
        return new Regex(escapedSearchTerm);
    }
}