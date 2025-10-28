using System.Diagnostics.CodeAnalysis;
using CrossCutting.Core.Contract.EventBrokerage;
using Konnect.Contract.DataClasses.Plugin.File.Font;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Components;

partial class ZoomableCharacterInfo
{
    private readonly IEventBroker _eventBroker;

    private CharacterInfo? _charInfo;

    public ZoomableCharacterInfo(IEventBroker eventBroker)
    {
        _eventBroker = eventBroker;

        _eventBroker.Subscribe<UpdateCharacterInfoZoomMessage>(ProcessUpdateCharacterInfoZoom);
        _eventBroker.Subscribe<SelectedCharacterInfoChangedMessage>(ProcessSelectedCharacterInfoChanged);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<UpdateCharacterInfoZoomMessage>(ProcessUpdateCharacterInfoZoom);
        _eventBroker.Unsubscribe<SelectedCharacterInfoChangedMessage>(ProcessSelectedCharacterInfoChanged);
    }

    private void ProcessSelectedCharacterInfoChanged(SelectedCharacterInfoChangedMessage message)
    {
        if (message.Target != this)
            return;

        UpdateCharacterInfo(message.CharacterInfo);
    }

    private void ProcessUpdateCharacterInfoZoom(UpdateCharacterInfoZoomMessage message)
    {
        if (message.Target != this)
            return;

        Zoom(message.Zoom);
    }

    private void UpdateCharacterInfo(CharacterInfo charInfo)
    {
        _charInfo = charInfo;
        UpdateGlyphImage(charInfo);
    }

    private bool TryGetCharacterInfo([NotNullWhen(true)] out CharacterInfo? charInfo)
    {
        charInfo = _charInfo;
        return charInfo is not null;
    }
}