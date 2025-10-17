using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Resources;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Forms;

partial class BgxForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    public BgxForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker)
    {
        InitializeComponent();

        _ndsInfo = ndsInfo;

        eventBroker.Subscribe<SelectedBgxChangedMessage>(UpdateImage);
    }

    private void UpdateImage(SelectedBgxChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        _zoomableImage.Image = ImageResource.FromImage(message.Image);

        _zoomableImage.Reset();
        _zoomableImage.Zoom(2f);
    }
}