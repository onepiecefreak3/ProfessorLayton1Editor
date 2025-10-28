using ImGui.Forms.Controls.Base;
using System.Numerics;
using ImGui.Forms.Extensions;
using SixLabors.ImageSharp;
using Rectangle = Veldrid.Rectangle;
using ImGui.Forms.Resources;
using UI.Layton1Tool.Messages.DataClasses;

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

        var totalBoundingBox = new Rectangle(boundingX, boundingY, boundingWidth - boundingX, boundingHeight - boundingY);

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
        var imageRect = new Rectangle((int)boundingStartPosition.X, (int)boundingStartPosition.Y, _paddedGlyphResource.Width, _paddedGlyphResource.Height);
        imageRect = Transform(contentRect, imageRect);

        ImGuiNET.ImGui.GetWindowDrawList().AddImage((nint)_paddedGlyphResource, imageRect.Position, imageRect.Position + imageRect.Size);
    }

    private void DrawBoundingBox(PaddedGlyph paddedGlyph, Rectangle contentRect)
    {
        Vector2 boundingStartPosition = -(new Vector2(paddedGlyph.BoundingBox.Width, paddedGlyph.BoundingBox.Height) / 2) + new Vector2(Math.Max(paddedGlyph.GlyphPosition.X, 0), Math.Max(paddedGlyph.GlyphPosition.Y, 0));
        var imageRect = new Rectangle((int)boundingStartPosition.X, (int)boundingStartPosition.Y, paddedGlyph.BoundingBox.Width, paddedGlyph.BoundingBox.Height);
        imageRect = Transform(contentRect, imageRect);

        ImGuiNET.ImGui.GetWindowDrawList().AddRect(imageRect.Position, imageRect.Position + imageRect.Size, Color.OrangeRed.ToUInt32(), 5f);
    }

    private void DrawTotalBoundingBox(Rectangle contentRect, Rectangle totalBoundingBox)
    {
        Vector2 boundingStartPosition = -(new Vector2(totalBoundingBox.Width, totalBoundingBox.Height) / 2);
        var imageRect = new Rectangle((int)boundingStartPosition.X, (int)boundingStartPosition.Y, totalBoundingBox.Width, totalBoundingBox.Height);
        imageRect = Transform(contentRect, imageRect);

        ImGuiNET.ImGui.GetWindowDrawList().AddRect(imageRect.Position, imageRect.Position + imageRect.Size, Color.Gold.ToUInt32(), 5f);
    }

    private void DrawBaseline(Rectangle contentRect, Rectangle totalBoundingBox)
    {
        Vector2 boundingStartPosition = -(new Vector2(totalBoundingBox.Width, totalBoundingBox.Height) / 2);
        var baseLineRect = new Rectangle((int)boundingStartPosition.X, (int)boundingStartPosition.Y + _paddedGlyph!.Baseline, totalBoundingBox.Width, 0);
        var baseLineRectTransformed = Transform(contentRect, baseLineRect);

        ImGuiNET.ImGui.GetWindowDrawList().AddLine(baseLineRectTransformed.Position, baseLineRectTransformed.Position + baseLineRectTransformed.Size,
            Color.Red.ToUInt32(), 3f);
    }

    private void UpdateGlyphImage(PaddedGlyph? paddedGlyph)
    {
        _paddedGlyphResource = paddedGlyph?.Glyph is not null
            ? ImageResource.FromImage(paddedGlyph.Glyph)
            : null;
    }
}