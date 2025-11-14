using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Tree;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Forms;

partial class PcmForm
{
    private readonly Layton1NdsInfo _ndsInfo;
    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PcmFileManager _pcmFileManager;

    private Layton1NdsFile? _selectedFile;

    private List<PcmFile>? _subFiles;
    private PcmFile? _selectedSubFile;

    public PcmForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1NdsFileManager fileManager, ILayton1PcmFileManager pcmFileManager)
    {
        InitializeComponent();

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;
        _fileManager = fileManager;
        _pcmFileManager = pcmFileManager;

        _textEditor!.TextChanged += _textEditor_TextChanged;
        _fileTree!.SelectedNodeChanged += _fileTree_SelectedNodeChanged;

        eventBroker.Subscribe<SelectedFileChangedMessage>(UpdateArchive);
        eventBroker.Subscribe<FileContentModifiedMessage>(UpdateArchive);
        eventBroker.Subscribe<SelectedPcmFileChangedMessage>(UpdateSelectedFile);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedFileChangedMessage>(UpdateArchive);
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(UpdateArchive);
        _eventBroker.Unsubscribe<SelectedPcmFileChangedMessage>(UpdateSelectedFile);
    }

    private void _textEditor_TextChanged(object? sender, string e)
    {
        if (_selectedFile is null || _selectedSubFile is null || _subFiles is null)
            return;

        _pcmFileManager.Compose(_selectedSubFile, e, FileType.Text);
        _fileManager.Compose(_selectedFile, _subFiles, FileType.Pcm);

        RaiseFileContentModified(_selectedFile);
    }

    private void _fileTree_SelectedNodeChanged(object? sender, EventArgs e)
    {
        if (_fileTree.SelectedNode.Data is null)
            return;

        _selectedSubFile = _fileTree.SelectedNode.Data;

        UpdateFileView(_fileTree.SelectedNode.Data);
    }

    private void RaiseFileContentModified(Layton1NdsFile file)
    {
        _eventBroker.Raise(new FileContentModifiedMessage(this, file, _subFiles));
    }

    private void UpdateArchive(SelectedFileChangedMessage message)
    {
        if (message.Target != this)
            return;

        UpdateArchive(message.File, message.Content);
    }

    private void UpdateArchive(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (message.File != _selectedFile)
            return;

        UpdateArchive(message.File, message.Content);
    }

    private void UpdateArchive(Layton1NdsFile file, object? content)
    {
        if (content is not List<PcmFile> files)
            return;

        if (file.Rom != _ndsInfo.Rom)
            return;

        _selectedFile = file;
        _subFiles = files;
        _selectedSubFile = null;

        UpdateFileTree(files);

        if (files.Count > 0)
            UpdateSelectedFile(files[0]);
    }

    private void UpdateSelectedFile(SelectedPcmFileChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        UpdateSelectedFile(message.File);
    }

    private void UpdateSelectedFile(PcmFile file)
    {
        _selectedSubFile = file;

        UpdateFileView(file);

        TreeNode<PcmFile>? node = _fileTree.Nodes.FirstOrDefault(f => f.Data.Name == file.Name);
        if (node is null)
            return;

        _fileTree.SelectedNodeChanged -= _fileTree_SelectedNodeChanged;
        _fileTree.SelectedNode = node;
        _fileTree.SelectedNodeChanged += _fileTree_SelectedNodeChanged;
    }

    private void UpdateFileTree(List<PcmFile> files)
    {
        _fileTree.Nodes.Clear();

        foreach (PcmFile file in files)
            _fileTree.Nodes.Add(new TreeNode<PcmFile> { Text = file.Name, Data = file });
    }

    private void UpdateFileView(PcmFile file)
    {
        string extension = Path.GetExtension(file.Name);
        switch (extension)
        {
            case ".txt":
                UpdateTxt(file);
                break;

            default:
                ResetContent();
                break;
        }
    }

    private void UpdateTxt(PcmFile file)
    {
        file.Data.Position = 0;

        var streamReader = new StreamReader(file.Data, leaveOpen: true);
        _textEditor.SetText(streamReader.ReadToEnd());
        _textEditor.IsReadOnly = false;

        _contentPanel.Content = _textEditor;
    }

    private void ResetContent()
    {
        _contentPanel.Content = null!;
    }
}