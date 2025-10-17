using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Resources;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using Logic.Domain.Level5Management.Contract.Enums;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms;

partial class AnimationForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private AnimationState? _animationState;

    public AnimationForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILocalizationProvider localizations)
    {
        InitializeComponent(localizations);

        _ndsInfo = ndsInfo;

        _fileTree!.SelectedNodeChanged += _fileTree_SelectedNodeChanged;

        eventBroker.Subscribe<SelectedAnimationsChangedMessage>(UpdateAnimations);
    }

    private void _fileTree_SelectedNodeChanged(object? sender, EventArgs e)
    {
        if (_animationState is null)
            return;

        UpdateAnimation(_animationState, _fileTree.SelectedNode.Data);
    }

    private void UpdateAnimations(SelectedAnimationsChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        var loadedImages = new ImageResource[message.AnimationSequences.Frames.Length];
        for (var i = 0; i < loadedImages.Length; i++)
            loadedImages[i] = ImageResource.FromImage(message.AnimationSequences.Frames[i].Image);

        var animationState = new AnimationState
        {
            Animations = message.AnimationSequences,
            Images = loadedImages
        };

        UpdateFileTree(animationState.Animations.Sequences);
        UpdateError(animationState.Animations);

        if (animationState.Animations.Sequences.Length > 0)
            UpdateAnimation(animationState, animationState.Animations.Sequences[0]);

        _animationState = animationState;
        _zoomableImage.Image = null;
    }

    private void UpdateFileTree(AnimationSequence[] animations)
    {
        _fileTree.Nodes.Clear();

        foreach (AnimationSequence animation in animations)
            _fileTree.Nodes.Add(new TreeNode<AnimationSequence> { Text = $"{animation.Name}", Data = animation });

        if (_fileTree.Nodes.Count > 0)
            _fileTree.SelectedNode = _fileTree.Nodes[0];

        _zoomableImage.Reset();
        _zoomableImage.Zoom(2f);
    }

    private void UpdateAnimation(AnimationState state, AnimationSequence activeAnimation)
    {
        if (state.ActiveAnimation == activeAnimation)
            return;

        state.FrameCounter = 0;
        state.StepCounter = 0;
        state.ActiveAnimation = activeAnimation;
    }

    private void UpdateError(AnimationSequences sequences)
    {
        bool hasErrors = sequences.Frames.Any(f => f.Errors is not AnimationFrameErrorType.None);
        ToggleError(hasErrors);
    }
}