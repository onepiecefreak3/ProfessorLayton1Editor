using CrossCutting.Core.Contract.EventBrokerage;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Messages.DataClasses;
using Vortice.Direct3D11.Debug;

namespace UI.Layton1Tool.Forms.Events.Views;

internal partial class EventTextControl
{
    private readonly Layton1NdsInfo _ndsInfo;
    private readonly IEventBroker _eventBroker;

    private int _textIndex;
    private int _subTextIndex;
    private TextElement[]? _texts;

    public EventTextControl(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, IFormFactory forms)
    {
        InitializeComponent(ndsInfo, forms);

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;

        _prevButton!.Clicked += _prevButton_Clicked;
        _nextButton!.Clicked += _nextButton_Clicked;

        eventBroker.Subscribe<SelectedEventViewTextsUpdatedMessage>(ProcessSelectedEventViewTextsUpdated);
    }

    private void _nextButton_Clicked(object? sender, EventArgs e)
    {
        if (_texts is null)
            return;

        if (_subTextIndex >= _texts[_textIndex].Texts.Length - 1)
        {
            if (_textIndex < _texts.Length - 1)
            {
                _textIndex++;
                _subTextIndex = 0;
            }
        }
        else
        {
            _subTextIndex++;
        }

        UpdateButtons();

        RaiseSelectedEventViewTextChanged(_texts[_textIndex], _subTextIndex);
    }

    private void _prevButton_Clicked(object? sender, EventArgs e)
    {
        if (_texts is null)
            return;

        if (_subTextIndex is 0)
        {
            if (_textIndex is not 0)
            {
                _textIndex--;
                _subTextIndex = _texts[_textIndex].Texts.Length - 1;
            }
        }
        else
        {
            _subTextIndex--;
        }

        UpdateButtons();

        RaiseSelectedEventViewTextChanged(_texts[_textIndex], _subTextIndex);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedEventViewTextsUpdatedMessage>(ProcessSelectedEventViewTextsUpdated);
    }

    private void ProcessSelectedEventViewTextsUpdated(SelectedEventViewTextsUpdatedMessage message)
    {
        if (_eventView != message.Source)
            return;

        if (message.KeepIndex)
        {
            int oldTextIndex = _textIndex;

            _textIndex = Math.Clamp(_textIndex, 0, message.Texts.Length);
            _subTextIndex = oldTextIndex == _textIndex ? Math.Clamp(_subTextIndex, 0, message.Texts[_textIndex].Texts.Length) : 0;
        }
        else
        {
            _textIndex = 0;
            _subTextIndex = 0;
        }

        _texts = message.Texts;
        
        UpdateButtons();

        RaiseSelectedEventViewTextChanged(_texts[_textIndex], _subTextIndex);
    }

    private void RaiseSelectedEventViewTextChanged(TextElement text, int textIndex)
    {
        _eventBroker.Raise(new SelectedEventViewTextChangedMessage(_eventView, _ndsInfo.Rom, text, textIndex));
    }

    private void UpdateButtons()
    {
        if (_texts is null)
            return;

        _prevButton.Enabled = _textIndex > 0 || _subTextIndex > 0;
        _nextButton.Enabled = _textIndex < _texts.Length - 1 || _subTextIndex < _texts[_textIndex].Texts.Length - 1;
    }
}