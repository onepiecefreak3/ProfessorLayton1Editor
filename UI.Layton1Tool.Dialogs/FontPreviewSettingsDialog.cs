using UI.Layton1Tool.Dialogs.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Dialogs;

partial class FontPreviewSettingsDialog
{
    private readonly FontPreviewSettings _settings;

    public FontPreviewSettingsDialog(FontPreviewSettings settings, ILocalizationProvider localizations)
    {
        InitializeComponent(settings, localizations);

        _settings = settings;

        _debugBoxCheck!.CheckChanged += _debugBoxCheck_CheckChanged;
        _spacingTextBox!.TextChanged += SpacingTextBoxTextChanged;
        _lineHeightBox!.TextChanged += _lineHeightBox_TextChanged;
        _alignmentComboBox!.SelectedItemChanged += _alignmentComboBox_SelectedItemChanged;
    }

    private void _lineHeightBox_TextChanged(object? sender, EventArgs e)
    {
        if (!int.TryParse(_lineHeightBox.Text, out int lineHeight))
            return;

        _settings.LineHeight = lineHeight;
    }

    private void _alignmentComboBox_SelectedItemChanged(object? sender, EventArgs e)
    {
        _settings.HorizontalAlignment = _alignmentComboBox.SelectedItem.Content;
    }

    private void SpacingTextBoxTextChanged(object? sender, EventArgs e)
    {
        if (!int.TryParse(_spacingTextBox.Text, out int spacing))
            return;

        _settings.Spacing = spacing;
    }

    private void _debugBoxCheck_CheckChanged(object? sender, EventArgs e)
    {
        _settings.ShowDebugBoxes = _debugBoxCheck.Checked;
    }
}