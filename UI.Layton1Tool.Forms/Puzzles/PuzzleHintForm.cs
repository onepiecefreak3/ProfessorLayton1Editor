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

internal partial class PuzzleHintForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1PathProvider _pathProvider;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PcmFileManager _pcmManager;

    private Layton1PuzzleId? _puzzleId;
    private TextLanguage? _language;

    public PuzzleHintForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1PathProvider pathProvider, ILayton1NdsFileManager fileManager,
        ILayton1PcmFileManager pcmManager, IFormFactory forms, ILocalizationProvider localizations)
    {
        InitializeComponent(ndsInfo, forms, localizations);

        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;
        _pathProvider = pathProvider;
        _fileManager = fileManager;
        _pcmManager = pcmManager;

        _hint1Box!.TextChanged += _hint1Box_TextChanged;
        _hint2Box!.TextChanged += _hint2Box_TextChanged;
        _hint3Box!.TextChanged += _hint3Box_TextChanged;

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

    private void RaiseSelectedPuzzleHint1TextModified(Layton1PuzzleId puzzleId, string hint1)
    {
        _eventBroker.Raise(new SelectedPuzzleHint1TextModifiedMessage(_ndsInfo.Rom, puzzleId, hint1));
    }

    private void RaiseSelectedPuzzleHint2TextModified(Layton1PuzzleId puzzleId, string hint2)
    {
        _eventBroker.Raise(new SelectedPuzzleHint2TextModifiedMessage(_ndsInfo.Rom, puzzleId, hint2));
    }

    private void RaiseSelectedPuzzleHint3TextModified(Layton1PuzzleId puzzleId, string hint3)
    {
        _eventBroker.Raise(new SelectedPuzzleHint3TextModifiedMessage(_ndsInfo.Rom, puzzleId, hint3));
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

    private void _hint1Box_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _language is null)
            return;

        string hint1 = _hint1Box.GetText();

        ModifyText(_puzzleId, _language.Value, $"h_{_puzzleId.InternalId}_1.txt", hint1);

        RaiseSelectedPuzzleHint1TextModified(_puzzleId, hint1);

        RaiseSelectedPuzzleModified(_puzzleId);
    }

    private void _hint2Box_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _language is null)
            return;

        string hint2 = _hint2Box.GetText();

        ModifyText(_puzzleId, _language.Value, $"h_{_puzzleId.InternalId}_2.txt", hint2);

        RaiseSelectedPuzzleHint2TextModified(_puzzleId, hint2);

        RaiseSelectedPuzzleModified(_puzzleId);
    }

    private void _hint3Box_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _language is null)
            return;

        string hint3 = _hint3Box.GetText();

        ModifyText(_puzzleId, _language.Value, $"h_{_puzzleId.InternalId}_3.txt", hint3);

        RaiseSelectedPuzzleHint3TextModified(_puzzleId, hint3);

        RaiseSelectedPuzzleModified(_puzzleId);
    }

    #endregion

    #region Updates

    private void UpdatePuzzle(Layton1PuzzleId puzzleId, TextLanguage language)
    {
        _puzzleId = puzzleId;

        UpdateTexts(puzzleId, language);

        RaiseSelectedPuzzleHint1TextModified(puzzleId, _hint1Box.GetText());
        RaiseSelectedPuzzleHint2TextModified(puzzleId, _hint2Box.GetText());
        RaiseSelectedPuzzleHint3TextModified(puzzleId, _hint3Box.GetText());
    }

    private void UpdateChangedTexts(Layton1PuzzleId puzzleId, Layton1NdsFile file, TextLanguage language)
    {
        string filePath = GetBaseTextPath(puzzleId, _ndsInfo.Rom.Version, language);
        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                if (file.Path == filePath + $"h_{puzzleId.InternalId}_1.txt")
                {
                    UpdateTexts(puzzleId, language);
                    RaiseSelectedPuzzleHint1TextModified(puzzleId, _hint1Box.GetText());
                }

                if (file.Path == filePath + $"h_{puzzleId.InternalId}_2.txt")
                {
                    UpdateTexts(puzzleId, language);
                    RaiseSelectedPuzzleHint2TextModified(puzzleId, _hint2Box.GetText());
                }

                if (file.Path == filePath + $"h_{puzzleId.InternalId}_3.txt")
                {
                    UpdateTexts(puzzleId, language);
                    RaiseSelectedPuzzleHint3TextModified(puzzleId, _hint3Box.GetText());
                }
                break;

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
            case GameVersion.JapanFriendly:
                if (file.Path == filePath)
                {
                    UpdateTexts(puzzleId, language);
                    RaiseSelectedPuzzleHint1TextModified(puzzleId, _hint1Box.GetText());
                    RaiseSelectedPuzzleHint2TextModified(puzzleId, _hint2Box.GetText());
                    RaiseSelectedPuzzleHint3TextModified(puzzleId, _hint3Box.GetText());
                }
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }
    }

    private void UpdateTexts(Layton1PuzzleId puzzleId, TextLanguage language)
    {
        string? hint1;
        string? hint2;
        string? hint3;

        string filePath = GetBaseTextPath(puzzleId, _ndsInfo.Rom.Version, language);
        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                hint1 = GetText(filePath + $"h_{puzzleId.InternalId}_1.txt");
                hint2 = GetText(filePath + $"h_{puzzleId.InternalId}_2.txt");
                hint3 = GetText(filePath + $"h_{puzzleId.InternalId}_3.txt");
                break;

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
            case GameVersion.JapanFriendly:
                hint1 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_1.txt");
                hint2 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_2.txt");
                hint3 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_3.txt");
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        UpdateHint1Text(hint1);
        UpdateHint2Text(hint2);
        UpdateHint3Text(hint3);
    }

    private void UpdateHint1Text(string? hint1)
    {
        _hint1Box.TextChanged -= _hint1Box_TextChanged;

        _hint1Box.IsReadOnly = false;
        _hint1Box.SetText(hint1 ?? string.Empty);

        _hint1Box.TextChanged += _hint1Box_TextChanged;
    }

    private void UpdateHint2Text(string? hint2)
    {
        _hint2Box.TextChanged -= _hint2Box_TextChanged;

        _hint2Box.IsReadOnly = false;
        _hint2Box.SetText(hint2 ?? string.Empty);

        _hint2Box.TextChanged += _hint2Box_TextChanged;
    }

    private void UpdateHint3Text(string? hint3)
    {
        _hint3Box.TextChanged -= _hint3Box_TextChanged;

        _hint3Box.IsReadOnly = false;
        _hint3Box.SetText(hint3 ?? string.Empty);

        _hint3Box.TextChanged += _hint3Box_TextChanged;
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