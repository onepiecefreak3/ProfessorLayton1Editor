using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Lists;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using System.Text.RegularExpressions;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using UI.Layton1Tool.Dialogs.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Dialogs;

partial class SearchDialog
{
    private CancellationTokenSource? _cancelSource = new();

    private readonly Layton1NdsRom _ndsRom;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PcmFileManager _pcmManager;
    private readonly ILayton1ScriptFileConverter _scriptFileConverter;
    private readonly ILevel5ScriptWhitespaceNormalizer _whitespaceNormalizer;
    private readonly ILevel5ScriptComposer _scriptComposer;
    private readonly ILocalizationProvider _localizations;

    public SearchDialog(Layton1NdsRom ndsRom, IEventBroker eventBroker, ILayton1NdsFileManager fileManager, ILayton1PcmFileManager pcmManager,
        ILayton1ScriptFileConverter scriptFileConverter, ILevel5ScriptWhitespaceNormalizer whitespaceNormalizer, ILevel5ScriptComposer scriptComposer,
        ILocalizationProvider localizations, IColorProvider colors)
    {
        InitializeComponent(localizations, colors);

        _ndsRom = ndsRom;

        _eventBroker = eventBroker;
        _fileManager = fileManager;
        _pcmManager = pcmManager;
        _scriptFileConverter = scriptFileConverter;
        _whitespaceNormalizer = whitespaceNormalizer;
        _scriptComposer = scriptComposer;
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

        if (row.Data.SubFile is null)
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
        _progressBar.Maximum = _ndsRom.Files.Count;

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

                case FileType.Gds:
                    var script = (GdsScriptFile?)_fileManager.Parse(file, fileType);
                    if (script is null)
                        continue;

                    CodeUnitSyntax codeUnit = _scriptFileConverter.CreateCodeUnit(script, _ndsRom.GameCode);
                    _whitespaceNormalizer.NormalizeCodeUnit(codeUnit);

                    string scriptText = _scriptComposer.ComposeCodeUnit(codeUnit);

                    if (IsMatchText(scriptText, matchRegex))
                        _matchTable.Rows.Add(new DataTableRow<SearchResult>(new SearchResult { File = file }));

                    break;

                case FileType.Pcm:
                    var pcmFiles = (System.Collections.Generic.List<PcmFile>?)_fileManager.Parse(file, fileType);

                    MatchPcm(file, pcmFiles, matchRegex);

                    break;
            }
        }

        _progressBar.Value = _ndsRom.Files.Count - 1;
        IncrementSearchProgress();

        _inputText.IsReadOnly = false;
        _searchButton.Enabled = true;
        _cancelButton.Enabled = false;
    }

    private static bool IsMatchText(string? data, Regex matchRegex)
    {
        return !string.IsNullOrEmpty(data) && matchRegex.IsMatch(data);
    }

    private void MatchPcm(Layton1NdsFile ndsFile, System.Collections.Generic.List<PcmFile>? files, Regex matchRegex)
    {
        if (files is null)
            return;

        foreach (PcmFile file in files)
        {
            FileType fileType = _pcmManager.Detect(file);

            if (fileType is not FileType.Text)
                continue;

            var text = (string?)_pcmManager.Parse(file, fileType, _ndsRom.Version);

            if (IsMatchText(text, matchRegex))
                _matchTable.Rows.Add(new DataTableRow<SearchResult>(new SearchResult { File = ndsFile, SubFile = file }));
        }
    }

    private void IncrementSearchProgress()
    {
        _progressBar.Value++;

        float completion = (float)_progressBar.Value / _ndsRom.Files.Count * 100;
        _progressBar.Text = _localizations.DialogSearchProgress(completion);
    }

    private Regex? GetSearchTerm()
    {
        if (string.IsNullOrEmpty(_inputText.Text))
            return new(".*");

        string escapedSearchTerm = _inputText.Text.Replace("(","\\(").Replace(")", "\\)").Replace(".", "\\.").Replace("*", ".*");
        return new Regex(escapedSearchTerm);
    }
}