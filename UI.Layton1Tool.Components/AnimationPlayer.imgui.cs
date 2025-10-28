using System.Numerics;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Models;
using ImGui.Forms.Models.IO;
using ImGuiNET;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using UI.Layton1Tool.Components.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Components;

partial class AnimationPlayer : Component
{
    private StackLayout _controlLayout;

    private TextBox _speedInput;
    private ImageButton _stepBackButton;
    private ImageButton _frameBackButton;
    private ImageButton _playButton;
    private ImageButton _frameForwardButton;
    private ImageButton _stepForwardButton;
    private Label _frameCounterLabel;

    private int _previousStepCounter = -1;

    public override Size GetSize() => Size.WidthAlign;

    private void InitializeComponent(ILocalizationProvider localizations, IImageProvider images)
    {
        _speedInput = new TextBox { Placeholder = localizations.AnimationSpeedInputCaption, Enabled = true };
        _stepBackButton = new ImageButton(images.StepBack) { Enabled = false, Padding = new Vector2(4, 4), KeyAction = new KeyCommand(ModifierKeys.Shift, Key.Left) };
        _frameBackButton = new ImageButton(images.FrameBack) { Enabled = false, Padding = new Vector2(4, 4), KeyAction = new KeyCommand(Key.Left) };
        _playButton = new ImageButton(images.Pause) { Padding = new Vector2(10, 10), KeyAction = new KeyCommand(Key.Space) };
        _frameForwardButton = new ImageButton(images.FrameForward) { Enabled = false, Padding = new Vector2(4, 4), KeyAction = new KeyCommand(Key.Right) };
        _stepForwardButton = new ImageButton(images.StepForward) { Enabled = false, Padding = new Vector2(4, 4), KeyAction = new KeyCommand(ModifierKeys.Shift, Key.Right) };
        _frameCounterLabel = new Label();

        _controlLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.WidthAlign,
            ItemSpacing = 5,
            Items =
            {
                new StackItem(_speedInput) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center },
                new StackItem(_stepBackButton) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center },
                new StackItem(_frameBackButton) { VerticalAlignment = VerticalAlignment.Center },
                _playButton,
                new StackItem(_frameForwardButton) { VerticalAlignment = VerticalAlignment.Center },
                new StackItem(_stepForwardButton) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center },
                new StackItem(_frameCounterLabel) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center }
            }
        };
    }

    protected override void UpdateInternal(Rectangle contentRect)
    {
        if (!TryGetAnimationState(out AnimationState? animationState))
            return;

        int controlWidth = _controlLayout.GetWidth(contentRect.Width - 2, contentRect.Height);
        int controlHeight = _controlLayout.GetHeight(contentRect.Width, contentRect.Height);

        ImGuiNET.ImGui.SetCursorPos(ImGuiNET.ImGui.GetCursorPos() + new Vector2(5, 26));
        _controlLayout.Update(new Rectangle(contentRect.X, contentRect.Y + 26, controlWidth, controlHeight));

        RenderPlayer(contentRect, animationState);

        if (animationState.StepCounter != _previousStepCounter)
            RaiseAnimationFrameChanged(animationState);

        _previousStepCounter = animationState.StepCounter;

        if (!_isPlaying)
            return;

        float frameSpeed = animationState.FrameSpeed;
        while (frameSpeed > 0)
            frameSpeed -= Step(animationState, frameSpeed);
    }

    protected override int GetContentHeight(int parentWidth, int parentHeight, float layoutCorrection = 1)
    {
        return 5 + 16 + 5 + _controlLayout.GetHeight(parentWidth, parentHeight, layoutCorrection) + 5;
    }

    private void RenderPlayer(Rectangle contentRect, AnimationState animationState)
    {
        if (animationState.ActiveAnimation is null)
            return;

        int totalFrames = animationState.ActiveAnimation.Steps.Sum(s => s.FrameCounter);

        float x = contentRect.Position.X + 5;
        float y = contentRect.Position.Y + 5;
        int barWidth = contentRect.Width - 10;

        float frameDistance = barWidth / (float)Math.Max(totalFrames - 1, 1);
        float cursorDistance = (int)animationState.TotalFrameCounter * frameDistance;

        var barPosition = new Vector2(x, y);

        DrawBar(barPosition, barWidth);
        DrawBarMarkers(barPosition, frameDistance, totalFrames, animationState.ActiveAnimation.Steps);
        DrawCursor(barPosition, cursorDistance);

        _frameCounterLabel.Text = $"{(int)animationState.TotalFrameCounter + 1}/{totalFrames}";
    }

    private static void DrawBar(Vector2 barPosition, float barWidth)
    {
        barPosition += new Vector2(0, 3);
        Vector2 barEndPosition = barPosition + new Vector2(barWidth, 4);

        ImGuiNET.ImGui.GetWindowDrawList().AddRectFilled(barPosition, barEndPosition, ImGuiNET.ImGui.GetColorU32(ImGuiCol.ScrollbarBg));
    }

    private static void DrawBarMarkers(Vector2 barPosition, float markerStep, int count, AnimationStep[] steps)
    {
        uint frameColor = ImGuiNET.ImGui.GetColorU32(ImGuiCol.Button);
        var stepColor = 0xFFBF40BFu;

        Vector2 markerPos = barPosition + new Vector2(0, 7);
        Vector2 markerEndPos = markerPos + new Vector2(0, 9);

        var frameCounter = 0;
        var stepIndex = 0;

        int markerCount = Math.Max(2, count);
        for (var i = 0; i < markerCount; i++)
        {
            if (steps[stepIndex].FrameCounter == frameCounter)
            {
                frameCounter = 1;
                stepIndex++;
            }
            else
            {
                frameCounter++;
            }

            uint markerColor = frameCounter == 1 ? stepColor : frameColor;

            var deltaPos = new Vector2(i * markerStep, 0);
            float thickness = 2;

            if (i == 0 || i + 1 == markerCount)
            {
                thickness = 1;

                if (i + 1 == markerCount)
                    deltaPos -= new Vector2(1, 0);
            }

            ImGuiNET.ImGui.GetWindowDrawList().AddLine(markerPos + deltaPos, markerEndPos + deltaPos, markerColor, thickness);
        }

        if (count == 1)
        {
            var deltaPos = new Vector2(markerStep - 1, 0);
            ImGuiNET.ImGui.GetWindowDrawList().AddLine(markerPos + deltaPos, markerEndPos + deltaPos, frameColor, 1);
        }
    }

    private static void DrawCursor(Vector2 barPosition, float cursorX)
    {
        uint cursorColor = ImGuiNET.ImGui.GetColorU32(ImGuiCol.Button) | 0xFF000000;

        Vector2 cursorPos = barPosition + new Vector2(cursorX - 5, 0);
        Vector2 cursorEndPos = cursorPos + new Vector2(10);

        ImGuiNET.ImGui.GetWindowDrawList().AddRectFilled(cursorPos, cursorEndPos, cursorColor);

        Vector2 cursorTriPos = cursorPos + new Vector2(0, 10);
        Vector2 cursorTriMidPos = cursorPos + new Vector2(5, 13);

        ImGuiNET.ImGui.GetWindowDrawList().AddTriangleFilled(cursorTriPos, cursorEndPos, cursorTriMidPos, cursorColor);
    }
}