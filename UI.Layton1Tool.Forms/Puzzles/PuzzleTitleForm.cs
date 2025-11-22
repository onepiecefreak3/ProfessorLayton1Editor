using System.Text;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms.Puzzles;

internal partial class PuzzleTitleForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1PathProvider _pathProvider;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PcmFileManager _pcmManager;

    private Layton1PuzzleId? _puzzleId;
    private TextLanguage? _language;

    public PuzzleTitleForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1PathProvider pathProvider, ILayton1NdsFileManager fileManager,
        ILayton1PcmFileManager pcmManager, IFormFactory forms, ILocalizationProvider localizations)
    {
        InitializeComponent(ndsInfo, forms, localizations);

        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;
        _pathProvider = pathProvider;
        _fileManager = fileManager;
        _pcmManager = pcmManager;

        _numberBox!.TextChanged += _numberBox_TextChanged;
        _titleBox!.TextChanged += _titleBox_TextChanged;
        _locationBox!.TextChanged += _locationBox_TextChanged;
        _typeBox!.TextChanged += _typeBox_TextChanged;
        _picarat1Box!.TextChanged += _picarat1Box_TextChanged;
        _picarat2Box!.TextChanged += _picarat2Box_TextChanged;
        _picarat3Box!.TextChanged += _picarat3Box_TextChanged;

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

    private void RaisePuzzleIdModified(Layton1PuzzleId puzzleId)
    {
        _eventBroker.Raise(new SelectedPuzzleIdModifiedMessage(_ndsInfo.Rom, puzzleId));
    }

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

    private void RaiseSelectedPuzzleTitleTextModified(Layton1PuzzleId puzzleId, string title)
    {
        _eventBroker.Raise(new SelectedPuzzleTitleTextModifiedMessage(_ndsInfo.Rom, puzzleId, title));
    }

    private void RaiseSelectedPuzzleIndexTextsModified(Layton1PuzzleId puzzleId, string title, string type, string location)
    {
        _eventBroker.Raise(new SelectedPuzzleIndexTextsModifiedMessage(_ndsInfo.Rom, puzzleId, title, type, location));
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

    private void _numberBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null || _language is null)
            return;

        if (!int.TryParse(_numberBox.Text, out int number))
            return;

        _puzzleId.Number = number;

        RaiseSelectedPuzzleTitleTextModified(_puzzleId, _titleBox.Text);
        RaiseSelectedPuzzleIndexTextsModified(_puzzleId, _titleBox.Text, _typeBox.Text, _locationBox.Text);

        RaisePuzzleIdModified(_puzzleId);
    }

    private void _titleBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null || _language is null)
            return;

        ModifyText(_puzzleId, _language.Value, $"t_{_puzzleId.InternalId}.txt", _titleBox.Text);
        ModifyTitleScript(_puzzleId, _language.Value, _titleBox.Text);

        RaiseSelectedPuzzleTitleTextModified(_puzzleId, _titleBox.Text);
        RaiseSelectedPuzzleIndexTextsModified(_puzzleId, _titleBox.Text, _typeBox.Text, _locationBox.Text);


        RaiseSelectedPuzzleModified(_puzzleId);
    }

    private void _locationBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null || _language is null)
            return;

        ModifyInfoScript(_puzzleId, _language.Value, _typeBox.Text, _locationBox.Text);

        RaiseSelectedPuzzleIndexTextsModified(_puzzleId, _titleBox.Text, _typeBox.Text, _locationBox.Text);

        RaiseSelectedPuzzleModified(_puzzleId);
    }

    private void _typeBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null || _language is null)
            return;

        ModifyInfoScript(_puzzleId, _language.Value, _typeBox.Text, _locationBox.Text);

        RaiseSelectedPuzzleIndexTextsModified(_puzzleId, _titleBox.Text, _typeBox.Text, _locationBox.Text);

        RaiseSelectedPuzzleModified(_puzzleId);
    }

    private void _picarat1Box_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null || _language is null)
            return;

        if (!int.TryParse(_picarat1Box.Text, out int picarat1))
            picarat1 = 0;

        if (!int.TryParse(_picarat2Box.Text, out int picarat2))
            picarat2 = 0;

        if (!int.TryParse(_picarat3Box.Text, out int picarat3))
            picarat3 = 0;

        ModifyPicaratScript(_puzzleId, picarat1, picarat2, picarat3);

        RaiseSelectedPuzzleTitleTextModified(_puzzleId, _titleBox.Text);

        RaiseSelectedPuzzleModified(_puzzleId);
    }

    private void _picarat2Box_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null || _language is null)
            return;

        if (!int.TryParse(_picarat1Box.Text, out int picarat1))
            picarat1 = 0;

        if (!int.TryParse(_picarat2Box.Text, out int picarat2))
            picarat2 = 0;

        if (!int.TryParse(_picarat3Box.Text, out int picarat3))
            picarat3 = 0;

        ModifyPicaratScript(_puzzleId, picarat1, picarat2, picarat3);

        RaiseSelectedPuzzleTitleTextModified(_puzzleId, _titleBox.Text);

        RaiseSelectedPuzzleModified(_puzzleId);
    }

    private void _picarat3Box_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null || _language is null)
            return;

        if (!int.TryParse(_picarat1Box.Text, out int picarat1))
            picarat1 = 0;

        if (!int.TryParse(_picarat2Box.Text, out int picarat2))
            picarat2 = 0;

        if (!int.TryParse(_picarat3Box.Text, out int picarat3))
            picarat3 = 0;

        ModifyPicaratScript(_puzzleId, picarat1, picarat2, picarat3);

        RaiseSelectedPuzzleTitleTextModified(_puzzleId, _titleBox.Text);

        RaiseSelectedPuzzleModified(_puzzleId);
    }

    #endregion

    #region Updates

    private void UpdatePuzzle(Layton1PuzzleId puzzleId, TextLanguage language)
    {
        _puzzleId = puzzleId;

        _internalIdBox.Text = $"{puzzleId.InternalId}";

        UpdateNumber(puzzleId);
        UpdateTexts(puzzleId, language);
        UpdateInfoTexts(puzzleId, language);
        UpdatePicarats(puzzleId);

        RaiseSelectedPuzzleTitleTextModified(puzzleId, _titleBox.Text);
        RaiseSelectedPuzzleIndexTextsModified(puzzleId, _titleBox.Text, _typeBox.Text, _locationBox.Text);
    }

    private void UpdateNumber(Layton1PuzzleId puzzleId)
    {
        _numberBox.TextChanged -= _numberBox_TextChanged;

        _numberBox.IsReadOnly = puzzleId.IsWifi;
        _numberBox.Text = puzzleId.IsWifi ? $"W{puzzleId.Number:00}" : $"{puzzleId.Number}";

        _numberBox.TextChanged += _numberBox_TextChanged;
    }

    private void UpdateChangedTexts(Layton1PuzzleId puzzleId, Layton1NdsFile file, TextLanguage language)
    {
        string filePath = _pathProvider.GetFullDirectory("script/qinfo/", _ndsInfo.Rom.Version, language) + "qscript.gds";
        if (file.Path == filePath)
        {
            UpdateInfoTexts(puzzleId, language);
            RaiseSelectedPuzzleIndexTextsModified(puzzleId, _titleBox.Text, _typeBox.Text, _locationBox.Text);
            return;
        }

        filePath = _pathProvider.GetFullDirectory("script/pcarot/pscript.gds", _ndsInfo.Rom.Version);
        if (file.Path == filePath)
        {
            UpdatePicarats(puzzleId);
            RaiseSelectedPuzzleTitleTextModified(puzzleId, _titleBox.Text);
            return;
        }

        filePath = GetBaseTextPath(puzzleId, _ndsInfo.Rom.Version, language);
        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                if (file.Path == filePath + $"t_{puzzleId.InternalId}.txt")
                {
                    UpdateTexts(puzzleId, language);
                    RaiseSelectedPuzzleTitleTextModified(puzzleId, _titleBox.Text);
                    RaiseSelectedPuzzleIndexTextsModified(puzzleId, _titleBox.Text, _typeBox.Text, _locationBox.Text);
                }
                break;

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
            case GameVersion.JapanFriendly:
                if (file.Path == filePath)
                {
                    UpdateTexts(puzzleId, language);
                    RaiseSelectedPuzzleTitleTextModified(puzzleId, _titleBox.Text);
                    RaiseSelectedPuzzleIndexTextsModified(puzzleId, _titleBox.Text, _typeBox.Text, _locationBox.Text);
                }
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }
    }

    private void UpdateInfoTexts(Layton1PuzzleId puzzleId, TextLanguage language)
    {
        string filePath = _pathProvider.GetFullDirectory("script/qinfo/", _ndsInfo.Rom.Version, language) + "qscript.gds";
        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? infoScriptFile))
            return;

        if (_fileManager.Parse(infoScriptFile, FileType.Gds) is not GdsScriptFile infoScript)
            return;

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

            string type = instruction.Arguments[2].Value as string ?? string.Empty;
            string location = instruction.Arguments[3].Value as string ?? string.Empty;

            if (_ndsInfo.Rom.Version is GameVersion.Korea)
            {
                _typeBox.Text = Encoding.BigEndianUnicode.GetString(Convert.FromHexString(type));
                _locationBox.Text = Encoding.BigEndianUnicode.GetString(Convert.FromHexString(location));
            }
            else
            {
                _typeBox.Text = type;
                _locationBox.Text = location;
            }

            _locationBox.IsReadOnly = false;
            _typeBox.IsReadOnly = false;

            _locationBox.TextChanged += _locationBox_TextChanged;
            _typeBox.TextChanged += _typeBox_TextChanged;

            return;
        }
    }

    private void UpdatePicarats(Layton1PuzzleId puzzleId)
    {
        string filePath = _pathProvider.GetFullDirectory("script/pcarot/pscript.gds", _ndsInfo.Rom.Version);
        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? picaratFile))
            return;

        if (_fileManager.Parse(picaratFile, FileType.Gds) is not GdsScriptFile picaratScript)
            return;

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

    private void UpdateTexts(Layton1PuzzleId puzzleId, TextLanguage language)
    {
        string? title;

        string filePath = GetBaseTextPath(puzzleId, _ndsInfo.Rom.Version, language);
        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                title = GetText(filePath + $"t_{puzzleId.InternalId}.txt");
                break;

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
            case GameVersion.JapanFriendly:
                title = GetPcmText(filePath, $"t_{puzzleId.InternalId}.txt");
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        UpdateTitleText(title);
    }

    private void UpdateTitleText(string? title)
    {
        _titleBox.TextChanged -= _titleBox_TextChanged;

        _titleBox.IsReadOnly = false;
        _titleBox.Text = title ?? string.Empty;

        _titleBox.TextChanged += _titleBox_TextChanged;
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

    private void ModifyTitleScript(Layton1PuzzleId puzzleId, TextLanguage language, string title)
    {
        string scriptPath = _pathProvider.GetFullDirectory("script/puzzletitle/", _ndsInfo.Rom.Version, language) + "qtitle.gds";

        if (_fileManager.TryGet(_ndsInfo.Rom, scriptPath, out Layton1NdsFile? scriptFile))
        {
            var script = (GdsScriptFile?)_fileManager.Parse(scriptFile, FileType.Gds);

            if (script is null)
                return;

            GdsScriptInstruction? foundInstruction = null;
            foreach (GdsScriptInstruction instruction in script.Instructions)
            {
                if (instruction.Type is not 0 || instruction.Arguments.Length is not 3 || instruction.Arguments[0].Value is not 186)
                    continue;

                if (instruction.Arguments[1].Value is not int internalId || puzzleId.InternalId != internalId)
                    continue;

                foundInstruction = instruction;
                break;
            }

            if (foundInstruction is null)
            {
                script.Instructions.Insert(script.Instructions.Count - 1, CreateTitleInstruction(puzzleId, title));
            }
            else
            {
                foundInstruction.Arguments[2].Value = _titleBox.Text;
            }

            _fileManager.Compose(scriptFile, script, FileType.Gds);
            RaiseFileContentModified(scriptFile, script);
        }
        else
        {
            var script = new GdsScriptFile
            {
                Instructions =
                [
                    CreateTitleInstruction(puzzleId, title),
                    new GdsScriptInstruction { Type = 12 }
                ]
            };

            scriptFile = _fileManager.Add(_ndsInfo.Rom, scriptPath, script, FileType.Gds, CompressionType.None);
            RaiseFileAdded(scriptFile);
        }
    }

    private void ModifyInfoScript(Layton1PuzzleId puzzleId, TextLanguage language, string type, string location)
    {
        string scriptPath = _pathProvider.GetFullDirectory("script/qinfo/", _ndsInfo.Rom.Version, language) + "qscript.gds";

        if (_fileManager.TryGet(_ndsInfo.Rom, scriptPath, out Layton1NdsFile? scriptFile))
        {
            var script = (GdsScriptFile?)_fileManager.Parse(scriptFile, FileType.Gds);

            if (script is null)
                return;

            GdsScriptInstruction? foundInstruction = null;
            foreach (GdsScriptInstruction instruction in script.Instructions)
            {
                if (instruction.Type is not 0 || instruction.Arguments.Length is not 4 || instruction.Arguments[0].Value is not 220)
                    continue;

                if (instruction.Arguments[1].Value is not int internalId || puzzleId.InternalId != internalId)
                    continue;

                foundInstruction = instruction;
                break;
            }

            if (_ndsInfo.Rom.Version is GameVersion.Korea)
            {
                type = Convert.ToHexString(Encoding.BigEndianUnicode.GetBytes(type)).ToUpper();
                location = Convert.ToHexString(Encoding.BigEndianUnicode.GetBytes(location)).ToUpper();
            }

            if (foundInstruction is null)
            {
                script.Instructions.Insert(script.Instructions.Count - 1, CreateInfoInstruction(puzzleId, type, location));
            }
            else
            {
                foundInstruction.Arguments[2].Value = type;
                foundInstruction.Arguments[3].Value = location;
            }

            _fileManager.Compose(scriptFile, script, FileType.Gds);
            RaiseFileContentModified(scriptFile, script);
        }
        else
        {
            var script = new GdsScriptFile
            {
                Instructions =
                [
                    CreateInfoInstruction(puzzleId, type, location),
                    new GdsScriptInstruction { Type = 12 }
                ]
            };

            scriptFile = _fileManager.Add(_ndsInfo.Rom, scriptPath, script, FileType.Gds, CompressionType.None);
            RaiseFileAdded(scriptFile);
        }
    }

    private void ModifyPicaratScript(Layton1PuzzleId puzzleId, int picarat1, int picarat2, int picarat3)
    {
        string scriptPath = _pathProvider.GetFullDirectory("script/pcarot/pscript.gds", _ndsInfo.Rom.Version);

        if (_fileManager.TryGet(_ndsInfo.Rom, scriptPath, out Layton1NdsFile? scriptFile))
        {
            var script = (GdsScriptFile?)_fileManager.Parse(scriptFile, FileType.Gds);

            if (script is null)
                return;

            GdsScriptInstruction? foundInstruction = null;
            foreach (GdsScriptInstruction instruction in script.Instructions)
            {
                if (instruction.Type is not 0 || instruction.Arguments.Length is not 5 || instruction.Arguments[0].Value is not 195)
                    continue;

                if (instruction.Arguments[1].Value is not int internalId || puzzleId.InternalId != internalId)
                    continue;

                foundInstruction = instruction;
                break;
            }

            if (foundInstruction is null)
            {
                script.Instructions.Insert(script.Instructions.Count - 1, CreatePicaratInstruction(puzzleId, picarat1, picarat2, picarat3));
            }
            else
            {
                foundInstruction.Arguments[2].Value = picarat1;
                foundInstruction.Arguments[3].Value = picarat2;
                foundInstruction.Arguments[4].Value = picarat3;
            }

            _fileManager.Compose(scriptFile, script, FileType.Gds);
            RaiseFileContentModified(scriptFile, script);
        }
        else
        {
            var script = new GdsScriptFile
            {
                Instructions =
                [
                    CreatePicaratInstruction(puzzleId, picarat1, picarat2, picarat3),
                    new GdsScriptInstruction { Type = 12 }
                ]
            };

            scriptFile = _fileManager.Add(_ndsInfo.Rom, scriptPath, script, FileType.Gds, CompressionType.None);
            RaiseFileAdded(scriptFile);
        }
    }

    #endregion

    #region Script Creation

    private static GdsScriptInstruction CreateTitleInstruction(Layton1PuzzleId puzzleId, string title)
    {
        return new GdsScriptInstruction
        {
            Type = 0,
            Arguments =
            [
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.Int,
                    Value = 186
                },
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.Int,
                    Value = puzzleId.InternalId
                },
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.String,
                    Value = title
                }
            ]
        };
    }

    private static GdsScriptInstruction CreateInfoInstruction(Layton1PuzzleId puzzleId, string type, string location)
    {
        return new GdsScriptInstruction
        {
            Type = 0,
            Arguments =
            [
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.Int,
                    Value = 220
                },
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.Int,
                    Value = puzzleId.InternalId
                },
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.String,
                    Value = type
                },
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.String,
                    Value = location
                }
            ]
        };
    }

    private static GdsScriptInstruction CreatePicaratInstruction(Layton1PuzzleId puzzleId, int picarat1, int picarat2, int picarat3)
    {
        return new GdsScriptInstruction
        {
            Type = 0,
            Arguments =
            [
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.Int,
                    Value = 195
                },
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.Int,
                    Value = puzzleId.InternalId
                },
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.Int,
                    Value = picarat1
                },
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.Int,
                    Value = picarat2
                },
                new GdsScriptArgument
                {
                    Type = GdsScriptArgumentType.Int,
                    Value = picarat3
                }
            ]
        };
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