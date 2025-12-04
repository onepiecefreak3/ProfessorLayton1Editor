using System.Numerics;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Models;
using ImGui.Forms.Resources;
using ImGuiNET;
using UI.Layton1Tool.Components.Contract.DataClasses;
using Veldrid;

namespace UI.Layton1Tool.Forms.Rooms.Views;

internal partial class RoomView : ZoomableComponent
{
    public override Size GetSize() => Size.Parent;

    protected override void DrawInternal(Rectangle contentRect)
    {
        ImDrawListPtr drawList = ImGuiNET.ImGui.GetWindowDrawList();

        var centerPos = new Vector2(-128, -192);

        if (_roomBg is not null)
        {
            Rectangle roomArea = Transform(contentRect, new Rectangle((int)centerPos.X, (int)centerPos.Y + 192, _roomBg.Width, _roomBg.Height));

            drawList.AddImage((IntPtr)_roomBg, roomArea.Position, roomArea.Position + roomArea.Size);
        }

        foreach ((int x, int y, AnimationState animationState) in _animationObjects)
        {
            if (animationState.ActiveAnimation is null)
                continue;

            ImageResource frame = animationState.Images[animationState.ActiveAnimation.Steps[animationState.StepCounter].FrameIndex];
            Rectangle animationArea = Transform(contentRect, new Rectangle(x + (int)centerPos.X, y + (int)centerPos.Y + 192, frame.Width, frame.Height));

            drawList.AddImage((IntPtr)frame, animationArea.Position, animationArea.Position + animationArea.Size);

            _animationManager.Step(animationState);
        }

        foreach ((int x, int y, int w, int h, AnimationState eventState) in _eventObjects)
        {
            if (eventState.ActiveAnimation is null)
                continue;

            ImageResource frame = eventState.Images[eventState.ActiveAnimation.Steps[eventState.StepCounter].FrameIndex];

            Rectangle eventArea = Transform(contentRect, new Rectangle(x + (int)centerPos.X, y + (int)centerPos.Y + 192, frame.Width, frame.Height));
            drawList.AddImage((IntPtr)frame, eventArea.Position, eventArea.Position + eventArea.Size);

            if (_settings?.RenderObjectBoxes ?? true)
            {
                Rectangle interactionArea = Transform(contentRect, new Rectangle(x + (int)centerPos.X, y + (int)centerPos.Y + 192, w, h));
                drawList.AddRect(interactionArea.Position, interactionArea.Position + interactionArea.Size, 0xCF0055CC);
            }

            _animationManager.Step(eventState);
        }

        if (_settings?.RenderMovementArrows ?? true)
        {
            foreach ((int x, int y, _, AnimationState arrowState) in _arrowObjects)
            {
                if (arrowState.ActiveAnimation is null)
                    continue;

                ImageResource frame = arrowState.Images[arrowState.ActiveAnimation.Steps[arrowState.StepCounter].FrameIndex];
                Rectangle arrowArea = Transform(contentRect, new Rectangle(x + (int)centerPos.X, y + (int)centerPos.Y + 192, frame.Width, frame.Height));

                drawList.AddImage((IntPtr)frame, arrowArea.Position, arrowArea.Position + arrowArea.Size);

                _animationManager.Step(arrowState);
            }
        }

        if (_settings?.RenderTextBoxes ?? true)
        {
            foreach ((int x, int y, int w, int h) in _textAreas)
            {
                Rectangle textArea = Transform(contentRect, new Rectangle(x + (int)centerPos.X, y + (int)centerPos.Y + 192, w, h));

                drawList.AddRectFilled(textArea.Position, textArea.Position + textArea.Size, 0x3F17BB4C);
                drawList.AddRect(textArea.Position, textArea.Position + textArea.Size, 0xCF17BB4C);
            }
        }

        if (_settings?.RenderHintBoxes ?? true)
        {
            foreach ((int x, int y, int w, int h) in _hintAreas)
            {
                Rectangle hintArea = Transform(contentRect, new Rectangle(x + (int)centerPos.X, y + (int)centerPos.Y + 192, w, h));

                drawList.AddRectFilled(hintArea.Position, hintArea.Position + hintArea.Size, 0x3F2B4BEE);
                drawList.AddRect(hintArea.Position, hintArea.Position + hintArea.Size, 0xCF2B4BEE);
            }
        }

        if (_fullMapBg is not null)
        {
            Rectangle mapArea = Transform(contentRect, new Rectangle((int)centerPos.X, (int)centerPos.Y, _fullMapBg.Width, _fullMapBg.Height));

            drawList.AddImage((IntPtr)_fullMapBg, mapArea.Position, mapArea.Position + mapArea.Size);
        }

        if (_icon.HasValue)
        {
            if (_icon.Value.Item3.ActiveAnimation is null)
                return;

            ImageResource frame = _icon.Value.Item3.Images[_icon.Value.Item3.ActiveAnimation.Steps[_icon.Value.Item3.StepCounter].FrameIndex];
            Rectangle iconArea = Transform(contentRect, new Rectangle((int)centerPos.X + _icon.Value.Item1, (int)centerPos.Y + _icon.Value.Item2, frame.Width, frame.Height));

            drawList.AddImage((IntPtr)frame, iconArea.Position - iconArea.Size / 2, iconArea.Position + iconArea.Size - iconArea.Size / 2);

            _animationManager.Step(_icon.Value.Item3);
        }
    }
}