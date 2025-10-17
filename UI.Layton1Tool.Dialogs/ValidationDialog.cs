using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using ImGui.Forms.Controls.Lists;
using UI.Layton1Tool.Resources.Contract;
using ImGui.Forms.Modals.IO.Windows;
using ImGui.Forms.Modals;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Localization;
using ImGui.Forms.Modals.IO;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using Logic.Domain.Level5Management.Contract.Enums;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Messages.Enums;

namespace UI.Layton1Tool.Dialogs;

partial class ValidationDialog
{
    private readonly CancellationTokenSource _cancelSource = new();

    private readonly Layton1NdsRom _ndsRom;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsValidator _validator;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ISettingsProvider _settings;
    private readonly ILocalizationProvider _localizations;

    private bool _isValidating = true;

    public ValidationDialog(Layton1NdsRom ndsRom, IEventBroker eventBroker, ILayton1NdsValidator validator, ILayton1NdsFileManager fileManager,
        ISettingsProvider settings, ILocalizationProvider localizations, IColorProvider colors)
    {
        InitializeComponent(localizations, colors);

        _ndsRom = ndsRom;
        _eventBroker = eventBroker;
        _validator = validator;
        _fileManager = fileManager;
        _settings = settings;
        _localizations = localizations;

        _cancelButton!.Clicked += _cancelButton_Clicked;
        _extractButton!.Clicked += _extractButton_Clicked;
        _errorTable!.DoubleClicked += _errorTable_DoubleClicked;
        _warningTable!.DoubleClicked += _warningTable_DoubleClicked;

        Task.Run(Validate, _cancelSource.Token);
    }

    private void _warningTable_DoubleClicked(object? sender, EventArgs e)
    {
        if (_warningTable.SelectedRows.Count <= 0)
            return;

        DataTableRow<Layton1NdsFile> row = _warningTable.SelectedRows[0];

        _eventBroker.Raise(new SelectedNdsFileChangedMessage(_ndsRom, row.Data));
    }

    private void _errorTable_DoubleClicked(object? sender, EventArgs e)
    {
        if (_errorTable.SelectedRows.Count <= 0)
            return;

        DataTableRow<Layton1ValidationError> row = _errorTable.SelectedRows[0];

        _eventBroker.Raise(new SelectedNdsFileChangedMessage(_ndsRom, row.Data.File));
    }

    private async void _extractButton_Clicked(object? sender, EventArgs e)
    {
        if (_isValidating)
            return;

        IEnumerable<Layton1NdsFile> ndsFiles = _errorTable.SelectedRows.Select(r => r.Data.File);
        ndsFiles = ndsFiles.Concat(_warningTable.SelectedRows.Select(r => r.Data));

        Layton1NdsFile[] files = [.. ndsFiles];

        if (files.Length <= 0)
            return;

        if (files.Length == 1)
        {
            await ExtractFile(files[0]);
            return;
        }

        await ExtractDirectory(files);
    }

    private void _cancelButton_Clicked(object? sender, EventArgs e)
    {
        _cancelSource.Cancel();
    }

    protected override Task CloseInternal()
    {
        _cancelSource.Cancel();
        return Task.CompletedTask;
    }

    private void Validate()
    {
        _progressBar.Value = 0;
        _progressBar.Minimum = 0;
        _progressBar.Maximum = _ndsRom.Files.Length;

        foreach (Layton1NdsFile file in _ndsRom.Files)
        {
            IncrementValidationProgress();

            if (_cancelSource.IsCancellationRequested)
                break;

            Layton1ValidationError? error = _validator.Validate(file);
            if (error is not null)
            {
                _errorTable.Rows.Add(new DataTableRow<Layton1ValidationError>(error));
                continue;
            }

            object? data = _fileManager.Parse(file, out FileType type);
            if (type is FileType.Anim or FileType.Anim2 && ((AnimationSequences)data!).Frames.Any(s => s.Errors is not AnimationFrameErrorType.None))
                _warningTable.Rows.Add(new DataTableRow<Layton1NdsFile>(file));
        }

        _progressBar.Value = _ndsRom.Files.Length - 1;
        IncrementValidationProgress();

        _isValidating = false;
        _extractButton.Enabled = true;
        _cancelButton.Enabled = false;
    }

    private void IncrementValidationProgress()
    {
        _progressBar.Value++;

        float completion = (float)_progressBar.Value / _ndsRom.Files.Length * 100;
        _progressBar.Text = _localizations.DialogValidationProgress(completion);
    }

    private void IncrementExtractionProgress(int total)
    {
        _progressBar.Value++;

        float completion = (float)_progressBar.Value / total * 100;
        _progressBar.Text = _localizations.DialogDirectoryExtractProgress(completion);
    }

    private async Task ExtractDirectory(Layton1NdsFile[] files)
    {
        string? dirPath = await SelectSaveDirectory();
        if (dirPath is null)
        {
            SetErrorStatus(_localizations.StatusDirectorySelectError);
            return;
        }

        await ExtractDirectory(files, dirPath);

        SetInfoStatus(string.Empty);
    }

    private async Task ExtractDirectory(Layton1NdsFile[] files, string dirPath)
    {
        _progressBar.Maximum = files.Length;
        _progressBar.Value = 0;

        foreach (Layton1NdsFile file in files)
        {
            IncrementExtractionProgress(files.Length);

            await ExtractFile(file, Path.Combine(dirPath, file.Path));
        }

        _progressBar.Value = files.Length - 1;
        IncrementExtractionProgress(files.Length);
    }

    private async Task ExtractFile(Layton1NdsFile file)
    {
        string? filePath = await SelectSaveFile(file);
        if (filePath is null)
        {
            SetErrorStatus(_localizations.StatusFileSelectError);
            return;
        }

        await ExtractFile(file, filePath);

        SetInfoStatus(string.Empty);
    }

    private async Task ExtractFile(Layton1NdsFile file, string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using Stream fileStream = File.Create(filePath);
        await _fileManager.GetUncompressedStream(file).CopyToAsync(fileStream);
    }

    private async Task<string?> SelectSaveFile(Layton1NdsFile file)
    {
        var sfd = new WindowsSaveFileDialog
        {
            Title = _localizations.DialogFileExtractCaption,
            InitialDirectory = _settings.GetExtractDirectory(),
            InitialFileName = Path.GetFileName(file.Path)
        };

        DialogResult result = await sfd.ShowAsync();
        if (result is not DialogResult.Ok)
            return null;

        string? saveDir = Path.GetDirectoryName(sfd.Files[0]);
        if (!string.IsNullOrEmpty(saveDir))
            _settings.SetExtractDirectory(saveDir);

        return sfd.Files[0];
    }

    private async Task<string?> SelectSaveDirectory()
    {
        var sfd = new SelectFolderDialog
        {
            Caption = _localizations.DialogDirectoryExtractCaption,
            Directory = _settings.GetExtractDirectory()
        };

        DialogResult result = await sfd.ShowAsync();

        return result is not DialogResult.Ok ? null : sfd.Directory;
    }

    private void SetErrorStatus(LocalizedString text, Exception? e = null)
    {
        _eventBroker.Raise(new UpdateStatusMessage(text, Status.Error, e));
    }

    private void SetInfoStatus(LocalizedString text)
    {
        _eventBroker.Raise(new UpdateStatusMessage(text, Status.Info, null));
    }
}