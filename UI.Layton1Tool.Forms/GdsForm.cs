using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Text.Editor;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.Contract.Enums;
using UI.Layton1Tool.Messages;

namespace UI.Layton1Tool.Forms;

partial class GdsForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1ScriptInstructionManager _instructionManager;
    private readonly ILevel5ScriptComposer _scriptComposer;
    private readonly ILevel5ScriptParser _scriptParser;
    private readonly IPositionManager _positionManager;

    private CodeUnitSyntax? _scriptSyntax;

    private MethodInvocationStatementSyntax? _selectedInvocation;
    private int _selectedParameterIndex = -1;

    public GdsForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1ScriptInstructionManager instructionManager,
        ILevel5ScriptComposer scriptComposer, ILevel5ScriptParser scriptParser, IPositionManager positionManager)
    {
        InitializeComponent();

        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;
        _instructionManager = instructionManager;
        _scriptComposer = scriptComposer;
        _scriptParser = scriptParser;
        _positionManager = positionManager;

        _scriptEditor!.CursorPositionChanged += _scriptEditor_CursorPositionChanged;
        _scriptEditor.TextChanged += _scriptEditor_TextChanged;

        _prevParameterButton!.Clicked += _prevParameterButton_Clicked;
        _nextParameterButton!.Clicked += _nextParameterButton_Clicked;

        eventBroker.Subscribe<SelectedGdsChangedMessage>(UpdateScript);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<SelectedGdsChangedMessage>(UpdateScript);
    }

    private void _nextParameterButton_Clicked(object? sender, EventArgs e)
    {
        if (_selectedInvocation?.Parameters.ParameterList is null || _selectedParameterIndex < 0)
            return;

        _selectedParameterIndex = Math.Min(_selectedInvocation.Parameters.ParameterList.Elements.Count - 1, _selectedParameterIndex + 1);

        UpdateSelectedInstruction(_selectedInvocation, _selectedParameterIndex);
        UpdateParameterButtons();
    }

    private void _prevParameterButton_Clicked(object? sender, EventArgs e)
    {
        if (_selectedInvocation is null || _selectedParameterIndex < 0)
            return;

        _selectedParameterIndex = Math.Max(0, _selectedParameterIndex - 1);

        UpdateSelectedInstruction(_selectedInvocation, _selectedParameterIndex);
        UpdateParameterButtons();
    }

    private void _scriptEditor_TextChanged(object? sender, string e)
    {
        try
        {
            _scriptSyntax = _scriptParser.ParseCodeUnit(e);
            UpdateSelectedInstruction(_scriptEditor.GetCursorPosition());

            _eventBroker.Raise(new GdsModifiedMessage(this, _scriptSyntax));
        }
        catch (Exception)
        {
            _scriptSyntax = null;
        }
    }

    private void _scriptEditor_CursorPositionChanged(object? sender, Coordinate e)
    {
        UpdateSelectedInstruction(e);
    }

    private void UpdateScript(SelectedGdsChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        _scriptSyntax = message.Script;

        string script = _scriptComposer.ComposeCodeUnit(message.Script);

        Coordinate currentCursor = _scriptEditor.GetCursorPosition();
        _scriptEditor.SetText(script);

        UpdateSelectedInstruction(currentCursor);
    }

    private void UpdateSelectedInstruction(Coordinate coordinate)
    {
        var hasInstructionInfoChanged = false;

        MethodInvocationStatementSyntax? selectedInvocation = GetSelectedMethodInvocation(coordinate);

        if (_selectedInvocation != selectedInvocation)
        {
            _selectedInvocation = selectedInvocation;
            hasInstructionInfoChanged = true;
        }

        int selectedParameterIndex = selectedInvocation is null ? -1 : GetSelectedParameterIndex(selectedInvocation, coordinate);

        if (_selectedParameterIndex != selectedParameterIndex)
        {
            _selectedParameterIndex = selectedParameterIndex;
            hasInstructionInfoChanged = true;
        }

        if (hasInstructionInfoChanged)
            UpdateSelectedInstruction(selectedInvocation, selectedParameterIndex);
    }

    private MethodInvocationStatementSyntax? GetSelectedMethodInvocation(Coordinate coordinate)
    {
        if (_scriptSyntax is null)
            return null;

        MethodDeclarationBodySyntax body = _scriptSyntax.MethodDeclarations[0].Body;

        IReadOnlyList<StatementSyntax> statements = body.Expressions;
        if (statements.Count <= 0)
            return null;

        if (_positionManager.Compare(coordinate, statements[0].Location, PositionComparison.SmallerThan))
            return null;

        foreach (StatementSyntax statement in statements)
        {
            if (statement is not MethodInvocationStatementSyntax invocation)
                continue;

            Coordinate startCoordinate = _scriptEditor.GetCharacterCoordinates(invocation.Span.Position);
            if (_positionManager.Compare(coordinate, startCoordinate, PositionComparison.SmallerThan))
                continue;

            Coordinate endCoordinate = _scriptEditor.GetCharacterCoordinates(invocation.Span.EndPosition);
            if (_positionManager.Compare(coordinate, endCoordinate, PositionComparison.GreaterThan))
                continue;

            return invocation;
        }

        return null;
    }

    private int GetSelectedParameterIndex(MethodInvocationStatementSyntax invocation, Coordinate coordinate)
    {
        if (invocation.Parameters.ParameterList is null)
            return 0;

        if (_positionManager.Compare(coordinate, invocation.Parameters.Location, PositionComparison.SmallerThan))
            return 0;

        for (var i = 0; i < invocation.Parameters.ParameterList.Elements.Count; i++)
        {
            ValueExpressionSyntax parameter = invocation.Parameters.ParameterList.Elements[i];

            Coordinate startCoordinate = _scriptEditor.GetCharacterCoordinates(parameter.Span.Position);
            if (_positionManager.Compare(coordinate, startCoordinate, PositionComparison.SmallerThan))
                continue;

            Coordinate endCoordinate = _scriptEditor.GetCharacterCoordinates(parameter.Span.EndPosition);
            if (_positionManager.Compare(coordinate, endCoordinate, PositionComparison.GreaterThan))
                continue;

            return i;
        }

        return 0;
    }

    private void UpdateSelectedInstruction(MethodInvocationStatementSyntax? invocation, int parameterIndex)
    {
        if (invocation is null)
        {
            ToggleInstructionInfo(false);
            return;
        }

        Layton1ScriptInstruction? description = _instructionManager.GetInstruction(invocation, _ndsInfo.Rom.GameCode);

        if (description is null)
        {
            ToggleInstructionInfo(false);
            return;
        }

        // TODO: Add placeholder for no info

        _instructionNameLabel.Text = string.IsNullOrEmpty(description.Name) ? invocation.Identifier.Text : description.Name;
        _instructionDescriptionLabel.Text = description.Description;

        if (parameterIndex >= 0 && parameterIndex < description.Parameters.Length)
        {
            Layton1ScriptInstructionParameter parameter = description.Parameters[parameterIndex];

            _parameterNameLabel.Text = parameter.Name;
            _parameterDescriptionLabel.Text = parameter.Description;
        }
        else
        {
            _parameterNameLabel.Text = string.Empty;
            _parameterDescriptionLabel.Text = string.Empty;
        }

        UpdateParameterButtons();
        ToggleInstructionInfo(true);
    }

    private void UpdateParameterButtons()
    {
        _prevParameterButton.Enabled = _selectedParameterIndex > 0;
        _nextParameterButton.Enabled = _selectedParameterIndex + 1 < (_selectedInvocation?.Parameters.ParameterList?.Elements.Count ?? 0);
    }
}