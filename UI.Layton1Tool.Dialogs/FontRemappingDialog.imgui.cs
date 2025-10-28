using System.Numerics;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Modals;
using ImGui.Forms.Models;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Dialogs;

partial class FontRemappingDialog : Modal
{
    private StackLayout _mainLayout;
    private UniformZLayout _glyphsLayout;
    private Button _remapButton;

    private void InitializeComponent(ILocalizationProvider localizations)
    {
        _glyphsLayout = new UniformZLayout(new Vector2(36, 61))
        {
            ItemSpacing = new Vector2(4, 4),
            Size = Size.Parent
        };

        _remapButton = new Button(localizations.DialogFontRemappingRemap)
        {
            Width = 75,
            Enabled = false
        };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            Size = Size.Parent,
            ItemSpacing = 4,
            Items =
            {
                new Label(localizations.DialogFontRemappingText),
                _glyphsLayout,
                new StackItem(_remapButton) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Right }
            }
        };

        Size = new Size(SizeValue.Relative(.7f), SizeValue.Relative(.8f));
        Caption = localizations.DialogFontRemappingCaption;
        Content = _mainLayout;
    }
}