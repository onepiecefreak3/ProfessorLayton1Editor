using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Lists;
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Modals;
using ImGui.Forms.Models;
using ImGui.Forms.Models.IO;
using UI.Layton1Tool.Dialogs.DataClasses;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Dialogs;

partial class SearchDialog : Modal
{
    private StackLayout _mainLayout;
    private StackLayout _inputLayout;
    private StackLayout _progressLayout;

    private DataTable<SearchResult> _matchTable;

    private TextBox _inputText;
    private Button _searchButton;

    private ProgressBar _progressBar;
    private Button _cancelButton;

    private void InitializeComponent(ILocalizationProvider localizations, IColorProvider colors)
    {
        _inputText = new TextBox { Placeholder = localizations.DialogSearchPlaceholderCaption };
        _searchButton = new Button
        {
            Text = localizations.DialogSearchExecuteCaption,
            Enabled = false,
            KeyAction = new KeyCommand(Key.Enter)
        };

        _progressBar = new ProgressBar
        {
            ProgressColor = colors.Progress,
            Size = new Size(SizeValue.Parent, 24)
        };

        _cancelButton = new Button(localizations.DialogSearchCancelCaption) { Enabled = false };

        _matchTable = new DataTable<SearchResult>
        {
            Size = Size.Parent,
            IsSelectable = true,
            IsResizable = true,
            ShowHeaders = true,
            Columns =
            {
                new DataTableColumn<SearchResult>(error => error.File.Path, localizations.DialogSearchPath),
                new DataTableColumn<SearchResult>(error => error.SubFile?.Name ?? string.Empty, localizations.DialogSearchSubPath)
            }
        };

        _inputLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.Content,
            ItemSpacing = 5,
            Items =
            {
                new StackItem(_inputText) { VerticalAlignment = VerticalAlignment.Center },
                _searchButton
            }
        };

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
                _inputLayout,
                _matchTable,
                _progressLayout
            }
        };

        Size = new Size(SizeValue.Relative(.3f), SizeValue.Relative(.6f));
        Content = _mainLayout;
        Caption = localizations.DialogSearchCaption;
    }
}