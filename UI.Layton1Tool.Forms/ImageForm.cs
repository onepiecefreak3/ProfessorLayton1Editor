using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Modals;
using ImGui.Forms.Modals.IO.Windows;
using ImGui.Forms.Resources;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
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
    private Layton1NdsFile? _selectedFile;

    public ImageForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, IImageProvider images, ILocalizationProvider localizations,
        ISettingsProvider settings)
    {
        InitializeComponent(images, localizations);

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;
        _localizations = localizations;
        _settings = settings;

        _extractButton!.Clicked += _extractButton_Clicked;

        eventBroker.Subscribe<SelectedFileChangedMessage>(UpdateImage);
        eventBroker.Subscribe<FileContentModifiedMessage>(UpdateImage);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedFileChangedMessage>(UpdateImage);
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(UpdateImage);
    }

    private async void _extractButton_Clicked(object? sender, EventArgs e)
    {
        if (_image is null)
            return;

        var sfd = new WindowsSaveFileDialog
        {
            Title = _localizations.ImageMenuExportPng,
            InitialDirectory = GetPreviewDirectory(),
            InitialFileName = "image.png"
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

    private void UpdateImage(SelectedFileChangedMessage message)
    {
        UpdateImage(message.File, message.Content);
    }

    private void UpdateImage(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (message.File != _selectedFile)
            return;

        UpdateImage(message.File, message.Content);
    }

    private void UpdateImage(Layton1NdsFile file, object? content)
    {
        if (content is not Image<Rgba32> image)
            return;

        if (file.Rom != _ndsInfo.Rom)
            return;

        _image = image;
        _selectedFile = file;

        _extractButton.Enabled = true;

        _zoomableImage.Image = ImageResource.FromImage(image);

        _zoomableImage.Reset();
        _zoomableImage.Zoom(2f);
    }

    private string GetPreviewDirectory()
    {
        var settingsDir = _settings.PreviewDirectory;
        return string.IsNullOrEmpty(settingsDir) ? Path.GetFullPath(".") : settingsDir;
    }
}