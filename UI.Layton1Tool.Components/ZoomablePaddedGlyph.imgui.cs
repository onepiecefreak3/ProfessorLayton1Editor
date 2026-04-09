using ImGui.Forms.Controls.Base;
using System.Numerics;
using ImGui.Forms.Extensions;
using ImGui.Forms.Resources;
using SixLabors.ImageSharp;
using UI.Layton1Tool.Messages.DataClasses;
using Rectangle = ImGui.Forms.Support.Rectangle;

namespace UI.Layton1Tool.Components;

partial class ZoomablePaddedGlyph : ZoomableComponent
{
    protected override void DrawInternal(Rectangle contentRect)
    {
        if (!TryGetPaddedGlyph(out PaddedGlyph? paddedGlyph))
            return;

        int boundingX = Math.Min(paddedGlyph.GlyphPosition.X, 0);
        int boundingY = Math.Min(paddedGlyph.GlyphPosition.Y, 0);
        int boundingWidth = Math.Max(paddedGlyph.GlyphPosition.X, 0) + Math.Max(paddedGlyph.Glyph?.Width ?? 0, paddedGlyph.BoundingBox.Width);
        int boundingHeight = Math.Max(paddedGlyph.GlyphPosition.Y, 0) + Math.Max(paddedGlyph.Glyph?.Height ?? 0, paddedGlyph.BoundingBox.Height);

        var totalBoundingBox = new Rectangle(new Vector2(boundingX, boundingY), new Vector2(boundingWidth - boundingX, boundingHeight - boundingY));

        DrawGlyph(paddedGlyph, contentRect);
        DrawBoundingBox(paddedGlyph, contentRect);
        DrawTotalBoundingBox(contentRect, totalBoundingBox);

        DrawBaseline(contentRect, totalBoundingBox);
    }

    private void DrawGlyph(PaddedGlyph paddedGlyph, Rectangle contentRect)
    {
        if (_paddedGlyphResource == null)
            return;

        Vector2 boundingStartPosition = -(new Vector2(paddedGlyph.BoundingBox.Width, paddedGlyph.BoundingBox.Height) / 2) + new Vector2(Math.Max(paddedGlyph.GlyphPosition.X, 0), Math.Max(paddedGlyph.GlyphPosition.Y, 0));
        var imageRect = new Rectangle(boundingStartPosition, _paddedGlyphResource.Size);
        imageRect = Transform(contentRect, imageRect);

        Hexa.NET.ImGui.ImGui.GetWindowDrawList().AddImage(_paddedGlyphResource.GetTextureRef(), imageRect.Position, imageRect.Position + imageRect.Size);
    }

    private void DrawBoundingBox(PaddedGlyph paddedGlyph, Rectangle contentRect)
    {
        Vector2 boundingStartPosition = -(new Vector2(paddedGlyph.BoundingBox.Width, paddedGlyph.BoundingBox.Height) / 2) + new Vector2(Math.Max(paddedGlyph.GlyphPosition.X, 0), Math.Max(paddedGlyph.GlyphPosition.Y, 0));
        var imageRect = new Rectangle(boundingStartPosition, new Vector2(paddedGlyph.BoundingBox.Width, paddedGlyph.BoundingBox.Height));
        imageRect = Transform(contentRect, imageRect);

        Hexa.NET.ImGui.ImGui.GetWindowDrawList().AddRect(imageRect.Position, imageRect.Position + imageRect.Size, Color.OrangeRed.ToUInt32(), 5f);
    }

    private void DrawTotalBoundingBox(Rectangle contentRect, Rectangle totalBoundingBox)
    {
        Vector2 boundingStartPosition = -(new Vector2(totalBoundingBox.Width, totalBoundingBox.Height) / 2);
        var imageRect = new Rectangle(boundingStartPosition, totalBoundingBox.Size);
        imageRect = Transform(contentRect, imageRect);

        Hexa.NET.ImGui.ImGui.GetWindowDrawList().AddRect(imageRect.Position, imageRect.Position + imageRect.Size, Color.Gold.ToUInt32(), 5f);
    }

    private void DrawBaseline(Rectangle contentRect, Rectangle totalBoundingBox)
    {
        Vector2 boundingStartPosition = -(new Vector2(totalBoundingBox.Width, totalBoundingBox.Height) / 2);
        var baseLineRect = new Rectangle(boundingStartPosition + new Vector2(0, _paddedGlyph!.Baseline), new Vector2(totalBoundingBox.Width, 0));
        var baseLineRectTransformed = Transform(contentRect, baseLineRect);

        Hexa.NET.ImGui.ImGui.GetWindowDrawList().AddLine(baseLineRectTransformed.Position, baseLineRectTransformed.Position + baseLineRectTransformed.Size,
            Color.Red.ToUInt32(), 3f);
    }

    private void UpdateGlyphImage(PaddedGlyph? paddedGlyph)
    {
        _paddedGlyphResource = paddedGlyph?.Glyph is not null
            ? ImageResource.FromImage(paddedGlyph.Glyph)
            : null;
    }
}