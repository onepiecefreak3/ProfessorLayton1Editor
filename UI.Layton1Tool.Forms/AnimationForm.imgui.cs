using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Models;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using UI.Layton1Tool.Components.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;
using Rectangle = Veldrid.Rectangle;
using Size = ImGui.Forms.Models.Size;

namespace UI.Layton1Tool.Forms;

partial class AnimationForm : Component
{
    private StackLayout _mainLayout;
    private StackLayout _fileLayout;

    private TreeView<AnimationSequence> _fileTree;
    private Component _animationPlayer;

    private Label _errorLabel;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent(Layton1NdsInfo ndsInfo, ILocalizationProvider localizations, IComponentFactory components)
    {
        _fileTree = new TreeView<AnimationSequence> { Size = new Size(SizeValue.Relative(.3f), SizeValue.Parent) };
        _animationPlayer = components.CreateAnimationViewer(ndsInfo);

        _errorLabel = new Label(localizations.StatusAnimationLoadError);

        _fileLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                _fileTree,
                _animationPlayer
            }
        };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                _fileLayout
            }
        };
    }

    private void ToggleError(bool toggle)
    {
        if (!toggle)
        {
            if (_mainLayout.Items.Count <= 1)
                return;

            _mainLayout.Items.RemoveAt(1);
        }
        else
        {
            var item = new StackItem(_errorLabel) { Size = Size.Content, HorizontalAlignment = HorizontalAlignment.Right };

            if (_mainLayout.Items.Count <= 1)
                _mainLayout.Items.Add(item);
            else
                _mainLayout.Items[1] = item;
        }
    }

    protected override void SetTabInactiveCore()
    {
        _fileTree.SetTabInactive();
    }
}