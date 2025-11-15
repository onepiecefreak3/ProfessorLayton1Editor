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

namespace UI.Layton1Tool.Forms;

partial class PuzzleInfoForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1PathProvider _pathProvider;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PcmFileManager _pcmManager;

    private TextLanguage _language = TextLanguage.English;

    private Layton1PuzzleId? _puzzleId;
    private Layton1NdsFile? _scriptFile;

    public PuzzleInfoForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1PathProvider pathProvider, ILayton1NdsFileManager fileManager,
        ILayton1PcmFileManager pcmManager, IFormFactory forms, ILocalizationProvider localizations)
    {
        InitializeComponent(ndsInfo, forms, localizations);

        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;
        _pathProvider = pathProvider;
        _fileManager = fileManager;
        _pcmManager = pcmManager;

        _prevButton!.Clicked += _prevButton_Clicked;
        _nextButton!.Clicked += _nextButton_Clicked;
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

        eventBroker.Subscribe<PuzzleScriptModifiedMessage>(ProcessPuzzleScriptModified);
        eventBroker.Subscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        eventBroker.Subscribe<FileAddedMessage>(ProcessFileAdded);
        eventBroker.Subscribe<SelectedPuzzleChangedMessage>(UpdatePuzzle);
        eventBroker.Subscribe<SelectedPuzzleLanguageChangedMessage>(UpdateLanguage);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<PuzzleScriptModifiedMessage>(ProcessPuzzleScriptModified);
        _eventBroker.Unsubscribe<SelectedPuzzleChangedMessage>(UpdatePuzzle);
        _eventBroker.Unsubscribe<SelectedPuzzleLanguageChangedMessage>(UpdateLanguage);
    }

    private void _nextButton_Clicked(object? sender, EventArgs e)
    {
        _layoutIndex = Math.Min(_layoutIndex + 1, 1);

        UpdateLayout();
        UpdateButtons();
    }

    private void _prevButton_Clicked(object? sender, EventArgs e)
    {
        _layoutIndex = Math.Max(_layoutIndex - 1, 0);

        UpdateLayout();
        UpdateButtons();
    }

    private void _picarat1Box_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        if (!int.TryParse(_picarat1Box.Text, out int picarat1))
            picarat1 = 0;

        if (!int.TryParse(_picarat2Box.Text, out int picarat2))
            picarat2 = 0;

        if (!int.TryParse(_picarat3Box.Text, out int picarat3))
            picarat3 = 0;

        ModifyPicaratScript(_puzzleId, picarat1, picarat2, picarat3);
    }

    private void _picarat2Box_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        if (!int.TryParse(_picarat1Box.Text, out int picarat1))
            picarat1 = 0;

        if (!int.TryParse(_picarat2Box.Text, out int picarat2))
            picarat2 = 0;

        if (!int.TryParse(_picarat3Box.Text, out int picarat3))
            picarat3 = 0;

        ModifyPicaratScript(_puzzleId, picarat1, picarat2, picarat3);
    }

    private void _picarat3Box_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        if (!int.TryParse(_picarat1Box.Text, out int picarat1))
            picarat1 = 0;

        if (!int.TryParse(_picarat2Box.Text, out int picarat2))
            picarat2 = 0;

        if (!int.TryParse(_picarat3Box.Text, out int picarat3))
            picarat3 = 0;

        ModifyPicaratScript(_puzzleId, picarat1, picarat2, picarat3);
    }

    private void _numberBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        if (!int.TryParse(_numberBox.Text, out int number))
            return;

        _puzzleId.Number = number;

        RaisePuzzleIdModified(_puzzleId);
    }

    private void _typeBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        ModifyInfoScript(_puzzleId, _typeBox.Text, _locationBox.Text);
    }

    private void _locationBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        ModifyInfoScript(_puzzleId, _typeBox.Text, _locationBox.Text);
    }

    private void _hint1Box_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null)
            return;

        ModifyText(_puzzleId, $"h_{_puzzleId.InternalId}_1.txt", _hint1Box.GetText());
    }

    private void _hint2Box_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null)
            return;

        ModifyText(_puzzleId, $"h_{_puzzleId.InternalId}_2.txt", _hint2Box.GetText());
    }

    private void _hint3Box_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null)
            return;

        ModifyText(_puzzleId, $"h_{_puzzleId.InternalId}_3.txt", _hint3Box.GetText());
    }

    private void _incorrectBox_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null)
            return;

        ModifyText(_puzzleId, $"f_{_puzzleId.InternalId}.txt", _incorrectBox.GetText());
    }

    private void _correctBox_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null)
            return;

        ModifyText(_puzzleId, $"c_{_puzzleId.InternalId}.txt", _correctBox.GetText());
    }

    private void _descriptionBox_TextChanged(object? sender, string e)
    {
        if (_puzzleId is null)
            return;

        ModifyText(_puzzleId, $"q_{_puzzleId.InternalId}.txt", _descriptionBox.GetText());
    }

    private void _titleBox_TextChanged(object? sender, EventArgs e)
    {
        if (_puzzleId is null)
            return;

        ModifyText(_puzzleId, $"t_{_puzzleId.InternalId}.txt", _titleBox.Text);
        ModifyTitleScript(_puzzleId, _titleBox.Text);
    }

    private void ModifyText(Layton1PuzzleId puzzleId, string fileName, string text)
    {
        Layton1NdsFile? textFile;
        string textPath;

        string filePath = GetBaseTextPath(puzzleId, _ndsInfo.Rom.Version);
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
                        _pcmManager.Compose(file, text, FileType.Text);
                    else
                        _ = _pcmManager.Add(files, textPath, text, FileType.Text);

                    _fileManager.Compose(textFile, files, FileType.Pcm);
                    RaiseFileContentModified(textFile, files);
                }
                else
                {
                    var files = new List<PcmFile>();
                    _pcmManager.Add(files, textPath, text, FileType.Text);

                    textFile = _fileManager.Add(_ndsInfo.Rom, filePath, files, FileType.Pcm, CompressionType.Level5Lz10);
                    RaiseFileAdded(textFile);
                }

                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }
    }

    private void ModifyTitleScript(Layton1PuzzleId puzzleId, string title)
    {
        string scriptPath = _pathProvider.GetFullDirectory("script/puzzletitle/", _ndsInfo.Rom.Version, _language) + "qtitle.gds";

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

    private void ModifyInfoScript(Layton1PuzzleId puzzleId, string type, string location)
    {
        string scriptPath = _pathProvider.GetFullDirectory("script/qinfo/", _ndsInfo.Rom.Version, _language) + "qscript.gds";

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

    private void RaiseFileContentModified(Layton1NdsFile file, object? content)
    {
        _eventBroker.Raise(new FileContentModifiedMessage(this, file, content));
    }

    private void RaiseFileAdded(Layton1NdsFile file)
    {
        _eventBroker.Raise(new FileAddedMessage(this, file));
    }

    private void RaisePuzzleIdModified(Layton1PuzzleId puzzleId)
    {
        _eventBroker.Raise(new PuzzleIdModifiedMessage(_ndsInfo.Rom, puzzleId));
    }

    private void RaisePuzzleScriptUpdated(Layton1PuzzleId puzzleId, GdsScriptFile script)
    {
        _eventBroker.Raise(new PuzzleScriptUpdatedMessage(_logicForm, _ndsInfo.Rom, puzzleId, script));
    }

    private void ProcessPuzzleScriptModified(PuzzleScriptModifiedMessage message)
    {
        if (_puzzleId is null)
            return;

        if (message.Source != _logicForm)
            return;

        if (_scriptFile is null)
        {
            string filePath = _pathProvider.GetFullDirectory($"script/qscript/q{_puzzleId.InternalId}_param.gds", _ndsInfo.Rom.Version);

            _scriptFile = _fileManager.Add(_ndsInfo.Rom, filePath, message.Script, FileType.Gds, CompressionType.None);
            RaiseFileAdded(_scriptFile);
        }
        else
        {
            _fileManager.Compose(_scriptFile, message.Script, FileType.Gds);
            RaiseFileContentModified(_scriptFile, message.Script);
        }
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (_puzzleId is null)
            return;

        if (message.Source == this)
            return;

        ProcessFileUpdate(_puzzleId, message.File);
    }

    private void ProcessFileAdded(FileAddedMessage message)
    {
        if (_puzzleId is null)
            return;

        if (message.Source == this)
            return;

        ProcessFileUpdate(_puzzleId, message.File);
    }

    private void ProcessFileUpdate(Layton1PuzzleId puzzleId, Layton1NdsFile file)
    {
        string filePath = _pathProvider.GetFullDirectory("script/qinfo/", _ndsInfo.Rom.Version, _language) + "qscript.gds";
        if (file.Path == filePath)
            UpdateInfoTexts(puzzleId);

        filePath = _pathProvider.GetFullDirectory("script/pcarot/pscript.gds", _ndsInfo.Rom.Version);
        if (file.Path == filePath)
            UpdatePicarats(puzzleId);

        filePath = _pathProvider.GetFullDirectory($"script/qscript/q{puzzleId.InternalId}_param.gds", _ndsInfo.Rom.Version);
        if (file.Path == filePath)
            UpdateLogic(puzzleId);

        filePath = GetBaseTextPath(puzzleId, _ndsInfo.Rom.Version);
        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                if (file.Path == filePath + $"c_{puzzleId.InternalId}.txt" ||
                    file.Path == filePath + $"f_{puzzleId.InternalId}.txt" ||
                    file.Path == filePath + $"h_{puzzleId.InternalId}_1.txt" ||
                    file.Path == filePath + $"h_{puzzleId.InternalId}_2.txt" ||
                    file.Path == filePath + $"h_{puzzleId.InternalId}_3.txt" ||
                    file.Path == filePath + $"q_{puzzleId.InternalId}.txt" ||
                    file.Path == filePath + $"t_{puzzleId.InternalId}.txt")
                    UpdateTexts(puzzleId);
                break;

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
            case GameVersion.JapanFriendly:
                if (file.Path == filePath)
                    UpdateTexts(puzzleId);
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }
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
        UpdateInfoTexts(puzzleId);
        UpdatePicarats(puzzleId);
        UpdateLogic(puzzleId);

        UpdateButtons();
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

        string filePath = GetBaseTextPath(puzzleId, _ndsInfo.Rom.Version);
        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                correct = GetText(filePath + $"c_{puzzleId.InternalId}.txt");
                incorrect = GetText(filePath + $"f_{puzzleId.InternalId}.txt");
                hint1 = GetText(filePath + $"h_{puzzleId.InternalId}_1.txt");
                hint2 = GetText(filePath + $"h_{puzzleId.InternalId}_2.txt");
                hint3 = GetText(filePath + $"h_{puzzleId.InternalId}_3.txt");
                description = GetText(filePath + $"q_{puzzleId.InternalId}.txt");
                title = GetText(filePath + $"t_{puzzleId.InternalId}.txt");
                break;

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
            case GameVersion.JapanFriendly:
                correct = GetPcmText(filePath, $"c_{puzzleId.InternalId}.txt");
                incorrect = GetPcmText(filePath, $"f_{puzzleId.InternalId}.txt");
                hint1 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_1.txt");
                hint2 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_2.txt");
                hint3 = GetPcmText(filePath, $"h_{puzzleId.InternalId}_3.txt");
                description = GetPcmText(filePath, $"q_{puzzleId.InternalId}.txt");
                title = GetPcmText(filePath, $"t_{puzzleId.InternalId}.txt");
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

    private string GetBaseTextPath(Layton1PuzzleId puzzleId, GameVersion version)
    {
        switch (version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                return _pathProvider.GetFullDirectory("qtext/", version, _language);

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
                string group = puzzleId.InternalId < 50 ? "000" : puzzleId.InternalId < 100 ? "050" : "100";
                return _pathProvider.GetFullDirectory("qtext/", version, _language) + $"q{group}.pcm";

            case GameVersion.JapanFriendly:
                return _pathProvider.GetFullDirectory("qtext/", version, _language) + $"q{puzzleId.InternalId / 50 * 50:000}.pcm";

            default:
                throw new InvalidOperationException($"Unknown game version {version}.");
        }
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

    private void UpdateInfoTexts(Layton1PuzzleId puzzleId)
    {
        string filePath = _pathProvider.GetFullDirectory("script/qinfo/", _ndsInfo.Rom.Version, _language) + "qscript.gds";
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

            _typeBox.Text = instruction.Arguments[2].Value as string ?? string.Empty;
            _locationBox.Text = instruction.Arguments[3].Value as string ?? string.Empty;

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

    private void UpdateButtons()
    {
        _prevButton.Enabled = _layoutIndex > 0;
        _nextButton.Enabled = _layoutIndex < 1;
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

        return _pcmManager.Parse(pcmFile, FileType.Text) as string;
    }
}