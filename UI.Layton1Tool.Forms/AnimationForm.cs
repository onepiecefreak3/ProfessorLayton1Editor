using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Tree;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using Logic.Domain.Level5Management.Contract.Enums;
using UI.Layton1Tool.Components.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms;

partial class AnimationForm
{
    private readonly Layton1NdsInfo _ndsInfo;
    private readonly IEventBroker _eventBroker;

    private AnimationSequences? _animationSequences;
    private Layton1NdsFile? _selectedFile;

    public AnimationForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILocalizationProvider localizations, IComponentFactory components)
    {
        InitializeComponent(ndsInfo, localizations, components);

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;

        _fileTree!.SelectedNodeChanged += _fileTree_SelectedNodeChanged;

        eventBroker.Subscribe<SelectedFileChangedMessage>(UpdateAnimations);
        eventBroker.Subscribe<FileContentModifiedMessage>(UpdateAnimations);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedFileChangedMessage>(UpdateAnimations);
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(UpdateAnimations);
    }

    private void _fileTree_SelectedNodeChanged(object? sender, EventArgs e)
    {
        if (_animationSequences is null)
            return;

        if (_fileTree.SelectedNode is null)
            return;

        int index = Array.IndexOf(_animationSequences.Sequences, _fileTree.SelectedNode.Data);
        if (index < 0)
            return;

        UpdateAnimation(_animationSequences, index);
    }

    private void UpdateAnimations(SelectedFileChangedMessage message)
    {
        if (message.Target != this)
            return;

        UpdateAnimations(message.File, message.Content);
    }

    private void UpdateAnimations(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (message.File != _selectedFile)
            return;

        UpdateAnimations(message.File, message.Content);
    }

    private void UpdateAnimations(Layton1NdsFile file, object? content)
    {
        if (content is not AnimationSequences sequences)
            return;

        if (file.Rom != _ndsInfo.Rom)
            return;

        _animationSequences = sequences;
        _selectedFile = file;

        UpdateFileTree(_animationSequences.Sequences);
        UpdateError(_animationSequences);

        if (_animationSequences.Sequences.Length > 0)
            UpdateAnimation(_animationSequences, 0);
    }

    private void UpdateFileTree(AnimationSequence[] animations)
    {
        _fileTree.Nodes.Clear();

        foreach (AnimationSequence animation in animations)
            _fileTree.Nodes.Add(new TreeNode<AnimationSequence> { Text = $"{animation.Name}", Data = animation });

        if (_fileTree.Nodes.Count > 0)
            _fileTree.SelectedNode = _fileTree.Nodes[0];
    }

    private void UpdateAnimation(AnimationSequences sequences, int index)
    {
        _eventBroker.Raise(new SelectedAnimationChangedMessage(_ndsInfo.Rom, sequences, index));
    }

    private void UpdateError(AnimationSequences sequences)
    {
        bool hasErrors = sequences.Frames.Any(f => f.Errors is not AnimationFrameErrorType.None);
        ToggleError(hasErrors);
    }
}