using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Models;
using ImGuiNET;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using Veldrid;

namespace UI.Layton1Tool.Forms.Events.Views;

internal partial class EventTextControl : Component
{
    private Component _eventView;
    private ArrowButton _prevButton;
    private ArrowButton _nextButton;
    private StackLayout _mainLayout;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent(Layton1NdsInfo ndsInfo, IFormFactory forms)
    {
        _eventView = forms.CreateEventView(ndsInfo);

        _prevButton = new ArrowButton(ImGuiDir.Left) { Enabled = false };
        _nextButton = new ArrowButton(ImGuiDir.Right) { Enabled = false };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            ItemSpacing = 5,
            Size = Size.Parent,
            Items =
            {
                new StackLayout
                {
                    Alignment = Alignment.Horizontal,
                    ItemSpacing = 5,
                    Size = Size.WidthAlign,
                    Items =
                    {
                        _prevButton,
                        _nextButton
                    }
                },
                _eventView
            }
        };
    }
}