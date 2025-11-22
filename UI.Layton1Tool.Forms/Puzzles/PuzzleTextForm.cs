using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms.Puzzles;

internal partial class PuzzleTextForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1PathProvider _pathProvider;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PcmFileManager _pcmManager;

    private Layton1PuzzleId? _puzzleId;
    private TextLanguage? _language;

    public PuzzleTextForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1PathProvider pathProvider, ILayton1NdsFileManager fileManager,
        ILayton1PcmFileManager pcmManager, IFormFactory forms, ILocalizationProvider localizations)
    {
        InitializeComponent(ndsInfo, forms, localizations);

        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;
        _pathProvider = pathProvider;
        _fileManager = fileManager;
        _pcmManager = pcmManager;

        _descriptionBox!.TextChanged += _descriptionBox_TextChanged;
        _correctBox!.TextChanged += _correctBox_TextChanged;
        _incorrectBox!.TextChanged += _incorrectBox_TextChanged;

        eventBroker.Subscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        eventBroker.Subscribe<FileAddedMessage>(ProcessFileAdded);
        eventBroker.Subscribe<SelectedPuzzleChangedMessage>(ProcessSelectedPuzzleChanged);
        eventBroker.Subscribe<SelectedPuzzleLanguageChangedMessage>(ProcessSelectedPuzzleLanguageChanged);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        _eventBroker.Unsubscribe<FileAddedMessage>(ProcessFileAdded);
        _eventBroker.Unsubscribe<SelectedPuzzleChangedMessage>(ProcessSelectedPuzzleChanged);
        _eventBroker.Unsubscribe<SelectedPuzzleLanguageChangedMessage>(ProcessSelectedPuzzleLanguageChanged);
    }

    #region EventBroker

    private void RaiseFileContentModified(Layton1NdsFile file, object? content)
    {
        _eventBroker.Raise(new FileContentModifiedMessage(this, file, content));
    }

    private void RaiseFileAdded(Layton1NdsFile file)
    {
        _eventBroker.Raise(new FileAddedMessage(this, file));
    }

    private void RaiseSelectedPuzzleModified(Layton1PuzzleId puzzleId)
    {
        _eventBroker.Raise(new SelectedPuzzleModifiedMessage(_ndsInfo.Rom, puzzleId));
    }

    private void RaiseSelectedPuzzleDescriptionTextModified(Layton1PuzzleId puzzleId, string hint1)
    {
        _eventBroker.Raise(new SelectedPuzzleDescriptionTextModifiedMessage(_ndsInfo.Rom, puzzleId, hint1));
    }

    private void RaiseSelectedPuzzleCorrectTextModified(Layton1PuzzleId puzzleId, string hint2)
    {
        _eventBroker.Raise(new SelectedPuzzleCorrectTextModifiedMessage(_ndsInfo.Rom, puzzleId, hint2));
    }

    private void RaiseSelectedPuzzleIncorrectTextModified(Layton1PuzzleId puzzleId, string hint3)
    {
        _eventBroker.Raise(new SelectedPuzzleIncorrectTextModifiedMessage(_ndsInfo.Rom, puzzleId, hint3));
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (_puzzleId is null || _language is null)
            return;

        if (message.Source == this)
            return;

        UpdateChangedTexts(_puzzleId, message.File, _language.Value);
        RaiseSelectedPuzzleModified(_puzzleId);
    }

    private void ProcessFileAdded(FileAddedMessage message)
    {
        if (_puzzleId is null || _language is null)
            return;

        if (message.Source == this)
            return;

        UpdateChangedTexts(_puzzleId, message.File, _language.Value);
        RaiseSelectedPuzzleModified(_puzzleId);
    }

    private void ProcessSelectedPuzzleChanged(SelectedPuzzleChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        _language = message.Language;

        UpdatePuzzle(message.Puzzle, message.Language);
    }

    private void ProcessSelectedPuzzleLanguageChanged(SelectedPuzzleLanguageChangedMessage message)
    {
        if (_puzzleId is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        _language = message.Language;

        UpdatePuzzle(_puzzleId, message.Language);
    }

    #endregion

    #region Events

    private void _descriptionBox_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _language is null)
            return;

        string description = _descriptionBox.GetText();

        ModifyText(_puzzleId, _language.Value, $"q_{_puzzleId.InternalId}.txt", description);

        RaiseSelectedPuzzleDescriptionTextModified(_puzzleId, description);

        RaiseSelectedPuzzleModified(_puzzleId);
    }

    private void _correctBox_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _language is null)
            return;

        string correct = _correctBox.GetText();

        ModifyText(_puzzleId, _language.Value, $"c_{_puzzleId.InternalId}.txt", correct);

        RaiseSelectedPuzzleCorrectTextModified(_puzzleId, correct);

        RaiseSelectedPuzzleModified(_puzzleId);
    }

    private void _incorrectBox_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _language is null)
            return;

        string incorrect = _incorrectBox.GetText();

        ModifyText(_puzzleId, _language.Value, $"f_{_puzzleId.InternalId}.txt", incorrect);

        RaiseSelectedPuzzleIncorrectTextModified(_puzzleId, incorrect);

        RaiseSelectedPuzzleModified(_puzzleId);
    }

    #endregion

    #region Updates

    private void UpdatePuzzle(Layton1PuzzleId puzzleId, TextLanguage language)
    {
        _puzzleId = puzzleId;

        UpdateTexts(puzzleId, language);

        RaiseSelectedPuzzleDescriptionTextModified(puzzleId, _descriptionBox.GetText());
        RaiseSelectedPuzzleCorrectTextModified(puzzleId, _correctBox.GetText());
        RaiseSelectedPuzzleIncorrectTextModified(puzzleId, _incorrectBox.GetText());
    }

    private void UpdateChangedTexts(Layton1PuzzleId puzzleId, Layton1NdsFile file, TextLanguage language)
    {
        string filePath = GetBaseTextPath(puzzleId, _ndsInfo.Rom.Version, language);
        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                if (file.Path == filePath + $"q_{puzzleId.InternalId}.txt")
                {
                    UpdateTexts(puzzleId, language);
                    RaiseSelectedPuzzleDescriptionTextModified(puzzleId, _descriptionBox.GetText());
                }

                if (file.Path == filePath + $"c_{puzzleId.InternalId}.txt")
                {
                    UpdateTexts(puzzleId, language);
                    RaiseSelectedPuzzleCorrectTextModified(puzzleId, _correctBox.GetText());
                }

                if (file.Path == filePath + $"f_{puzzleId.InternalId}.txt")
                {
                    UpdateTexts(puzzleId, language);
                    RaiseSelectedPuzzleIncorrectTextModified(puzzleId, _incorrectBox.GetText());
                }
                break;

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
            case GameVersion.JapanFriendly:
                if (file.Path == filePath)
                {
                    UpdateTexts(puzzleId, language);
                    RaiseSelectedPuzzleDescriptionTextModified(puzzleId, _descriptionBox.GetText());
                    RaiseSelectedPuzzleCorrectTextModified(puzzleId, _correctBox.GetText());
                    RaiseSelectedPuzzleIncorrectTextModified(puzzleId, _incorrectBox.GetText());
                }
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }
    }

    private void UpdateTexts(Layton1PuzzleId puzzleId, TextLanguage language)
    {
        string? description;
        string? correct;
        string? incorrect;

        string filePath = GetBaseTextPath(puzzleId, _ndsInfo.Rom.Version, language);
        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                description = GetText(filePath + $"q_{puzzleId.InternalId}.txt");
                correct = GetText(filePath + $"c_{puzzleId.InternalId}.txt");
                incorrect = GetText(filePath + $"f_{puzzleId.InternalId}.txt");
                break;

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
            case GameVersion.JapanFriendly:
                description = GetPcmText(filePath, $"q_{puzzleId.InternalId}.txt");
                correct = GetPcmText(filePath, $"c_{puzzleId.InternalId}.txt");
                incorrect = GetPcmText(filePath, $"f_{puzzleId.InternalId}.txt");
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        UpdateDescriptionText(description);
        UpdateCorrectText(correct);
        UpdateIncorrectText(incorrect);
    }

    private void UpdateDescriptionText(string? description)
    {
        _descriptionBox.TextChanged -= _descriptionBox_TextChanged;

        _descriptionBox.IsReadOnly = false;
        _descriptionBox.SetText(description ?? string.Empty);

        _descriptionBox.TextChanged += _descriptionBox_TextChanged;
    }

    private void UpdateCorrectText(string? correct)
    {
        _correctBox.TextChanged -= _correctBox_TextChanged;

        _correctBox.IsReadOnly = false;
        _correctBox.SetText(correct ?? string.Empty);

        _correctBox.TextChanged += _correctBox_TextChanged;
    }

    private void UpdateIncorrectText(string? incorrect)
    {
        _incorrectBox.TextChanged -= _incorrectBox_TextChanged;

        _incorrectBox.IsReadOnly = false;
        _incorrectBox.SetText(incorrect ?? string.Empty);

        _incorrectBox.TextChanged += _incorrectBox_TextChanged;
    }

    #endregion

    #region Text Modification

    private void ModifyText(Layton1PuzzleId puzzleId, TextLanguage language, string fileName, string text)
    {
        Layton1NdsFile? textFile;
        string textPath;

        string filePath = GetBaseTextPath(puzzleId, _ndsInfo.Rom.Version, language);
        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                textPath = filePath + fileName;

                if (_fileManager.TryGet(_ndsInfo.Rom, textPath, out textFile))
                {
                    _fileManager.Compose(textFile, text, FileType.Text);
                    RaiseFileContentModified(textFile, text);
                }
                else
                {
                    textFile = _fileManager.Add(_ndsInfo.Rom, textPath, text, FileType.Text, CompressionType.Level5Lz10);
                    RaiseFileAdded(textFile);
                }

                break;

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
            case GameVersion.JapanFriendly:
                textPath = fileName;

                if (_fileManager.TryGet(_ndsInfo.Rom, filePath, out textFile))
                {
                    var files = (List<PcmFile>?)_fileManager.Parse(textFile, FileType.Pcm);

                    if (files is null)
                        break;

                    if (_pcmManager.TryGet(files, textPath, out PcmFile? file))
                        _pcmManager.Compose(file, text, FileType.Text, _ndsInfo.Rom.Version);
                    else
                        _ = _pcmManager.Add(files, textPath, text, FileType.Text, _ndsInfo.Rom.Version);

                    _fileManager.Compose(textFile, files, FileType.Pcm);
                    RaiseFileContentModified(textFile, files);
                }
                else
                {
                    var files = new List<PcmFile>();
                    _pcmManager.Add(files, textPath, text, FileType.Text, _ndsInfo.Rom.Version);

                    textFile = _fileManager.Add(_ndsInfo.Rom, filePath, files, FileType.Pcm, CompressionType.Level5Lz10);
                    RaiseFileAdded(textFile);
                }

                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }
    }

    #endregion

    #region Helper

    private string GetBaseTextPath(Layton1PuzzleId puzzleId, GameVersion version, TextLanguage language)
    {
        switch (version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                return _pathProvider.GetFullDirectory("qtext/", version, language);

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
                string group = puzzleId.InternalId < 50 ? "000" : puzzleId.InternalId < 100 ? "050" : "100";
                return _pathProvider.GetFullDirectory("qtext/", version, language) + $"q{group}.pcm";

            case GameVersion.JapanFriendly:
                return _pathProvider.GetFullDirectory("qtext/", version, language) + $"q{puzzleId.InternalId / 50 * 50:000}.pcm";

            default:
                throw new InvalidOperationException($"Unknown game version {version}.");
        }
    }

    private string? GetText(string filePath)
    {
        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? ndsFile))
            return null;

        return _fileManager.Parse(ndsFile, FileType.Text) as string;
    }

    private string? GetPcmText(string filePath, string entryName)
    {
        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? ndsFile))
            return null;

        if (_fileManager.Parse(ndsFile, FileType.Pcm) is not List<PcmFile> pcmFiles)
            return null;

        PcmFile? pcmFile = pcmFiles.FirstOrDefault(f => f.Name == entryName);

        if (pcmFile is null)
            return null;

        return _pcmManager.Parse(pcmFile, FileType.Text, _ndsInfo.Rom.Version) as string;
    }

    #endregion
}