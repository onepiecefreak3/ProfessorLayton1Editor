using ImGui.Forms.Controls.Base;
using ImGui.Forms.Resources;
using System.Numerics;
using ImGui.Forms.Extensions;
using Konnect.Contract.DataClasses.Plugin.File.Font;
using SixLabors.ImageSharp;
using Rectangle = Veldrid.Rectangle;
using Size = ImGui.Forms.Models.Size;

namespace UI.Layton1Tool.Components;

partial class ZoomableCharacterInfo : ZoomableComponent
{
    private ImageResource? _glyphResource;

    public override Size GetSize() => Size.Parent;

    protected override void DrawInternal(Rectangle contentRect)
    {
        if (!TryGetCharacterInfo(out CharacterInfo? charInfo))
            return;

        int boundingX = Math.Min(charInfo.GlyphPosition.X, 0);
        int boundingY = Math.Min(charInfo.GlyphPosition.Y, 0);
        int boundingWidth = Math.Max(charInfo.GlyphPosition.X, 0) + Math.Max(charInfo.Glyph?.Width ?? 0, charInfo.BoundingBox.Width);
        int boundingHeight = Math.Max(charInfo.GlyphPosition.Y, 0) + Math.Max(charInfo.Glyph?.Height ?? 0, charInfo.BoundingBox.Height);

        var totalBoundingBox = new Rectangle(boundingX, boundingY, boundingWidth - boundingX, boundingHeight - boundingY);

        DrawBackground(contentRect);

        DrawGlyph(charInfo, contentRect, totalBoundingBox);
        DrawBoundingBox(charInfo, contentRect, totalBoundingBox);
        DrawTotalBoundingBox(contentRect, totalBoundingBox);
    }

    private static void DrawBackground(Rectangle contentRect)
    {
        ImGuiNET.ImGui.GetWindowDrawList().AddRect(contentRect.Position, contentRect.Position + contentRect.Size, Color.Transparent.ToUInt32());
    }

    private void DrawGlyph(CharacterInfo charInfo, Rectangle contentRect, Rectangle totalBoundingBox)
    {
        if (_glyphResource is null)
            return;

        Vector2 boundingStartPosition = -(new Vector2(totalBoundingBox.Width, totalBoundingBox.Height) / 2) + new Vector2(Math.Max(charInfo.GlyphPosition.X, 0), Math.Max(charInfo.GlyphPosition.Y, 0));
        var imageRect = new Rectangle((int)boundingStartPosition.X, (int)boundingStartPosition.Y, _glyphResource.Width, _glyphResource.Height);
        imageRect = Transform(contentRect, imageRect);

        ImGuiNET.ImGui.GetWindowDrawList().AddImage((nint)_glyphResource, imageRect.Position, imageRect.Position + imageRect.Size);
    }

    private void DrawBoundingBox(CharacterInfo charInfo, Rectangle contentRect, Rectangle totalBoundingBox)
    {
        Vector2 boundingStartPosition = -(new Vector2(totalBoundingBox.Width, totalBoundingBox.Height) / 2) + new Vector2(Math.Max(charInfo.GlyphPosition.X, 0), Math.Max(charInfo.GlyphPosition.Y, 0));
        var imageRect = new Rectangle((int)boundingStartPosition.X, (int)boundingStartPosition.Y, charInfo.BoundingBox.Width, charInfo.BoundingBox.Height);
        imageRect = Transform(contentRect, imageRect);

        ImGuiNET.ImGui.GetWindowDrawList().AddRect(imageRect.Position, imageRect.Position + imageRect.Size, Color.OrangeRed.ToUInt32());
    }

    private void DrawTotalBoundingBox(Rectangle contentRect, Rectangle totalBoundingBox)
    {
        Vector2 boundingStartPosition = -(new Vector2(totalBoundingBox.Width, totalBoundingBox.Height) / 2);
        var imageRect = new Rectangle((int)boundingStartPosition.X, (int)boundingStartPosition.Y, totalBoundingBox.Width, totalBoundingBox.Height);
        imageRect = Transform(contentRect, imageRect);

        ImGuiNET.ImGui.GetWindowDrawList().AddRect(imageRect.Position, imageRect.Position + imageRect.Size, Color.Gold.ToUInt32());
    }

    private void UpdateGlyphImage(CharacterInfo charInfo)
    {
        _glyphResource = charInfo.Glyph is not null
            ? ImageResource.FromImage(charInfo.Glyph)
            : null;
    }
}