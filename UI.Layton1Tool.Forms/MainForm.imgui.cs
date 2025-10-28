using System.Numerics;
using ImGui.Forms;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Models.IO;
using ImGui.Forms.Resources;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Forms;

partial class MainForm : Form
{
    private MainMenuBar? _mainMenuBar;
    private MenuBarMenu? _fileMenuItem;
    private MenuBarButton _fileValidateButton;
    private MenuBarButton _fileSearchButton;
    private MenuBarButton _fileOpenButton;

    private StackLayout _contentLayout;

    private TabControl _tabControl;
    private Label _statusLabel;

    private void InitializeComponent(ILocalizationProvider localizations, IImageProvider images)
    {
        _fileValidateButton = new MenuBarButton { Text = localizations.MenuFileValidateCaption, Enabled = false };
        _fileSearchButton = new MenuBarButton { Text = localizations.MenuFileSearchCaption, Enabled = false };
        _fileOpenButton = new MenuBarButton
        {
            Text = localizations.MenuFileOpenCaption,
            KeyAction = new KeyCommand(ModifierKeys.Control, Key.O, localizations.MenuFileOpenShortcut)
        };

        _fileMenuItem = new MenuBarMenu
        {
            Text = localizations.MenuFileCaption,
            Items =
            {
                _fileOpenButton
            }
        };

        _mainMenuBar = new MainMenuBar
        {
            Items =
            {
                _fileMenuItem,
                _fileValidateButton,
                _fileSearchButton
            }
        };

        _tabControl = new TabControl();
        _statusLabel = new Label();

        _contentLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            Size = ImGui.Forms.Models.Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                _tabControl
            }
        };

        Title = localizations.ApplicationTitle;
        Size = new Vector2(1100, 700);

        MenuBar = _mainMenuBar;
        Content = _contentLayout;

        Icon = images.Icon;
    }

    private void ToggleStatus(bool toggle)
    {
        if (!toggle)
        {
            if (_contentLayout.Items.Count <= 1)
                return;

            _contentLayout.Items.RemoveAt(1);
        }
        else
        {
            var item = new StackItem(_statusLabel) { Size = ImGui.Forms.Models.Size.Content };

            if (_contentLayout.Items.Count <= 1)
                _contentLayout.Items.Add(item);
            else
                _contentLayout.Items[1] = item;
        }
    }
}