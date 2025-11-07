using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Forms;

partial class TextForm
{
    private readonly Layton1NdsInfo _ndsInfo;
    private readonly IEventBroker _eventBroker;

    private Layton1NdsFile? _selectedFile;

    public TextForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker)
    {
        InitializeComponent();

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;

        eventBroker.Subscribe<SelectedFileChangedMessage>(UpdateText);
        eventBroker.Subscribe<FileContentModifiedMessage>(UpdateText);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedFileChangedMessage>(UpdateText);
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(UpdateText);
    }

    private void UpdateText(SelectedFileChangedMessage message)
    {
        UpdateText(message.File, message.Content);
    }

    private void UpdateText(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (message.File != _selectedFile)
            return;

        UpdateText(message.File, message.Content);
    }

    private void UpdateText(Layton1NdsFile file, object? content)
    {
        if (content is not string text)
            return;

        if (file.Rom != _ndsInfo.Rom)
            return;

        _selectedFile = file;

        _textEditor.SetText(text);
    }
}