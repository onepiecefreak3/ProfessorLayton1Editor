using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms;
using ImGui.Forms.Controls;
using ImGui.Forms.Localization;
using ImGui.Forms.Modals;
using ImGui.Forms.Modals.IO;
using ImGui.Forms.Modals.IO.Windows;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using UI.Layton1Tool.Dialogs.Contract;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Messages.Enums;
using UI.Layton1Tool.Resources.Contract;
using Veldrid.Sdl2;

namespace UI.Layton1Tool.Forms;

partial class MainForm
{
    private readonly IEventBroker _eventBroker;
    private readonly ILocalizationProvider _localizations;
    private readonly IColorProvider _colors;
    private readonly ISettingsProvider _settings;
    private readonly ILayton1NdsParser _ndsParser;
    private readonly ILayton1NdsComposer _ndsComposer;
    private readonly IFormFactory _formFactory;
    private readonly IDialogFactory _dialogFactory;

    private readonly Dictionary<string, Layton1NdsTabPage> _loadedFiles = [];
    private readonly Dictionary<TabPage, Layton1NdsTabPage> _loadedTabs = [];

    public MainForm(IEventBroker eventBroker, ILocalizationProvider localizations, IColorProvider colors, ISettingsProvider settings,
        ILayton1NdsParser ndsParser, ILayton1NdsComposer ndsComposer, IFormFactory formFactory, IDialogFactory dialogFactory,
        IImageProvider images)
    {
        InitializeComponent(localizations, images);

        _eventBroker = eventBroker;
        _localizations = localizations;
        _colors = colors;
        _settings = settings;
        _ndsParser = ndsParser;
        _ndsComposer = ndsComposer;
        _formFactory = formFactory;
        _dialogFactory = dialogFactory;

        _fileOpenButton!.Clicked += _fileOpenButton_Clicked;
        _fileValidateButton!.Clicked += _fileValidateButton_Clicked;
        _fileSearchButton!.Clicked += _fileSearchButton_Clicked;
        _tabControl!.PageRemoving += _tabControl_PageRemoving;
        _tabControl.PageRemoved += _tabControl_PageRemoved;
        _tabControl.SelectedPageChanged += _tabControl_SelectedPageChanged;

        eventBroker.Subscribe<UpdateStatusMessage>(UpdateStatus);
        eventBroker.Subscribe<NdsFileModifiedMessage>(ProcessNdsFileModified);
        eventBroker.Subscribe<NdsFileSaveRequestedMessage>(ProcessNdsFileSaveRequested);

        AllowDragDrop = true;
        DragDrop += MainForm_DragDrop;

        Closing += MainForm_Closing;
    }

    private async void _fileSearchButton_Clicked(object? sender, EventArgs e)
    {
        if (_tabControl.SelectedPage is null)
            return;

        if (!_loadedTabs.TryGetValue(_tabControl.SelectedPage, out Layton1NdsTabPage? loadedPage))
            return;

        Modal dialog = _dialogFactory.CreateSearchDialog(loadedPage.Rom);
        await dialog.ShowAsync();

        dialog.Destroy();
    }

    private async Task MainForm_Closing(object sender, ClosingEventArgs e)
    {
        bool hasChanges = _loadedTabs.Keys.Any(t => t.HasChanges);

        if (!hasChanges)
            return;

        DialogResult result = await MessageBox.ShowYesNoCancelAsync(_localizations.DialogUnsavedChangesCaption,
            _localizations.DialogUnsavedChangesText);

        if (result is DialogResult.Yes)
            await SaveAll();

        e.Cancel = result is DialogResult.Cancel;
    }

    private void _tabControl_SelectedPageChanged(object? sender, EventArgs e)
    {
        if (_tabControl.SelectedPage is null)
            return;

        bool isLoaded = _loadedTabs.ContainsKey(_tabControl.SelectedPage);

        _fileValidateButton.Enabled = isLoaded;
        _fileSearchButton.Enabled = isLoaded;
    }

    private async void _fileValidateButton_Clicked(object? sender, EventArgs e)
    {
        if (_tabControl.SelectedPage is null)
            return;

        if (!_loadedTabs.TryGetValue(_tabControl.SelectedPage, out Layton1NdsTabPage? loadedPage))
            return;

        Modal dialog = _dialogFactory.CreateValidationDialog(loadedPage.Rom);
        await dialog.ShowAsync();

        dialog.Destroy();
    }

    private async Task _tabControl_PageRemoving(object arg1, RemovingEventArgs arg2)
    {
        if (!arg2.Page.HasChanges)
            return;

        DialogResult result = await MessageBox.ShowYesNoCancelAsync(_localizations.DialogUnsavedChangesCaption,
            _localizations.DialogUnsavedChangesText);

        switch (result)
        {
            case DialogResult.Cancel:
                arg2.Cancel = true;
                break;

            case DialogResult.Yes:
                {
                    if (!_loadedTabs.TryGetValue(_tabControl.SelectedPage, out Layton1NdsTabPage? loadedPage))
                        return;

                    await Save(loadedPage, false);
                    break;
                }
        }
    }

    private void _tabControl_PageRemoved(object? sender, RemoveEventArgs e)
    {
        if (!_loadedTabs.TryGetValue(e.Page, out Layton1NdsTabPage? loadedPage))
            return;

        _loadedTabs.Remove(loadedPage.Page);
        _loadedFiles.Remove(loadedPage.Path);

        _tabControl.RemovePage(loadedPage.Page);

        if (_tabControl.SelectedPage is null)
        {
            if (_tabControl.Pages.Count <= 0)
            {
                _fileValidateButton.Enabled = false;
                _fileSearchButton.Enabled = false;
            }
            else
            {
                _tabControl.SelectedPage = _tabControl.Pages[0];
                _fileValidateButton.Enabled = _loadedTabs.ContainsKey(_tabControl.SelectedPage);
                _fileSearchButton.Enabled = _loadedTabs.ContainsKey(_tabControl.SelectedPage);
            }
        }
        else
        {
            _fileValidateButton.Enabled = _loadedTabs.ContainsKey(_tabControl.SelectedPage);
            _fileSearchButton.Enabled = _loadedTabs.ContainsKey(_tabControl.SelectedPage);
        }

        loadedPage.Stream.Close();

        loadedPage.Page.Content.Destroy();
    }

    private void MainForm_DragDrop(object? sender, DragDropEvent[] e)
    {
        foreach (DragDropEvent evt in e)
            OpenNdsFile(evt.File);
    }

    private async void _fileOpenButton_Clicked(object? sender, EventArgs e)
    {
        await OpenNdsFile();
    }

    private async Task OpenNdsFile()
    {
        string? ndsPath = await SelectOpenFile();
        if (ndsPath is null)
            return;

        OpenNdsFile(ndsPath);
    }

    private void OpenNdsFile(string ndsPath)
    {
        if (_loadedFiles.TryGetValue(ndsPath, out Layton1NdsTabPage? loadedPage))
        {
            _tabControl.SelectedPage = loadedPage.Page;
            return;
        }

        Stream ndsStream = File.OpenRead(ndsPath);

        Layton1NdsRom? ndsRom = TryParseNds(ndsStream);
        if (ndsRom is null)
        {
            ndsStream.Close();
            return;
        }

        loadedPage = CreateTabPage(ndsStream, ndsRom, ndsPath);

        _loadedFiles[ndsPath] = loadedPage;
        _loadedTabs[loadedPage.Page] = loadedPage;

        _tabControl.AddPage(loadedPage.Page);
        _tabControl.SelectedPage = loadedPage.Page;

        UpdateStatus(string.Empty, Status.Info);
    }

    private async Task<string?> SelectOpenFile()
    {
        var ofd = new WindowsOpenFileDialog
        {
            Title = _localizations.DialogFileNdsOpenCaption,
            Filters = [new FileFilter(_localizations.DialogFileNdsOpenFilter, ".nds")],
            InitialDirectory = _settings.OpenDirectory,
        };

        DialogResult result = await ofd.ShowAsync();
        if (result is not DialogResult.Ok)
        {
            UpdateStatus(_localizations.StatusNdsSelectError, Status.Error);
            return null;
        }

        string? directory = Path.GetDirectoryName(ofd.Files[0]);
        if (directory is not null)
            _settings.OpenDirectory = directory;

        return ofd.Files[0];
    }

    private async Task<string?> SelectSaveFile()
    {
        var ofd = new WindowsSaveFileDialog
        {
            Title = _localizations.DialogFileNdsSaveCaption,
            Filters = [new FileFilter(_localizations.DialogFileNdsSaveFilter, ".nds")],
            InitialDirectory = _settings.SaveDirectory
        };

        DialogResult result = await ofd.ShowAsync();
        if (result is not DialogResult.Ok)
        {
            UpdateStatus(_localizations.StatusNdsSelectError, Status.Error);
            return null;
        }

        string? directory = Path.GetDirectoryName(ofd.Files[0]);
        if (directory is not null)
            _settings.SaveDirectory = directory;

        return ofd.Files[0];
    }

    private Layton1NdsRom? TryParseNds(Stream input)
    {
        try
        {
            return _ndsParser.Parse(input);
        }
        catch (Exception)
        {
            UpdateStatus(_localizations.StatusNdsOpenError, Status.Error);
            return null;
        }
    }

    private Layton1NdsTabPage CreateTabPage(Stream ndsStream, Layton1NdsRom ndsRom, string ndsPath)
    {
        var ndsInfo = new Layton1NdsInfo { Path = ndsPath, Rom = ndsRom };
        var tabPage = new TabPage(_formFactory.CreateNdsForm(ndsInfo))
        {
            Title = Path.GetFileName(ndsPath)
        };

        return new Layton1NdsTabPage
        {
            Path = ndsPath,
            Stream = ndsStream,
            Rom = ndsRom,
            Page = tabPage
        };
    }

    private async Task SaveAll()
    {
        foreach (Layton1NdsTabPage tabPage in _loadedFiles.Values)
            await Save(tabPage, false);
    }

    private async Task Save(Layton1NdsTabPage tabPage, bool isSaveAs)
    {
        if (!tabPage.Page.HasChanges)
            return;

        string? outputPath;
        string? finalPath;

        if (isSaveAs)
        {
            finalPath = await SelectSaveFile();
            if (finalPath is null)
                return;

            outputPath = tabPage.Path == finalPath ? Path.GetTempFileName() : finalPath;
        }
        else
        {
            finalPath = tabPage.Path;
            outputPath = Path.GetTempFileName();
        }

        Stream output = File.Create(outputPath);
        _ndsComposer.Compose(tabPage.Rom, output);

        output.Close();
        tabPage.Stream.Close();

        if (tabPage.Path == finalPath)
        {
            try
            {
                File.Delete(finalPath);
                File.Move(outputPath, finalPath);
            }
            catch (Exception e)
            {
                UpdateStatus(_localizations.StatusNdsSaveError, Status.Error, e);
                File.Delete(outputPath);

                finalPath = tabPage.Path;
            }
        }

        Layton1NdsRom originalRom = tabPage.Rom;

        tabPage.Path = finalPath;
        tabPage.Stream = File.OpenRead(finalPath);
        tabPage.Rom = _ndsParser.Parse(tabPage.Stream);

        tabPage.Page.HasChanges = false;

        RaiseNdsFileSaved(originalRom, tabPage.Path, tabPage.Rom);
    }

    private void UpdateStatus(UpdateStatusMessage message)
    {
        UpdateStatus(message.Text, message.Status, message.Exception);
    }

    private void UpdateStatus(LocalizedString text, Status status, Exception? e = null)
    {
        ToggleStatus(!string.IsNullOrEmpty(text));

        _statusLabel.Text = text;
        _statusLabel.TextColor = status switch
        {
            Status.Info => _colors.Default,
            Status.Error => _colors.Error,
            _ => _colors.Default
        };
    }

    private void ProcessNdsFileModified(NdsFileModifiedMessage message)
    {
        if (!_loadedFiles.TryGetValue(message.RomPath, out Layton1NdsTabPage? tabPage))
            return;

        tabPage.Page.HasChanges = true;
    }

    private async Task ProcessNdsFileSaveRequested(NdsFileSaveRequestedMessage message)
    {
        if (!_loadedFiles.TryGetValue(message.RomPath, out Layton1NdsTabPage? tabPage))
            return;

        await Save(tabPage, message.IsSaveAs);
    }

    private void RaiseNdsFileSaved(Layton1NdsRom originalRom, string romPath, Layton1NdsRom ndsRom)
    {
        _eventBroker.Raise(new NdsFileSavedMessage(originalRom, romPath, ndsRom));
    }
}