using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Tree;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Forms;

partial class PcmForm
{
    private readonly Layton1NdsInfo _ndsInfo;
    private readonly IEventBroker _eventBroker;

    private Layton1NdsFile? _selectedFile;

    public PcmForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker)
    {
        InitializeComponent();

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;

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

    private void _fileTree_SelectedNodeChanged(object? sender, EventArgs e)
    {
        UpdateFileView(_fileTree.SelectedNode.Data);
    }

    private void UpdateArchive(SelectedFileChangedMessage message)
    {
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
        if (content is not PcmFile[] files)
            return;

        if (file.Rom != _ndsInfo.Rom)
            return;

        _selectedFile = file;

        UpdateFileTree(files);

        if (files.Length > 0)
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
        UpdateFileView(file);

        TreeNode<PcmFile>? node = _fileTree.Nodes.FirstOrDefault(f => f.Data.Name == file.Name);
        if (node is null)
            return;

        _fileTree.SelectedNodeChanged -= _fileTree_SelectedNodeChanged;
        _fileTree.SelectedNode = node;
        _fileTree.SelectedNodeChanged += _fileTree_SelectedNodeChanged;
    }

    private void UpdateFileTree(PcmFile[] files)
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

        _contentPanel.Content = _textEditor;
    }

    private void ResetContent()
    {
        _contentPanel.Content = null!;
    }
}