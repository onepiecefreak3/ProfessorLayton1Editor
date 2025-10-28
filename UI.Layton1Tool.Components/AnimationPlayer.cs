using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Resources;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using UI.Layton1Tool.Components.Contract.DataClasses;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Components;

partial class AnimationPlayer
{
    private readonly Layton1NdsInfo _ndsInfo;
    private readonly IEventBroker _eventBroker;
    private readonly IImageProvider _images;

    private AnimationState? _animationState;
    private bool _isPlaying = true;

    public AnimationPlayer(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILocalizationProvider localizations, IImageProvider images)
    {
        InitializeComponent(localizations, images);

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;
        _images = images;

        _speedInput!.TextChanged += _speedInput_TextChanged;
        _playButton!.Clicked += _playButton_Clicked;
        _stepBackButton!.Clicked += _stepBackButton_Clicked;
        _frameBackButton!.Clicked += _frameBackButton_Clicked;
        _frameForwardButton!.Clicked += _frameForwardButton_Clicked;
        _stepForwardButton!.Clicked += _stepForwardButton_Clicked;

        eventBroker.Subscribe<SelectedAnimationChangedMessage>(ProcessSelectedAnimationChanged);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedAnimationChangedMessage>(ProcessSelectedAnimationChanged);
    }

    private void ProcessSelectedAnimationChanged(SelectedAnimationChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_animationState is null || _animationState.Animations != message.AnimationSequences)
        {
            var loadedImages = new ImageResource[message.AnimationSequences.Frames.Length];
            for (var i = 0; i < loadedImages.Length; i++)
                loadedImages[i] = ImageResource.FromImage(message.AnimationSequences.Frames[i].Image);

            _animationState = new AnimationState
            {
                Animations = message.AnimationSequences,
                Images = loadedImages,
                FrameSpeed = 1f
            };
        }

        if (message.SequenceId < 0 && message.SequenceId >= message.AnimationSequences.Sequences.Length)
            return;

        AnimationSequence activeAnimation = message.AnimationSequences.Sequences[message.SequenceId];

        if (_animationState.ActiveAnimation == activeAnimation)
            return;

        _animationState.TotalFrameCounter = 0;
        _animationState.FrameCounter = 0;
        _animationState.StepCounter = 0;
        _animationState.ActiveAnimation = activeAnimation;

        _speedInput.Text = $"{_animationState.FrameSpeed.ToString("0.00", CultureInfo.InvariantCulture)}";

        UpdatePlayButtons();
    }

    private void _stepForwardButton_Clicked(object? sender, EventArgs e)
    {
        if (_animationState?.ActiveAnimation is null)
            return;

        if (_animationState.StepCounter + 1 >= _animationState.ActiveAnimation.Steps.Length)
            return;

        _animationState.TotalFrameCounter += _animationState.ActiveAnimation.Steps[_animationState.StepCounter].FrameCounter - _animationState.FrameCounter;
        _animationState.FrameCounter = 0;
        _animationState.StepCounter++;

        UpdatePlayButtons();
    }

    private void _frameForwardButton_Clicked(object? sender, EventArgs e)
    {
        if (_animationState?.ActiveAnimation is null)
            return;

        int totalFrames = _animationState.ActiveAnimation.Steps.Sum(x => x.FrameCounter);

        _animationState.TotalFrameCounter = Math.Min(totalFrames - 1, _animationState.TotalFrameCounter + 1);

        AnimationStep step = _animationState.ActiveAnimation.Steps[_animationState.StepCounter];

        if (_animationState.FrameCounter + 1 >= step.FrameCounter)
        {
            if (_animationState.StepCounter + 1 >= _animationState.ActiveAnimation.Steps.Length)
                return;

            _animationState.StepCounter = Math.Min(_animationState.ActiveAnimation.Steps.Length - 1, _animationState.StepCounter + 1);
            _animationState.FrameCounter = 0;
        }
        else
        {
            _animationState.FrameCounter++;
        }

        UpdatePlayButtons();
    }

    private void _frameBackButton_Clicked(object? sender, EventArgs e)
    {
        if (_animationState?.ActiveAnimation is null)
            return;

        _animationState.TotalFrameCounter = Math.Max(0, _animationState.TotalFrameCounter - 1);

        if (_animationState.FrameCounter <= 0)
        {
            if (_animationState.StepCounter <= 0)
                return;

            AnimationStep step = _animationState.ActiveAnimation.Steps[_animationState.StepCounter - 1];

            _animationState.StepCounter--;
            _animationState.FrameCounter = step.FrameCounter;
        }

        _animationState.FrameCounter--;

        UpdatePlayButtons();
    }

    private void _stepBackButton_Clicked(object? sender, EventArgs e)
    {
        if (_animationState?.ActiveAnimation is null)
            return;

        if (_animationState.FrameCounter is not 0)
        {
            _animationState.TotalFrameCounter -= _animationState.FrameCounter;
            _animationState.FrameCounter = 0;
        }
        else
        {
            _animationState.StepCounter = Math.Max(0, _animationState.StepCounter - 1);
            _animationState.TotalFrameCounter -= _animationState.ActiveAnimation.Steps[_animationState.StepCounter].FrameCounter;
        }

        UpdatePlayButtons();
    }

    private void _playButton_Clicked(object? sender, EventArgs e)
    {
        _isPlaying = !_isPlaying;

        _speedInput.Enabled = _isPlaying;

        if (_animationState is not null)
        {
            _animationState.TotalFrameCounter = (int)_animationState.TotalFrameCounter;
            _animationState.FrameCounter = (int)_animationState.FrameCounter;
        }

        UpdatePlayButtons();
    }

    private void _speedInput_TextChanged(object? sender, EventArgs e)
    {
        if (_animationState is null)
            return;

        if (string.IsNullOrEmpty(_speedInput.Text))
            return;

        if (!float.TryParse(_speedInput.Text, CultureInfo.InvariantCulture, out float speed))
            return;

        _animationState.FrameSpeed = speed;
    }

    private bool TryGetAnimationState([NotNullWhen(true)] out AnimationState? animationState)
    {
        animationState = _animationState;
        return animationState is not null;
    }

    private static float Step(AnimationState animationState, float frameSpeed)
    {
        if (animationState.ActiveAnimation is null)
            return frameSpeed;

        int stepIndex = animationState.StepCounter;
        if (stepIndex >= animationState.ActiveAnimation.Steps.Length)
            return frameSpeed;

        AnimationStep step = animationState.ActiveAnimation.Steps[stepIndex];

        var stepTotalFrames = 0;
        for (var i = 0; i < stepIndex; i++)
            stepTotalFrames += animationState.ActiveAnimation.Steps[i].FrameCounter;

        float maxFrameSpeed = Math.Min(frameSpeed, 1);

        animationState.FrameCounter += maxFrameSpeed;
        animationState.TotalFrameCounter = stepTotalFrames + animationState.FrameCounter;

        if (animationState.FrameCounter < step.FrameCounter)
            return maxFrameSpeed;

        animationState.StepCounter = step.NextStepIndex;
        animationState.FrameCounter = 0;

        return maxFrameSpeed;
    }

    private void UpdatePlayButtons()
    {
        if (_animationState?.ActiveAnimation is null)
            return;

        AnimationStep[] steps = _animationState.ActiveAnimation.Steps;

        int totalFrames = steps.Sum(x => x.FrameCounter);

        _stepBackButton.Enabled = !_isPlaying && (_animationState.StepCounter > 0 || _animationState.FrameCounter > 0);
        _frameBackButton.Enabled = !_isPlaying && _animationState.TotalFrameCounter > 0;
        _frameForwardButton.Enabled = !_isPlaying && _animationState.TotalFrameCounter < totalFrames - 1;
        _stepForwardButton.Enabled = !_isPlaying && _animationState.StepCounter < steps.Length - 1;

        _playButton.Image = _isPlaying ? _images.Pause : _images.Play;
    }

    private void RaiseAnimationFrameChanged(AnimationState animationState)
    {
        if (animationState.ActiveAnimation is null)
            return;

        int stepIndex = animationState.StepCounter;
        if (stepIndex >= animationState.ActiveAnimation.Steps.Length)
            return;

        AnimationStep step = animationState.ActiveAnimation.Steps[stepIndex];

        int frameIndex = step.FrameIndex;
        if (frameIndex < animationState.Images.Length)
            return;

        _eventBroker.Raise(new AnimationFrameChangedMessage(this, animationState.Images[frameIndex]));
    }
}