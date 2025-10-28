using ImGui.Forms;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Models;
using ImGui.Forms.Resources;
using ImGuiNET;
using Konnect.Contract.DataClasses.Plugin.File.Font;
using System.Numerics;
using ImGui.Forms.Extensions;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Components;

partial class GlyphElement : Component
{
    private static readonly Vector2 GlyphMaxSize = new(36, 36);

    private ThemedImageResource? _glyph;
    private FontResource _mainFont;
    private FontResource _codeFont;

    private bool IsSelected { get; set; }

    public override Size GetSize() => Size.Content;

    private void InitializeComponent(CharacterInfo charInfo, IFontFactory fonts)
    {
        _glyph = charInfo.Glyph is not null ? ImageResource.FromImage(charInfo.Glyph) : (ThemedImageResource?)null;
        _mainFont = fonts.GetApplicationFont(15);
        _codeFont = fonts.GetHexadecimalFont(11);
    }

    protected override void UpdateInternal(Rectangle contentRect)
    {
        // Draw selection
        bool isSelected = IsSelected;
        ImGuiNET.ImGui.Selectable($"##{Id}", ref isSelected, ImGuiSelectableFlags.None, contentRect.Size);

        if (IsSelected != isSelected)
        {
            IsSelected = isSelected;
            RaiseSelectedGlyphElementChanged();
        }

        // Draw glyph
        if (_glyph != null)
        {
            var imageSize = new Vector2(Math.Min(GlyphMaxSize.X, _glyph.Width), Math.Min(GlyphMaxSize.Y, _glyph.Height));
            var imgPosition = contentRect.Position + (GlyphMaxSize - imageSize) / 2;

            ImGuiNET.ImGui.SetCursorScreenPos(imgPosition);
            ImGuiNET.ImGui.Image((nint)_glyph, imageSize);
        }

        // Draw character and code
        var character = $"{_charInfo.CodePoint}";
        var codeUpper = $"{(_charInfo.CodePoint >> 8) & 0xFF:X2}";
        var codeLower = $"{_charInfo.CodePoint & 0xFF:X2}";

        var characterSize = new Vector2(_mainFont.GetLineWidth(character), _mainFont.GetLineHeight());
        var codeUpperSize = new Vector2(_codeFont.GetLineWidth(codeUpper), _codeFont.GetLineHeight());
        var codeLowerSize = new Vector2(_codeFont.GetLineWidth(codeLower), _codeFont.GetLineHeight());

        var textWidth = characterSize.X + Math.Max(codeUpperSize.X, codeLowerSize.X) + 5;
        var textHeight = Math.Max(characterSize.Y, codeUpperSize.Y + codeLowerSize.Y);

        var characterX = (GlyphMaxSize.X - textWidth) / 2;
        var characterPosition = contentRect.Position + new Vector2(characterX, GlyphMaxSize.Y + (textHeight - characterSize.Y) / 2);
        var codeUpperPosition = contentRect.Position + new Vector2(characterX + characterSize.X + 5, GlyphMaxSize.Y);
        var codeLowerPosition = contentRect.Position + new Vector2(characterX + characterSize.X + 5, GlyphMaxSize.Y + codeUpperSize.Y);

        ImGuiNET.ImGui.SetCursorScreenPos(characterPosition);
        ImGuiNET.ImGui.Text(character);

        if (_codeFont.GetPointer().HasValue)
            ImGuiNET.ImGui.PushFont(_codeFont.GetPointer()!.Value);

        ImGuiNET.ImGui.SetCursorScreenPos(codeUpperPosition);
        ImGuiNET.ImGui.Text(codeUpper);

        ImGuiNET.ImGui.SetCursorScreenPos(codeLowerPosition);
        ImGuiNET.ImGui.Text(codeLower);

        if (_codeFont.GetPointer().HasValue)
            ImGuiNET.ImGui.PopFont();

        // Draw border
        ImGuiNET.ImGui.GetWindowDrawList().AddRect(contentRect.Position, contentRect.Position + contentRect.Size, Style.GetColor(ImGuiCol.Border).ToUInt32());
    }

    protected override int GetContentWidth(int parentWidth, int parentHeight, float layoutCorrection = 1)
    {
        return (int)GlyphMaxSize.X;
    }

    protected override int GetContentHeight(int parentWidth, int parentHeight, float layoutCorrection = 1)
    {
        float textHeight = Math.Max(_mainFont.GetLineHeight(), _codeFont.GetLineHeight() + 1);
        return (int)(GlyphMaxSize.Y + 12 + textHeight);
    }
}