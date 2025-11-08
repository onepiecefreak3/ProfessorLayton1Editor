using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms;

partial class PuzzleInfoForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1PuzzleIdProvider _puzzleIdProvider;
    private readonly ILayton1PathProvider _pathProvider;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PcmFileManager _pcmManager;

    private TextLanguage _language = TextLanguage.English;

    private Layton1PuzzleId? _puzzleId;
    private Layton1PuzzleFileReference? _titleFile;
    private Layton1PuzzleFileReference? _titleScriptFile;
    private Layton1PuzzleFileReference? _infoScriptFile;
    private Layton1PuzzleFileReference? _descriptionFile;
    private Layton1PuzzleFileReference? _correctFile;
    private Layton1PuzzleFileReference? _incorrectFile;
    private Layton1PuzzleFileReference? _hint1File;
    private Layton1PuzzleFileReference? _hint2File;
    private Layton1PuzzleFileReference? _hint3File;
    private Layton1PuzzleFileReference? _picaratFile;

    public PuzzleInfoForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1PuzzleIdProvider puzzleIdProvider, ILayton1PathProvider pathProvider,
        ILayton1NdsFileManager fileManager, ILayton1PcmFileManager pcmManager, ILocalizationProvider localizations)
    {
        InitializeComponent(localizations);

        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;
        _puzzleIdProvider = puzzleIdProvider;
        _pathProvider = pathProvider;
        _fileManager = fileManager;
        _pcmManager = pcmManager;

        _numberBox!.TextChanged += _numberBox_TextChanged;
        _titleBox!.TextChanged += _titleBox_TextChanged;
        _descriptionBox!.TextChanged += _descriptionBox_TextChanged;
        _correctBox!.TextChanged += _correctBox_TextChanged;
        _incorrectBox!.TextChanged += _incorrectBox_TextChanged;
        _hint1Box!.TextChanged += _hint1Box_TextChanged;
        _hint2Box!.TextChanged += _hint2Box_TextChanged;
        _hint3Box!.TextChanged += _hint3Box_TextChanged;
        _locationBox!.TextChanged += _locationBox_TextChanged;
        _typeBox!.TextChanged += _typeBox_TextChanged;
        _picarat1Box!.TextChanged += _picarat1Box_TextChanged;
        _picarat2Box!.TextChanged += _picarat2Box_TextChanged;
        _picarat3Box!.TextChanged += _picarat3Box_TextChanged;

        eventBroker.Subscribe<SelectedPuzzleChangedMessage>(UpdatePuzzle);
        eventBroker.Subscribe<SelectedPuzzleLanguageChangedMessage>(UpdateLanguage);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedPuzzleChangedMessage>(UpdatePuzzle);
        _eventBroker.Unsubscribe<SelectedPuzzleLanguageChangedMessage>(UpdateLanguage);
    }

    private void _picarat1Box_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        if (!int.TryParse(_picarat1Box.Text, out int picarat))
            return;

        if (_picaratFile?.Content is not GdsScriptFile picaratScript)
            return;

        foreach (GdsScriptInstruction instruction in picaratScript.Instructions)
        {
            if (instruction.Type is not 0)
                continue;

            if (instruction.Arguments.Length is not 5)
                continue;

            if (instruction.Arguments[0].Value is not 195)
                continue;

            if (instruction.Arguments[1].Value is not int internalId || internalId != _puzzleId.InternalId)
                continue;

            instruction.Arguments[2].Value = picarat;
            _fileManager.Compose(_picaratFile.File, _picaratFile.Content);

            RaiseFileContentModified(_picaratFile.File, _picaratFile.Content);
            return;
        }
    }

    private void _picarat2Box_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        if (!int.TryParse(_picarat2Box.Text, out int picarat))
            return;

        if (_picaratFile?.Content is not GdsScriptFile picaratScript)
            return;

        foreach (GdsScriptInstruction instruction in picaratScript.Instructions)
        {
            if (instruction.Type is not 0)
                continue;

            if (instruction.Arguments.Length is not 5)
                continue;

            if (instruction.Arguments[0].Value is not 195)
                continue;

            if (instruction.Arguments[1].Value is not int internalId || internalId != _puzzleId.InternalId)
                continue;

            instruction.Arguments[3].Value = picarat;
            _fileManager.Compose(_picaratFile.File, _picaratFile.Content);

            RaiseFileContentModified(_picaratFile.File, _picaratFile.Content);
            return;
        }
    }

    private void _picarat3Box_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        if (!int.TryParse(_picarat3Box.Text, out int picarat))
            return;

        if (_picaratFile?.Content is not GdsScriptFile picaratScript)
            return;

        foreach (GdsScriptInstruction instruction in picaratScript.Instructions)
        {
            if (instruction.Type is not 0)
                continue;

            if (instruction.Arguments.Length is not 5)
                continue;

            if (instruction.Arguments[0].Value is not 195)
                continue;

            if (instruction.Arguments[1].Value is not int internalId || internalId != _puzzleId.InternalId)
                continue;

            instruction.Arguments[4].Value = picarat;
            _fileManager.Compose(_picaratFile.File, _picaratFile.Content);

            RaiseFileContentModified(_picaratFile.File, _picaratFile.Content);
            return;
        }
    }

    private void _typeBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        if (_infoScriptFile?.Content is not GdsScriptFile infoScript)
            return;

        foreach (GdsScriptInstruction instruction in infoScript.Instructions)
        {
            if (instruction.Type is not 0)
                continue;

            if (instruction.Arguments.Length is not 4)
                continue;

            if (instruction.Arguments[0].Value is not 220)
                continue;

            if (instruction.Arguments[1].Value is not int internalId || internalId != _puzzleId.InternalId)
                continue;

            instruction.Arguments[2].Value = _typeBox.Text;
            _fileManager.Compose(_infoScriptFile.File, _infoScriptFile.Content);

            RaiseFileContentModified(_infoScriptFile.File, _infoScriptFile.Content);
            return;
        }
    }

    private void _locationBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        if (_infoScriptFile?.Content is not GdsScriptFile infoScript)
            return;

        foreach (GdsScriptInstruction instruction in infoScript.Instructions)
        {
            if (instruction.Type is not 0)
                continue;

            if (instruction.Arguments.Length is not 4)
                continue;

            if (instruction.Arguments[0].Value is not 220)
                continue;

            if (instruction.Arguments[1].Value is not int internalId || internalId != _puzzleId.InternalId)
                continue;

            instruction.Arguments[3].Value = _locationBox.Text;
            _fileManager.Compose(_infoScriptFile.File, _infoScriptFile.Content);

            RaiseFileContentModified(_infoScriptFile.File, _infoScriptFile.Content);
            return;
        }
    }

    private void _hint1Box_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _hint1File is null)
            return;

        string hint1 = _hint1Box.GetText();

        if (_hint1File.PcmFile is null)
        {
            _hint1File.Content = hint1;
            _fileManager.Compose(_hint1File.File, hint1);

            RaiseFileContentModified(_hint1File.File, hint1);
        }
        else
        {
            _pcmManager.Compose(_hint1File.PcmFile, hint1, FileType.Text);
            _fileManager.Compose(_hint1File.File, _hint1File.Content);

            RaiseFileContentModified(_hint1File.File, _hint1File.Content);
        }
    }

    private void _hint2Box_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _hint2File is null)
            return;

        string hint2 = _hint2Box.GetText();

        if (_hint2File.PcmFile is null)
        {
            _hint2File.Content = hint2;
            _fileManager.Compose(_hint2File.File, hint2);

            RaiseFileContentModified(_hint2File.File, hint2);
        }
        else
        {
            _pcmManager.Compose(_hint2File.PcmFile, hint2, FileType.Text);
            _fileManager.Compose(_hint2File.File, _hint2File.Content);

            RaiseFileContentModified(_hint2File.File, _hint2File.Content);
        }
    }

    private void _hint3Box_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _hint3File is null)
            return;

        string hint3 = _hint3Box.GetText();

        if (_hint3File.PcmFile is null)
        {
            _hint3File.Content = hint3;
            _fileManager.Compose(_hint3File.File, hint3);

            RaiseFileContentModified(_hint3File.File, hint3);
        }
        else
        {
            _pcmManager.Compose(_hint3File.PcmFile, hint3, FileType.Text);
            _fileManager.Compose(_hint3File.File, _hint3File.Content);

            RaiseFileContentModified(_hint3File.File, _hint3File.Content);
        }
    }

    private void _numberBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        if (!_fileManager.TryGet(_ndsInfo.Rom, "sys/arm9.bin", out Layton1NdsFile? arm9File))
            return;

        if (!int.TryParse(_numberBox.Text, out int number))
            return;

        _puzzleId.Number = number;
        _puzzleIdProvider.Set(_ndsInfo.Rom, _puzzleId);

        RaiseFileContentModified(arm9File, _fileManager.GetUncompressedStream(arm9File));
    }

    private void _incorrectBox_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _incorrectFile is null)
            return;

        string incorrect = _incorrectBox.GetText();

        if (_incorrectFile.PcmFile is null)
        {
            _incorrectFile.Content = incorrect;
            _fileManager.Compose(_incorrectFile.File, incorrect);

            RaiseFileContentModified(_incorrectFile.File, incorrect);
        }
        else
        {
            _pcmManager.Compose(_incorrectFile.PcmFile, incorrect, FileType.Text);
            _fileManager.Compose(_incorrectFile.File, _incorrectFile.Content);

            RaiseFileContentModified(_incorrectFile.File, _incorrectFile.Content);
        }
    }

    private void _correctBox_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _correctFile is null)
            return;

        string correct = _correctBox.GetText();

        if (_correctFile.PcmFile is null)
        {
            _correctFile.Content = correct;
            _fileManager.Compose(_correctFile.File, correct);

            RaiseFileContentModified(_correctFile.File, correct);
        }
        else
        {
            _pcmManager.Compose(_correctFile.PcmFile, correct, FileType.Text);
            _fileManager.Compose(_correctFile.File, _correctFile.Content);

            RaiseFileContentModified(_correctFile.File, _correctFile.Content);
        }
    }

    private void _descriptionBox_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null || _descriptionFile is null)
            return;

        string description = _descriptionBox.GetText();

        if (_descriptionFile.PcmFile is null)
        {
            _descriptionFile.Content = description;
            _fileManager.Compose(_descriptionFile.File, description);

            RaiseFileContentModified(_descriptionFile.File, description);
        }
        else
        {
            _pcmManager.Compose(_descriptionFile.PcmFile, description, FileType.Text);
            _fileManager.Compose(_descriptionFile.File, _descriptionFile.Content);

            RaiseFileContentModified(_descriptionFile.File, _descriptionFile.Content);
        }
    }

    private void _titleBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null || _titleFile is null)
            return;

        if (_titleFile.PcmFile is null)
        {
            _titleFile.Content = _titleBox.Text;
            _fileManager.Compose(_titleFile.File, _titleBox.Text);

            RaiseFileContentModified(_titleFile.File, _titleBox.Text);
        }
        else
        {
            _pcmManager.Compose(_titleFile.PcmFile, _titleBox.Text, FileType.Text);
            _fileManager.Compose(_titleFile.File, _titleFile.Content);

            RaiseFileContentModified(_titleFile.File, _titleFile.Content);
        }

        if (_titleScriptFile is null)
            return;

        foreach (var instruction in ((GdsScriptFile)_titleScriptFile.Content).Instructions)
        {
            if (instruction.Type is not 0 || instruction.Arguments.Length is not 3 || instruction.Arguments[0].Value is not 186)
                continue;

            if (instruction.Arguments[1].Value is not int internalId || _puzzleId.InternalId != internalId)
                continue;

            instruction.Arguments[2].Value = _titleBox.Text;
            _fileManager.Compose(_titleScriptFile.File, _titleScriptFile.Content);

            RaiseFileContentModified(_titleScriptFile.File, _titleScriptFile.Content);
            break;
        }
    }

    private void RaiseFileContentModified(Layton1NdsFile file, object? content)
    {
        _eventBroker.Raise(new FileContentModifiedMessage(this, file, content));
    }

    private void UpdatePuzzle(SelectedPuzzleChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        UpdatePuzzle(message.Puzzle);
    }

    private void UpdateLanguage(SelectedPuzzleLanguageChangedMessage message)
    {
        if (_puzzleId is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        _language = message.Language;

        UpdatePuzzle(_puzzleId);
    }

    private void UpdatePuzzle(Layton1PuzzleId puzzleId)
    {
        _puzzleId = puzzleId;

        _internalIdBox.Text = $"{puzzleId.InternalId}";

        UpdateNumber(puzzleId);
        UpdateTexts(puzzleId);
        UpdatePicarats(puzzleId);
    }

    private void UpdateNumber(Layton1PuzzleId puzzleId)
    {
        _numberBox.TextChanged -= _numberBox_TextChanged;

        _numberBox.IsReadOnly = puzzleId.IsWifi;
        _numberBox.Text = puzzleId.IsWifi ? $"W{puzzleId.Number:00}" : $"{puzzleId.Number}";

        _numberBox.TextChanged += _numberBox_TextChanged;
    }

    private void UpdateTexts(Layton1PuzzleId puzzleId)
    {
        string? correct;
        string? incorrect;
        string? hint1;
        string? hint2;
        string? hint3;
        string? description;
        string? title;

        string filePath = _pathProvider.GetFullDirectory("script/puzzletitle/", _ndsInfo.Rom.Version, _language) + "qtitle.gds";
        if (_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? titleScriptFile))
        {
            object? titleScript = _fileManager.Parse(titleScriptFile, FileType.Gds);
            if (titleScript is not null)
            {
                _titleScriptFile = new Layton1PuzzleFileReference
                {
                    File = titleScriptFile,
                    Content = titleScript
                };
            }
        }

        filePath = _pathProvider.GetFullDirectory("script/qinfo/", _ndsInfo.Rom.Version, _language) + "qscript.gds";
        if (_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? infoScriptFile))
        {
            object? titleScript = _fileManager.Parse(infoScriptFile, FileType.Gds);
            if (titleScript is GdsScriptFile infoScript)
            {
                _infoScriptFile = new Layton1PuzzleFileReference
                {
                    File = infoScriptFile,
                    Content = titleScript
                };

                UpdateInfoTexts(puzzleId, infoScript);
            }
        }

        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                filePath = _pathProvider.GetFullDirectory("qtext/", _ndsInfo.Rom.Version, _language) + $"c_{puzzleId.InternalId}.txt";
                correct = GetText(filePath, out _correctFile);

                filePath = _pathProvider.GetFullDirectory("qtext/", _ndsInfo.Rom.Version, _language) + $"f_{puzzleId.InternalId}.txt";
                incorrect = GetText(filePath, out _incorrectFile);

                filePath = _pathProvider.GetFullDirectory("qtext/", _ndsInfo.Rom.Version, _language) + $"h_{puzzleId.InternalId}_";
                hint1 = GetText(filePath + "1.txt", out _hint1File);
                hint2 = GetText(filePath + "2.txt", out _hint2File);
                hint3 = GetText(filePath + "3.txt", out _hint3File);

                filePath = _pathProvider.GetFullDirectory("qtext/", _ndsInfo.Rom.Version, _language) + $"q_{puzzleId.InternalId}.txt";
                description = GetText(filePath, out _descriptionFile);

                filePath = _pathProvider.GetFullDirectory("qtext/", _ndsInfo.Rom.Version, _language) + $"t_{puzzleId.InternalId}.txt";
                title = GetText(filePath, out _titleFile);
                break;

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
                string group = puzzleId.InternalId < 50 ? "000" : puzzleId.InternalId < 100 ? "050" : "100";
                filePath = _pathProvider.GetFullDirectory("qtext/", _ndsInfo.Rom.Version, _language) + $"q{group}.pcm";

                correct = GetPcmText(filePath, $"c_{puzzleId.InternalId}.txt", out _correctFile);
                incorrect = GetPcmText(filePath, $"f_{puzzleId.InternalId}.txt", out _incorrectFile);
                hint1 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_1.txt", out _hint1File);
                hint2 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_2.txt", out _hint2File);
                hint3 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_3.txt", out _hint3File);
                description = GetPcmText(filePath, $"q_{puzzleId.InternalId}.txt", out _descriptionFile);
                title = GetPcmText(filePath, $"t_{puzzleId.InternalId}.txt", out _titleFile);
                break;

            case GameVersion.JapanFriendly:
                filePath = _pathProvider.GetFullDirectory("qtext/", _ndsInfo.Rom.Version, _language) + $"q{puzzleId.InternalId / 50 * 50:000}.pcm";

                correct = GetPcmText(filePath, $"c_{puzzleId.InternalId}.txt", out _correctFile);
                incorrect = GetPcmText(filePath, $"f_{puzzleId.InternalId}.txt", out _incorrectFile);
                hint1 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_1.txt", out _hint1File);
                hint2 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_2.txt", out _hint2File);
                hint3 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_3.txt", out _hint3File);
                description = GetPcmText(filePath, $"q_{puzzleId.InternalId}.txt", out _descriptionFile);
                title = GetPcmText(filePath, $"t_{puzzleId.InternalId}.txt", out _titleFile);
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        UpdateCorrectText(correct);
        UpdateIncorrectText(incorrect);
        UpdateHintTexts(hint1, hint2, hint3);
        UpdateDescriptionText(description);
        UpdateTitleText(title);
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

    private void UpdateHintTexts(string? hint1, string? hint2, string? hint3)
    {
        _hint1Box.TextChanged -= _hint1Box_TextChanged;
        _hint2Box.TextChanged -= _hint2Box_TextChanged;
        _hint3Box.TextChanged -= _hint3Box_TextChanged;

        _hint1Box.IsReadOnly = false;
        _hint1Box.SetText(hint1 ?? string.Empty);

        _hint2Box.IsReadOnly = false;
        _hint2Box.SetText(hint2 ?? string.Empty);

        _hint3Box.IsReadOnly = false;
        _hint3Box.SetText(hint3 ?? string.Empty);

        _hint1Box.TextChanged += _hint1Box_TextChanged;
        _hint2Box.TextChanged += _hint2Box_TextChanged;
        _hint3Box.TextChanged += _hint3Box_TextChanged;
    }

    private void UpdateDescriptionText(string? description)
    {
        _descriptionBox.TextChanged -= _descriptionBox_TextChanged;

        _descriptionBox.IsReadOnly = false;
        _descriptionBox.SetText(description ?? string.Empty);

        _descriptionBox.TextChanged += _descriptionBox_TextChanged;
    }

    private void UpdateTitleText(string? title)
    {
        _titleBox.TextChanged -= _titleBox_TextChanged;

        _titleBox.IsReadOnly = false;
        _titleBox.Text = title ?? string.Empty;

        _titleBox.TextChanged += _titleBox_TextChanged;
    }

    private void UpdateInfoTexts(Layton1PuzzleId puzzleId, GdsScriptFile infoScript)
    {
        foreach (GdsScriptInstruction instruction in infoScript.Instructions)
        {
            if (instruction.Type is not 0)
                continue;

            if (instruction.Arguments.Length is not 4)
                continue;

            if (instruction.Arguments[0].Value is not 220)
                continue;

            if (instruction.Arguments[1].Value is not int internalId || internalId != puzzleId.InternalId)
                continue;

            _locationBox.TextChanged -= _locationBox_TextChanged;
            _typeBox.TextChanged -= _typeBox_TextChanged;

            _locationBox.Text = instruction.Arguments[2].Value as string ?? string.Empty;
            _typeBox.Text = instruction.Arguments[3].Value as string ?? string.Empty;

            _locationBox.IsReadOnly = false;
            _typeBox.IsReadOnly = false;

            _locationBox.TextChanged += _locationBox_TextChanged;
            _typeBox.TextChanged += _typeBox_TextChanged;

            return;
        }
    }

    private void UpdatePicarats(Layton1PuzzleId puzzleId)
    {
        var filePath = _pathProvider.GetFullDirectory("script/pcarot/pscript.gds", _ndsInfo.Rom.Version);
        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? picaratFile))
            return;

        if (_fileManager.Parse(picaratFile, FileType.Gds) is not GdsScriptFile picaratScript)
            return;

        _picaratFile = new Layton1PuzzleFileReference
        {
            File = picaratFile,
            Content = picaratScript
        };

        foreach (GdsScriptInstruction instruction in picaratScript.Instructions)
        {
            if (instruction.Type is not 0)
                continue;

            if (instruction.Arguments.Length is not 5)
                continue;

            if (instruction.Arguments[0].Value is not 195)
                continue;

            if (instruction.Arguments[1].Value is not int internalId || internalId != puzzleId.InternalId)
                continue;

            _picarat1Box.TextChanged -= _picarat1Box_TextChanged;
            _picarat2Box.TextChanged -= _picarat2Box_TextChanged;
            _picarat3Box.TextChanged -= _picarat3Box_TextChanged;

            _picarat1Box.Text = instruction.Arguments[2].Value is int picarat1 ? $"{picarat1}" : string.Empty;
            _picarat2Box.Text = instruction.Arguments[3].Value is int picarat2 ? $"{picarat2}" : string.Empty;
            _picarat3Box.Text = instruction.Arguments[4].Value is int picarat3 ? $"{picarat3}" : string.Empty;

            _picarat1Box.IsReadOnly = false;
            _picarat2Box.IsReadOnly = false;
            _picarat3Box.IsReadOnly = false;

            _picarat1Box.TextChanged += _picarat1Box_TextChanged;
            _picarat2Box.TextChanged += _picarat2Box_TextChanged;
            _picarat3Box.TextChanged += _picarat3Box_TextChanged;

            return;
        }
    }

    private string? GetText(string filePath, out Layton1PuzzleFileReference? fileReference)
    {
        fileReference = null;

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? ndsFile))
            return null;

        if (_fileManager.Parse(ndsFile, FileType.Text) is not string fileContent)
            return null;

        fileReference = new Layton1PuzzleFileReference
        {
            File = ndsFile,
            Content = fileContent
        };

        return fileContent;
    }

    private string? GetPcmText(string filePath, string entryName, out Layton1PuzzleFileReference? fileReference)
    {
        fileReference = null;

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? ndsFile))
            return null;

        if (_fileManager.Parse(ndsFile, FileType.Pcm) is not PcmFile[] pcmFiles)
            return null;

        PcmFile? pcmFile = pcmFiles.FirstOrDefault(f => f.Name == entryName);

        if (pcmFile is null)
            return null;

        if (_pcmManager.Parse(pcmFile, FileType.Text) is not string fileContent)
            return null;

        fileReference = new Layton1PuzzleFileReference
        {
            File = ndsFile,
            Content = pcmFiles,
            PcmFile = pcmFile
        };

        return fileContent;
    }
}