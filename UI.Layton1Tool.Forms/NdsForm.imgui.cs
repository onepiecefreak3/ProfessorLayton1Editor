using System.Numerics;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Models;
using ImGui.Forms.Models.IO;
using ImGuiNET;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Forms;

partial class NdsForm : Component
{
    private StackLayout _mainLayout;
    private StackLayout _fileLayout;
    private StackLayout _buttonLayout;
    private StackLayout _searchLayout;

    private ContextMenu _fileContextMenu;
    private MenuBarButton _extractButton;

    private ImageButton _saveButton;
    private ImageButton _saveAsButton;
    private ArrowButton _prevButton;
    private ArrowButton _nextButton;

    private TextBox _searchBox;
    private ImageButton _searchClearButton;
    private TreeView<Layton1NdsFile?> _fileTree;
    private Panel _contentPanel;

    private Component _imageForm;
    private Component _gdsForm;
    private Component _pcmForm;
    private Component _animForm;
    private Component _textForm;
    private Component _fontForm;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    protected override void SetTabInactiveCore()
    {
        _fileTree.SetTabInactive();
    }

    private void InitializeComponent(Layton1NdsInfo ndsInfo, IFormFactory formFactory, ILocalizationProvider localizations, IImageProvider images)
    {
        _imageForm = formFactory.CreateImageForm(ndsInfo);
        _gdsForm = formFactory.CreateGdsForm(ndsInfo);
        _pcmForm = formFactory.CreatePcmForm(ndsInfo);
        _animForm = formFactory.CreateAnimationForm(ndsInfo);
        _textForm = formFactory.CreateTextForm(ndsInfo);
        _fontForm = formFactory.CreateFontForm(ndsInfo);

        _extractButton = new MenuBarButton(localizations.MenuFileExtractCaption);
        _fileContextMenu = new ContextMenu { Items = { _extractButton } };

        _saveButton = new ImageButton(images.Save)
        {
            Enabled = false,
            ImageSize = new Vector2(16),
            Tooltip = localizations.MenuFileSaveCaption,
            KeyAction = new KeyCommand(ModifierKeys.Control, Key.S, localizations.MenuFileSaveShortcut)
        };
        _saveAsButton = new ImageButton(images.SaveAs)
        {
            Enabled = false,
            ImageSize = new Vector2(16),
            Tooltip = localizations.MenuFileSaveAsCaption,
            KeyAction = new KeyCommand(Key.F12, localizations.MenuFileSaveAsShortcut)
        };
        _prevButton = new ArrowButton(ImGuiDir.Left)
        {
            Enabled = false,
            KeyAction = new KeyCommand(ModifierKeys.Control, Key.Left, localizations.FileNdsHistoryPreviousShortcut)
        };
        _nextButton = new ArrowButton(ImGuiDir.Right)
        {
            Enabled = false,
            KeyAction = new KeyCommand(ModifierKeys.Control, Key.Right, localizations.FileNdsHistoryNextShortcut)
        };

        _fileTree = new TreeView<Layton1NdsFile?> { ContextMenu = _fileContextMenu };
        _searchBox = new TextBox { Width = SizeValue.Parent, Placeholder = localizations.FileNdsSearchPlaceholder };
        _searchClearButton = new ImageButton { Image = images.SearchClear, Tooltip = localizations.FileNdsSearchClear };
        _contentPanel = new Panel { Size = Size.Parent };

        _buttonLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.WidthAlign,
            ItemSpacing = 5,
            Items =
            {
                _saveButton,
                new StackItem(_saveAsButton){Size = Size.WidthAlign},
                _prevButton,
                _nextButton
            }
        };

        _searchLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.WidthAlign,
            ItemSpacing = 5,
            Items =
            {
                _searchBox,
                _searchClearButton
            }
        };

        _fileLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            Size = new Size(SizeValue.Relative(.3f), SizeValue.Parent),
            ItemSpacing = 5,
            Items =
            {
                _buttonLayout,
                _searchLayout,
                _fileTree
            }
        };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                _fileLayout,
                _contentPanel
            }
        };
    }
}