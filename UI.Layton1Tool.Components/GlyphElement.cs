using CrossCutting.Core.Contract.EventBrokerage;
using Konnect.Contract.DataClasses.Plugin.File.Font;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Components;

partial class GlyphElement
{
    private readonly CharacterInfo _charInfo;
    private readonly IEventBroker _eventBroker;

    public GlyphElement(CharacterInfo charInfo, IEventBroker eventBroker, IFontFactory fonts)
    {
        InitializeComponent(charInfo, fonts);

        _charInfo = charInfo;
        _eventBroker = eventBroker;

        _eventBroker.Subscribe<UpdateSelectedGlyphElementMessage>(ProcessSelectedGlyphElement);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<UpdateSelectedGlyphElementMessage>(ProcessSelectedGlyphElement);
    }

    private void ProcessSelectedGlyphElement(UpdateSelectedGlyphElementMessage message)
    {
        if (message.Target != this)
            return;

        IsSelected = message.IsSelected;
    }

    private void RaiseSelectedGlyphElementChanged()
    {
        _eventBroker.Raise(new SelectedGlyphElementChangedMessage(this, IsSelected));
    }
}