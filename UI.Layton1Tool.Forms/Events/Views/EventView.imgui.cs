using System.Numerics;
using Hexa.NET.ImGui;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Extensions;
using ImGui.Forms.Models;
using ImGui.Forms.Resources;
using ImGui.Forms.Support;
using UI.Layton1Tool.Components.Contract.DataClasses;

namespace UI.Layton1Tool.Forms.Events.Views;

internal partial class EventView : ZoomableComponent
{
    public override Size GetSize() => Size.Parent;

    protected override void DrawInternal(Rectangle contentRect)
    {
        ImDrawListPtr drawList = Hexa.NET.ImGui.ImGui.GetWindowDrawList();

        var centerPos = new Vector2(-128, -192);

        if (_bottomBg is not null)
        {
            Rectangle roomArea = Transform(contentRect, new Rectangle(centerPos + new Vector2(0, 192), _bottomBg.Size));

            drawList.AddImage(_bottomBg.GetTextureRef(), roomArea.Position, roomArea.Position + roomArea.Size);
        }

        if (_topBg is not null)
        {
            Rectangle roomArea = Transform(contentRect, new Rectangle(centerPos, _topBg.Size));

            drawList.AddImage(_topBg.GetTextureRef(), roomArea.Position, roomArea.Position + roomArea.Size);
        }

        if (_bottomColor is not null)
        {
            Rectangle colorArea = Transform(contentRect, new Rectangle(centerPos + new Vector2(0, 192), new Vector2(256, 192)));

            drawList.AddRectFilled(colorArea.Position, colorArea.Position + colorArea.Size, _bottomColor.Value.ToUInt32());
        }

        foreach ((int x, int y, AnimationState animationState) in _personObjects)
        {
            if (animationState.ActiveAnimation is null)
                continue;

            ImageResource frame = animationState.Images[animationState.ActiveAnimation.Steps[animationState.StepCounter].FrameIndex];
            Rectangle animationArea = Transform(contentRect, new Rectangle(centerPos + new Vector2(x, y + 192), frame.Size));

            drawList.AddImage(frame.GetTextureRef(), animationArea.Position, animationArea.Position + animationArea.Size);

            _animationManager.Step(animationState);
        }

        if (_eventWindowAnimation?.ActiveAnimation is not null)
        {
            ImageResource frame = _eventWindowAnimation.Images[_eventWindowAnimation.ActiveAnimation.Steps[_eventWindowAnimation.StepCounter].FrameIndex];
            Rectangle animationArea = Transform(contentRect, new Rectangle(centerPos + new Vector2(2, 192 + 127), frame.Size));

            drawList.AddImage(frame.GetTextureRef(), animationArea.Position, animationArea.Position + animationArea.Size);

            _animationManager.Step(_eventWindowAnimation);
        }

        if (_eventCursorAnimation?.ActiveAnimation is not null)
        {
            ImageResource frame = _eventCursorAnimation.Images[_eventCursorAnimation.ActiveAnimation.Steps[_eventCursorAnimation.StepCounter].FrameIndex];
            Rectangle animationArea = Transform(contentRect, new Rectangle(centerPos + new Vector2(236, 192 + 174), frame.Size));

            drawList.AddImage(frame.GetTextureRef(), animationArea.Position, animationArea.Position + animationArea.Size);

            _animationManager.Step(_eventCursorAnimation);
        }

        if (_textImage is not null)
        {
            Rectangle textArea = Transform(contentRect, new Rectangle(centerPos + new Vector2(0, 192), _textImage.Size));

            drawList.AddImage(_textImage.GetTextureRef(), textArea.Position, textArea.Position + textArea.Size);
        }
    }
}