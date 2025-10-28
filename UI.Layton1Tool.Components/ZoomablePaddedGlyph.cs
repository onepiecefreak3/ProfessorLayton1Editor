using System.Diagnostics.CodeAnalysis;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Resources;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Messages.DataClasses;

namespace UI.Layton1Tool.Components;

partial class ZoomablePaddedGlyph
{
    private readonly IEventBroker _eventBroker;

    private PaddedGlyph? _paddedGlyph;
    private ImageResource? _paddedGlyphResource;

    public ZoomablePaddedGlyph(IEventBroker eventBroker)
    {
        _eventBroker = eventBroker;

        _eventBroker.Subscribe<UpdatePaddedGlyphZoomMessage>(ProcessUpdatePaddedGlyphZoom);
        eventBroker.Subscribe<SelectedPaddedGlyphChangedMessage>(ProcessSelectedPaddedGlyphChanged);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedPaddedGlyphChangedMessage>(ProcessSelectedPaddedGlyphChanged);
    }

    private void ProcessSelectedPaddedGlyphChanged(SelectedPaddedGlyphChangedMessage message)
    {
        if (message.Target != this)
            return;

        UpdatePaddedGlyph(message.PaddedGlyph);
    }

    private void ProcessUpdatePaddedGlyphZoom(UpdatePaddedGlyphZoomMessage message)
    {
        if (message.Target != this)
            return;

        Zoom(message.Zoom);
    }

    public void UpdatePaddedGlyph(PaddedGlyph? paddedGlyph)
    {
        _paddedGlyph = paddedGlyph;
        UpdateGlyphImage(paddedGlyph);
    }

    private bool TryGetPaddedGlyph([NotNullWhen(true)] out PaddedGlyph? paddedGlyph)
    {
        paddedGlyph = _paddedGlyph;
        return paddedGlyph is not null;
    }
}