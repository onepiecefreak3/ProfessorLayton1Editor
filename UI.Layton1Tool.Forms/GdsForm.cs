using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Text.Editor;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.Contract.Enums;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

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

    public GdsForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1ScriptInstructionManager instructionManager,
        ILevel5ScriptComposer scriptComposer, ILevel5ScriptParser scriptParser, ILocalizationProvider localizations, IPositionManager positionManager)
    {
        InitializeComponent(localizations);

        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;
        _instructionManager = instructionManager;
        _scriptComposer = scriptComposer;
        _scriptParser = scriptParser;
        _positionManager = positionManager;

        _scriptEditor!.CursorPositionChanged += _scriptEditor_CursorPositionChanged;
        _scriptEditor.TextChanged += _scriptEditor_TextChanged;

        eventBroker.Subscribe<SelectedGdsChangedMessage>(UpdateScript);
    }

    private void _scriptEditor_TextChanged(object? sender, string e)
    {
        try
        {
            _scriptSyntax = _scriptParser.ParseCodeUnit(e);
            UpdateSelectedMethodInvocation(_scriptEditor.GetCursorPosition());

            _eventBroker.Raise(new GdsModifiedMessage(this, _scriptSyntax));
        }
        catch (Exception)
        {
            _scriptSyntax = null;
        }
    }

    private void _scriptEditor_CursorPositionChanged(object? sender, Coordinate e)
    {
        UpdateSelectedMethodInvocation(e);
    }

    private void UpdateScript(SelectedGdsChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        _scriptSyntax = message.Script;

        string script = _scriptComposer.ComposeCodeUnit(message.Script);

        Coordinate currentCursor = _scriptEditor.GetCursorPosition();
        _scriptEditor.SetText(script);

        if (currentCursor != _scriptEditor.GetCursorPosition())
            UpdateSelectedMethodInvocation(currentCursor);
    }

    private void UpdateSelectedMethodInvocation(Coordinate coordinate)
    {
        MethodInvocationStatementSyntax? selectedInvocation = GetSelectedMethodInvocation(coordinate);

        if (selectedInvocation == _selectedInvocation)
            return;

        UpdateInstructionInfo(selectedInvocation);

        _selectedInvocation = selectedInvocation;
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

        MethodInvocationStatementSyntax? result = null;

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

            result = invocation;
            break;
        }

        return result;
    }

    private void UpdateInstructionInfo(MethodInvocationStatementSyntax? invocation)
    {
        if (invocation is null)
        {
            ToggleInstructionInfo(false);
            return;
        }

        Layton1ScriptInstruction? description = _instructionManager.GetInstruction(invocation);

        if (description is null)
        {
            ToggleInstructionInfo(false);
            return;
        }

        _instructionText.Text = string.IsNullOrEmpty(description.Name) ? invocation.Identifier.Text : description.Name;

        ToggleInstructionInfo(true);
    }
}