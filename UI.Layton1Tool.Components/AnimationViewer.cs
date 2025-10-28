using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using UI.Layton1Tool.Components.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Components;

partial class AnimationViewer
{
    private readonly Layton1NdsInfo _ndsInfo;
    private readonly IEventBroker _eventBroker;

    private AnimationSequence? _activeSequence;

    public AnimationViewer(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, IComponentFactory components)
    {
        InitializeComponent(ndsInfo, components);

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;

        eventBroker.Subscribe<SelectedAnimationChangedMessage>(ProcessSelectedAnimationChanged);
        eventBroker.Subscribe<AnimationFrameChangedMessage>(ProcessAnimationFrameChanged);
    }

    public override void Destroy()
    {
        _player.Destroy();

        _eventBroker.Unsubscribe<SelectedAnimationChangedMessage>(ProcessSelectedAnimationChanged);
        _eventBroker.Unsubscribe<AnimationFrameChangedMessage>(ProcessAnimationFrameChanged);
    }

    private void ProcessSelectedAnimationChanged(SelectedAnimationChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        if (message.SequenceId < 0 && message.SequenceId >= message.AnimationSequences.Sequences.Length)
            return;

        AnimationSequence activeAnimation = message.AnimationSequences.Sequences[message.SequenceId];

        if (_activeSequence == activeAnimation)
            return;

        _activeSequence = activeAnimation;

        _zoomableImage.Reset();
        _zoomableImage.Zoom(2f);
    }

    private void ProcessAnimationFrameChanged(AnimationFrameChangedMessage message)
    {
        if (_player != message.Source)
            return;

        SetFrame(message.Frame);
    }
}