using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Modals.IO.Windows;
using ImGui.Forms.Modals;
using ImGui.Forms.Resources;
using System.Runtime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms;

partial class ImageForm
{
    private readonly Layton1NdsInfo _ndsInfo;
    private readonly IEventBroker _eventBroker;
    private readonly ILocalizationProvider _localizations;
    private readonly ISettingsProvider _settings;

    private Image<Rgba32>? _image;

    public ImageForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, IImageProvider images, ILocalizationProvider localizations,
        ISettingsProvider settings)
    {
        InitializeComponent(images, localizations);

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;
        _localizations = localizations;
        _settings = settings;

        _extractButton!.Clicked += _extractButton_Clicked;

        eventBroker.Subscribe<SelectedImageChangedMessage>(UpdateImage);
    }

    private async void _extractButton_Clicked(object? sender, EventArgs e)
    {
        if (_image is null)
            return;

        var sfd = new WindowsSaveFileDialog
        {
            Title = _localizations.ImageMenuExportPng,
            InitialDirectory = GetPreviewDirectory(),
            InitialFileName = "banner.png"
        };

        DialogResult result = await sfd.ShowAsync();

        if (result is not DialogResult.Ok)
            return;

        await _image.SaveAsPngAsync(sfd.Files[0]);

        string? selectedDir = Path.GetDirectoryName(sfd.Files[0]);
        if (string.IsNullOrEmpty(selectedDir))
            return;

        _settings.PreviewDirectory = selectedDir;
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedImageChangedMessage>(UpdateImage);
    }

    private void UpdateImage(SelectedImageChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        _image = message.Image;

        _extractButton.Enabled = true;

        _zoomableImage.Image = ImageResource.FromImage(message.Image);

        _zoomableImage.Reset();
        _zoomableImage.Zoom(2f);
    }

    private string GetPreviewDirectory()
    {
        var settingsDir = _settings.PreviewDirectory;
        return string.IsNullOrEmpty(settingsDir) ? Path.GetFullPath(".") : settingsDir;
    }
}