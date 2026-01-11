using System.Numerics;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Extensions;
using ImGui.Forms.Models;
using ImGui.Forms.Resources;
using ImGuiNET;
using Veldrid;

namespace UI.Layton1Tool.Forms.Events.Views;

internal partial class EventView : ZoomableComponent
{
    public override Size GetSize() => Size.Parent;

    protected override void DrawInternal(Rectangle contentRect)
    {
        ImDrawListPtr drawList = ImGuiNET.ImGui.GetWindowDrawList();

        var centerPos = new Vector2(-128, -192);

        if (_bottomBg is not null)
        {
            Rectangle roomArea = Transform(contentRect, new Rectangle((int)centerPos.X, (int)centerPos.Y + 192, _bottomBg.Width, _bottomBg.Height));

            drawList.AddImage((IntPtr)_bottomBg, roomArea.Position, roomArea.Position + roomArea.Size);
        }

        if (_topBg is not null)
        {
            Rectangle roomArea = Transform(contentRect, new Rectangle((int)centerPos.X, (int)centerPos.Y, _topBg.Width, _topBg.Height));

            drawList.AddImage((IntPtr)_topBg, roomArea.Position, roomArea.Position + roomArea.Size);
        }

        if (_bottomColor is not null)
        {
            Rectangle colorArea = Transform(contentRect, new Rectangle((int)centerPos.X, (int)centerPos.Y + 192, 256, 192));

            drawList.AddRectFilled(colorArea.Position, colorArea.Position + colorArea.Size, _bottomColor.Value.ToUInt32());
        }

        if (_eventWindowAnimation?.ActiveAnimation is not null)
        {
            ImageResource frame = _eventWindowAnimation.Images[_eventWindowAnimation.ActiveAnimation.Steps[_eventWindowAnimation.StepCounter].FrameIndex];
            Rectangle animationArea = Transform(contentRect, new Rectangle((int)centerPos.X + 2, (int)centerPos.Y + 192 + 127, frame.Width, frame.Height));

            drawList.AddImage((IntPtr)frame, animationArea.Position, animationArea.Position + animationArea.Size);

            _animationManager.Step(_eventWindowAnimation);
        }

        if (_eventCursorAnimation?.ActiveAnimation is not null)
        {
            ImageResource frame = _eventCursorAnimation.Images[_eventCursorAnimation.ActiveAnimation.Steps[_eventCursorAnimation.StepCounter].FrameIndex];
            Rectangle animationArea = Transform(contentRect, new Rectangle((int)centerPos.X + 236, (int)centerPos.Y + 192 + 174, frame.Width, frame.Height));

            drawList.AddImage((IntPtr)frame, animationArea.Position, animationArea.Position + animationArea.Size);

            _animationManager.Step(_eventCursorAnimation);
        }

        if (_textImage is not null)
        {
            Rectangle textArea = Transform(contentRect, new Rectangle((int)centerPos.X, (int)centerPos.Y + 192, _textImage.Width, _textImage.Height));

            drawList.AddImage((IntPtr)_textImage, textArea.Position, textArea.Position + textArea.Size);
        }

        //foreach ((int x, int y, AnimationState animationState) in _personObjects)
        //{
        //    if (animationState.ActiveAnimation is null)
        //        continue;

        //    ImageResource frame = animationState.Images[animationState.ActiveAnimation.Steps[animationState.StepCounter].FrameIndex];
        //    Rectangle animationArea = Transform(contentRect, new Rectangle(x + (int)centerPos.X, y + (int)centerPos.Y + 192, frame.Width, frame.Height));

        //    drawList.AddImage((IntPtr)frame, animationArea.Position, animationArea.Position + animationArea.Size);

        //    _animationManager.Step(animationState);
        //}
    }
}