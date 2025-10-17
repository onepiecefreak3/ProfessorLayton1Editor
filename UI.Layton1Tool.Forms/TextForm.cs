using CrossCutting.Core.Contract.EventBrokerage;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Forms;

partial class TextForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    public TextForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker)
    {
        InitializeComponent();

        _ndsInfo = ndsInfo;

        eventBroker.Subscribe<SelectedTextChangedMessage>(UpdateText);
    }

    private void UpdateText(SelectedTextChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        _textEditor.SetText(message.Text);
    }
}