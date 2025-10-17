using System.Text.RegularExpressions;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Messages;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Localization;
using ImGui.Forms.Modals;
using ImGui.Forms.Modals.IO;
using ImGui.Forms.Modals.IO.Windows;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using Logic.Domain.Level5Management.Contract.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.DataClasses;
using UI.Layton1Tool.Forms.InternalContract;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Messages.Enums;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms;

partial class NdsForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly IFileHistory _fileHistory;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILocalizationProvider _localizations;
    private readonly ISettingsProvider _settings;
    private readonly IColorProvider _colors;

    private readonly Dictionary<Layton1NdsFile, TreeNode<Layton1NdsFile?>> _fileLookup = [];
    private readonly HashSet<string> _expandedNodes = [];

    private readonly HashSet<string> _errorPaths = [];
    private readonly HashSet<string> _changedPaths = [];

    private Layton1NdsFile? _selectedFile;

    public NdsForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, IFormFactory formFactory, IFileHistory fileHistory,
        IImageProvider images, ILayton1NdsFileManager fileManager, ILocalizationProvider localizations, ISettingsProvider settings,
        IColorProvider colors)
    {
        InitializeComponent(ndsInfo, formFactory, localizations, images);

        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;
        _fileHistory = fileHistory;
        _fileManager = fileManager;
        _localizations = localizations;
        _settings = settings;
        _colors = colors;

        _saveButton!.Clicked += _saveButton_Clicked;
        _saveAsButton!.Clicked += _saveAsButton_Clicked;
        _prevButton!.Clicked += _prevButton_Clicked;
        _nextButton!.Clicked += _nextButton_Clicked;
        _extractButton!.Clicked += _extractButton_Clicked;
        _searchBox!.TextChanged += _searchBox_TextChanged;
        _searchClearButton!.Clicked += _searchClearButton_Clicked;
        _fileTree!.SelectedNodeChanged += _fileTree_SelectedNodeChanged;
        _fileTree.NodeExpanded += _fileTree_NodeExpanded;
        _fileTree.NodeCollapsed += _fileTree_NodeCollapsed;
        _fileContextMenu!.Show += _fileContextMenu_Show;

        eventBroker.Subscribe<Layton1NdsFileParsedMessage>(ProcessParsedFile);
        eventBroker.Subscribe<NdsFileSavedMessage>(ProcessNdsFileSaved);
        eventBroker.Subscribe<GdsModifiedMessage>(ProcessModifiedGds);
        eventBroker.Subscribe<SelectedNdsFileChangedMessage>(ProcessSelectedFileChanged);

        UpdateTreeView(ndsInfo);
    }

    private async void _saveAsButton_Clicked(object? sender, EventArgs e)
    {
        await Save(true);
    }

    private async void _saveButton_Clicked(object? sender, EventArgs e)
    {
        await Save(false);
    }

    private void _fileTree_NodeCollapsed(object? sender, NodeEventArgs<Layton1NdsFile?> e)
    {
        if (e.Node.IsRoot)
            return;

        if (e.Node.Data is not null)
            return;

        string nodePath = GetNodePath(e.Node);
        _expandedNodes.Remove(nodePath);
    }

    private void _fileTree_NodeExpanded(object? sender, NodeEventArgs<Layton1NdsFile?> e)
    {
        if (e.Node.IsRoot)
            return;

        if (e.Node.Data is not null)
            return;

        string nodePath = GetNodePath(e.Node);
        _expandedNodes.Add(nodePath);
    }

    private void _nextButton_Clicked(object? sender, EventArgs e)
    {
        Layton1NdsFile? file = _fileHistory.Forward();
        if (file is null)
            return;

        UpdateSelectedNode(file);
        if (UpdateFileView(file))
            SetInfoStatus(string.Empty);

        UpdateHistoryButtons();
    }

    private void _prevButton_Clicked(object? sender, EventArgs e)
    {
        Layton1NdsFile? file = _fileHistory.Back();
        if (file is null)
            return;

        UpdateSelectedNode(file);
        if (UpdateFileView(file))
            SetInfoStatus(string.Empty);

        UpdateHistoryButtons();
    }

    private void _fileContextMenu_Show(object? sender, EventArgs e)
    {
        if (_fileTree.SelectedNode is null)
            return;

        _extractButton.Enabled = _fileTree.SelectedNode.Parent is not null;
    }

    private async void _extractButton_Clicked(object? sender, EventArgs e)
    {
        if (_fileTree.SelectedNode is null)
            return;

        if (_fileTree.SelectedNode.Data is null)
            await ExtractDirectory(_fileTree.SelectedNode);
        else
            await ExtractFile(_fileTree.SelectedNode.Data);
    }

    private void _fileTree_SelectedNodeChanged(object? sender, EventArgs e)
    {
        Layton1NdsFile? file = _fileTree.SelectedNode.Data;
        if (file is null)
            return;

        ChangeSelectedFile(file);
    }

    private void _searchClearButton_Clicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_searchBox.Text))
            return;

        _searchBox.Text = string.Empty;
    }

    private void _searchBox_TextChanged(object? sender, EventArgs e)
    {
        UpdateTreeView(_ndsInfo);
    }

    private void ChangeSelectedFile(Layton1NdsFile file)
    {
        if (_selectedFile == file)
            return;

        bool hasUpdatedFileView = UpdateFileView(file);
        if (!hasUpdatedFileView)
            return;

        SetInfoStatus(string.Empty);

        _fileHistory.Add(file);

        UpdateHistoryButtons();
    }

    private void ProcessParsedFile(Layton1NdsFileParsedMessage message)
    {
        if (message.Type is not FileType.Anim and not FileType.Anim2)
            return;

        var animations = (AnimationSequences)message.Data;
        bool hasErrors = animations.Frames.Any(f => f.Errors is not AnimationFrameErrorType.None);

        if (!hasErrors)
            return;

        if (!_fileLookup.TryGetValue(message.File, out TreeNode<Layton1NdsFile?>? node))
            return;

        do
        {
            string nodePath = GetNodePath(node);
            if (!_changedPaths.Contains(nodePath))
                node.TextColor = _colors.Error;

            _errorPaths.Add(nodePath);

            node = node.Parent;
        } while (node is not null && !node.IsRoot);
    }

    private void ProcessNdsFileSaved(NdsFileSavedMessage message)
    {
        if (message.OriginalRom != _ndsInfo.Rom)
            return;

        _ndsInfo.Path = message.RomPath;
        _ndsInfo.Rom = message.Rom;

        if (_selectedFile is not null)
            _selectedFile = message.Rom.Files.FirstOrDefault(f => f.Path == _selectedFile.Path);

        _changedPaths.Clear();
        _errorPaths.Clear();

        _fileHistory.Clear();
        if (_selectedFile is not null)
            _fileHistory.Add(_selectedFile);

        UpdateTreeView(_ndsInfo);

        if (_selectedFile is not null)
            _ = UpdateFileView(_selectedFile);

        UpdateSaveButtons();
        UpdateHistoryButtons();
    }

    private void ProcessModifiedGds(GdsModifiedMessage message)
    {
        if (_selectedFile is null)
            return;

        if (message.Source != _gdsForm)
            return;

        try
        {
            _fileManager.Compose(_selectedFile, message.Script);
        }
        catch (Exception)
        {
            return;
        }

        ToggleChangedFile(_selectedFile, true);
        UpdateTreeView(_ndsInfo);

        UpdateSaveButtons();

        RaiseNdsFileModified();
    }

    private void ProcessSelectedFileChanged(SelectedNdsFileChangedMessage message)
    {
        if (_ndsInfo.Rom != message.Rom)
            return;

        ChangeSelectedFile(message.File);
    }

    private void ToggleChangedFile(Layton1NdsFile file, bool toggle)
    {
        string[] parts = file.Path.Split('/');

        var totalPath = string.Empty;
        foreach (string part in parts)
        {
            totalPath += string.IsNullOrEmpty(totalPath) ? part : $"/{part}";

            if (toggle)
                _changedPaths.Add(totalPath);
            else
                _changedPaths.Remove(totalPath);
        }
    }

    private async Task Save(bool saveAs)
    {
        if (_changedPaths.Count <= 0)
            return;

        await _eventBroker.RaiseAsync(new NdsFileSaveRequestedMessage(_ndsInfo.Path, saveAs));
    }

    private void UpdateHistoryButtons()
    {
        _nextButton.Enabled = _fileHistory.Index + 1 < _fileHistory.Count;
        _prevButton.Enabled = _fileHistory.Index > 0;
    }

    private void UpdateSaveButtons()
    {
        _saveButton.Enabled = _saveAsButton.Enabled = _changedPaths.Count > 0;
    }

    private void UpdateSelectedNode(Layton1NdsFile file)
    {
        _fileLookup.TryGetValue(file, out TreeNode<Layton1NdsFile?>? node);

        _fileTree.SelectedNodeChanged -= _fileTree_SelectedNodeChanged;
        _fileTree.SelectedNode = node!;
        _fileTree.SelectedNodeChanged += _fileTree_SelectedNodeChanged;
    }

    private bool UpdateFileView(Layton1NdsFile file)
    {
        FileType fileType;
        object? data;

        try
        {
            data = _fileManager.Parse(file, out fileType);
        }
        catch (Exception e)
        {
            SetErrorStatus(_localizations.StatusFileOpenError, e);

            _contentPanel.Content = null!;
            _selectedFile = null;

            return false;
        }

        switch (fileType)
        {
            case FileType.Bgx:
                RaiseBgxUpdated((Image<Rgba32>)data!);
                _contentPanel.Content = _bgxForm;
                break;

            case FileType.Gds:
                RaiseGdsUpdated((CodeUnitSyntax)data!);
                _contentPanel.Content = _gdsForm;
                break;

            case FileType.Text:
                RaiseTextUpdated((string)data!);
                _contentPanel.Content = _textForm;
                break;

            case FileType.Pcm:
                RaisePcmUpdated((PcmFile[])data!);
                _contentPanel.Content = _pcmForm;
                break;

            case FileType.Anim:
            case FileType.Anim2:
            case FileType.Anim3:
                RaiseAnimationsUpdated((AnimationSequences)data!);
                _contentPanel.Content = _animForm;
                break;

            default:
                _contentPanel.Content = null!;
                break;
        }

        _selectedFile = file;

        return true;
    }

    private void RaiseBgxUpdated(Image<Rgba32> image)
    {
        _eventBroker.Raise(new SelectedBgxChangedMessage(_ndsInfo.Rom, image));
    }

    private void RaiseGdsUpdated(CodeUnitSyntax script)
    {
        _eventBroker.Raise(new SelectedGdsChangedMessage(_ndsInfo.Rom, script));
    }

    private void RaiseTextUpdated(string text)
    {
        _eventBroker.Raise(new SelectedTextChangedMessage(_ndsInfo.Rom, text));
    }

    private void RaisePcmUpdated(PcmFile[] files)
    {
        _eventBroker.Raise(new SelectedPcmChangedMessage(_ndsInfo.Rom, files));
    }

    private void RaiseAnimationsUpdated(AnimationSequences animations)
    {
        _eventBroker.Raise(new SelectedAnimationsChangedMessage(_ndsInfo.Rom, animations));
    }

    private void UpdateTreeView(Layton1NdsInfo ndsInfo)
    {
        Regex searchTerm = GetSearchTerm();
        Layton1NdsDirectory directoryTree = CreateDirectoryTree(ndsInfo, searchTerm);

        _fileTree.Nodes.Clear();
        _fileLookup.Clear();

        var root = new TreeNode<Layton1NdsFile?> { Text = directoryTree.Name, IsExpanded = true };
        _fileTree.Nodes.Add(root);

        _fileTree.NodeExpanded -= _fileTree_NodeExpanded;
        _fileTree.NodeCollapsed -= _fileTree_NodeCollapsed;
        _fileTree.SelectedNodeChanged -= _fileTree_SelectedNodeChanged;

        UpdateTreeNode(root, directoryTree);

        _fileTree.NodeExpanded += _fileTree_NodeExpanded;
        _fileTree.NodeCollapsed += _fileTree_NodeCollapsed;
        _fileTree.SelectedNodeChanged += _fileTree_SelectedNodeChanged;
    }

    private Regex GetSearchTerm()
    {
        if (string.IsNullOrEmpty(_searchBox.Text))
            return new(".*");

        string escapedSearchTerm = _searchBox.Text.Replace(".", "\\.").Replace("*", ".*");
        return new Regex(escapedSearchTerm);
    }

    private void UpdateTreeNode(TreeNode<Layton1NdsFile?> node, Layton1NdsDirectory directory)
    {
        foreach (Layton1NdsDirectory subDirectory in directory.Directories.OrderBy(x => x.Name))
        {
            var directoryNode = new TreeNode<Layton1NdsFile?> { Text = subDirectory.Name };
            node.Nodes.Add(directoryNode);

            string nodePath = GetNodePath(directoryNode);
            directoryNode.IsExpanded = _expandedNodes.Contains(nodePath);
            directoryNode.TextColor = _changedPaths.Contains(nodePath)
                ? _colors.Changed
                : _errorPaths.Contains(nodePath)
                    ? _colors.Error
                    : _colors.Default;

            UpdateTreeNode(directoryNode, subDirectory);
        }

        foreach (Layton1NdsFile file in directory.Files.OrderBy(x => Path.GetFileNameWithoutExtension(x.Path)))
        {
            var fileNode = new TreeNode<Layton1NdsFile?> { Text = Path.GetFileName(file.Path), Data = file };
            node.Nodes.Add(fileNode);

            string nodePath = GetNodePath(fileNode);
            fileNode.TextColor = _changedPaths.Contains(nodePath)
                ? _colors.Changed
                : _errorPaths.Contains(nodePath)
                    ? _colors.Error
                    : _colors.Default;

            if (file == _selectedFile)
                _fileTree.SelectedNode = fileNode;

            _fileLookup[file] = fileNode;
        }
    }

    private static Layton1NdsDirectory CreateDirectoryTree(Layton1NdsInfo ndsInfo, Regex searchTerm)
    {
        var root = new Layton1NdsDirectory { Name = Path.GetFileName(ndsInfo.Path) };

        foreach (Layton1NdsFile file in ndsInfo.Rom.Files)
        {
            if (!searchTerm.IsMatch(file.Path))
                continue;

            Layton1NdsDirectory parent = root;

            string? directory = Path.GetDirectoryName(file.Path);
            if (string.IsNullOrEmpty(directory))
            {
                parent.Files.Add(file);
                continue;
            }

            foreach (string directoryPart in directory.Split(Path.DirectorySeparatorChar))
            {
                Layton1NdsDirectory? entry = parent.Directories.FirstOrDefault(x => x.Name == directoryPart);
                if (entry == null)
                {
                    entry = new Layton1NdsDirectory { Name = directoryPart };
                    parent.Directories.Add(entry);
                }

                parent = entry;
            }

            parent.Files.Add(file);
        }

        return root;
    }

    private string GetNodePath(TreeNode<Layton1NdsFile?> node)
    {
        if (node.Data is not null)
            return node.Data.Path;

        if (node.Parent is null || node.Parent.IsRoot)
            return node.Text;

        return $"{GetNodePath(node.Parent)}/{node.Text}";
    }

    private async Task ExtractDirectory(TreeNode<Layton1NdsFile?> node)
    {
        string? dirPath = await SelectSaveDirectory();
        if (dirPath is null)
        {
            SetErrorStatus(_localizations.StatusDirectorySelectError);
            return;
        }

        await ExtractDirectory(node, dirPath);

        SetInfoStatus(string.Empty);
    }

    private async Task ExtractDirectory(TreeNode<Layton1NdsFile?> node, string dirPath)
    {
        if (node.Data is not null)
        {
            await ExtractFile(node.Data, Path.Combine(dirPath, Path.GetFileName(node.Data.Path)));
            return;
        }

        foreach (TreeNode<Layton1NdsFile?> directoryNode in node.Nodes)
            await ExtractDirectory(directoryNode, Path.Combine(dirPath, directoryNode.Text));
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

    private void RaiseNdsFileModified()
    {
        _eventBroker.RaiseAsync(new NdsFileModifiedMessage(_ndsInfo.Path));
    }
}