using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Lists;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Modals;
using ImGui.Forms.Models;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Dialogs;

partial class ValidationDialog : Modal
{
    private StackLayout _mainLayout;
    private StackLayout _progressLayout;

    private ContextMenu _fileContextMenu;
    private MenuBarButton _extractButton;

    private DataTable<Layton1ValidationError> _errorTable;
    private DataTable<Layton1NdsFile> _warningTable;
    private Button _cancelButton;

    private ProgressBar _progressBar;

    private void InitializeComponent(ILocalizationProvider localizations, IColorProvider colors)
    {
        _progressBar = new ProgressBar
        {
            ProgressColor = colors.Progress,
            Size = new Size(SizeValue.Parent, 24)
        };

        _extractButton = new MenuBarButton(localizations.MenuFileExtractCaption) { Enabled = false };
        _fileContextMenu = new ContextMenu { Items = { _extractButton } };

        _errorTable = new DataTable<Layton1ValidationError>
        {
            Size = Size.Parent,
            IsSelectable = true,
            CanSelectMultiple = true,
            IsResizable = true,
            ShowHeaders = true,
            Columns =
            {
                new DataTableColumn<Layton1ValidationError>(error => error.File.Path, localizations.DialogValidationPath),
                new DataTableColumn<Layton1ValidationError>(error => localizations.DialogValidationErrorText(error.Error),
                    localizations.DialogValidationErrorCaption)
            },
            ContextMenu = _fileContextMenu
        };

        _warningTable = new DataTable<Layton1NdsFile>
        {
            Size = Size.Parent,
            IsSelectable = true,
            CanSelectMultiple = true,
            IsResizable = true,
            ShowHeaders = true,
            Columns =
            {
                new DataTableColumn<Layton1NdsFile>(error => error.Path, localizations.DialogValidationPath)
            },
            ContextMenu = _fileContextMenu
        };

        _cancelButton = new Button(localizations.DialogValidationCancelCaption);

        _progressLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.WidthAlign,
            ItemSpacing = 5,
            Items =
            {
                _progressBar,
                new StackItem(_cancelButton) { VerticalAlignment = VerticalAlignment.Center }
            }
        };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                _errorTable,
                _warningTable,
                _progressLayout
            }
        };

        Size = new Size(SizeValue.Relative(.3f), SizeValue.Relative(.6f));
        Content = _mainLayout;
        Caption = localizations.DialogValidationCaption;
    }
}