using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Modals;
using ImGui.Forms.Models;
using Kaligraphy.Enums.Layout;
using UI.Layton1Tool.Dialogs.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Dialogs;

partial class FontPreviewSettingsDialog : Modal
{
    private CheckBox _debugBoxCheck;
    private TextBox _spacingTextBox;
    private TextBox _lineHeightBox;
    private ComboBox<HorizontalTextAlignment> _alignmentComboBox;

    private void InitializeComponent(FontPreviewSettings settings, ILocalizationProvider localizations)
    {
        _debugBoxCheck = new CheckBox
        {
            Checked = settings.ShowDebugBoxes
        };
        _spacingTextBox = new TextBox
        {
            Width = SizeValue.Absolute(100),
            AllowedCharacters = CharacterRestriction.Decimal,
            Text = $"{settings.Spacing}"
        };
        _lineHeightBox = new TextBox
        {
            Width = SizeValue.Absolute(100),
            AllowedCharacters = CharacterRestriction.Decimal,
            Text = $"{settings.LineHeight}"
        };
        _alignmentComboBox = new ComboBox<HorizontalTextAlignment>
        {
            MaxShowItems = 3
        };

        var mainLayout = new TableLayout
        {
            Size = Size.Content,
            Spacing = new(4, 4),
            Rows =
            {
                new TableRow
                {
                    Cells =
                    {
                        new Label(localizations.FontPreviewSettingsShowDebug),
                        _debugBoxCheck
                    }
                },
                new TableRow
                {
                    Cells =
                    {
                        new Label(localizations.FontPreviewSettingsSpacing),
                        _spacingTextBox
                    }
                },
                new TableRow
                {
                    Cells =
                    {
                        new Label(localizations.FontPreviewSettingsLineHeight),
                        _lineHeightBox
                    }
                },
                new TableRow
                {
                    Cells =
                    {
                        new Label(localizations.FontPreviewSettingsAlignment),
                        _alignmentComboBox
                    }
                }
            }
        };

        Content = mainLayout;
        Size = Size.Content;

        Caption = localizations.FontPreviewSettingsCaption;

        InitializeAlignment(settings, localizations);
    }

    private void InitializeAlignment(FontPreviewSettings settings, ILocalizationProvider localizations)
    {
        _alignmentComboBox.Items.Add(new DropDownItem<HorizontalTextAlignment>(HorizontalTextAlignment.Left, localizations.FontPreviewSettingsAlignmentLeft));
        _alignmentComboBox.Items.Add(new DropDownItem<HorizontalTextAlignment>(HorizontalTextAlignment.Center, localizations.FontPreviewSettingsAlignmentCenter));
        _alignmentComboBox.Items.Add(new DropDownItem<HorizontalTextAlignment>(HorizontalTextAlignment.Right, localizations.FontPreviewSettingsAlignmentRight));

        _alignmentComboBox.SelectedItem = _alignmentComboBox.Items.FirstOrDefault(settings.HorizontalAlignment);
    }
}