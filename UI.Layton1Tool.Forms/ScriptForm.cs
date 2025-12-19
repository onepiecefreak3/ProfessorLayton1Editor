using ImGui.Forms.Controls.Text.Editor;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.Contract.Enums;

namespace UI.Layton1Tool.Forms;

internal abstract partial class ScriptForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly ILayton1ScriptInstructionManager _instructionManager;
    private readonly ILayton1ScriptCodeUnitConverter _codeUnitConverter;
    private readonly ILayton1ScriptFileConverter _scriptFileConverter;
    private readonly ILayton1ScriptParser _scriptParser;
    private readonly ILayton1ScriptComposer _scriptComposer;
    private readonly ILayton1ScriptWhitespaceNormalizer _whitespaceNormalizer;
    private readonly IPositionManager _positionManager;

    private MethodInvocationExpressionSyntax? _selectedInvocation;
    private int _selectedParameterIndex = -1;

    private CodeUnitSyntax? _codeUnit;

    protected GdsScriptFile? Script { get; private set; }

    public ScriptForm(Layton1NdsInfo ndsInfo, ILayton1ScriptInstructionManager instructionManager, ILayton1ScriptCodeUnitConverter codeUnitConverter,
        ILayton1ScriptFileConverter scriptFileConverter, ILayton1ScriptParser scriptParser, ILayton1ScriptComposer scriptComposer,
        ILayton1ScriptWhitespaceNormalizer whitespaceNormalizer, IPositionManager positionManager)
    {
        InitializeComponent();

        _ndsInfo = ndsInfo;

        _instructionManager = instructionManager;
        _codeUnitConverter = codeUnitConverter;
        _scriptFileConverter = scriptFileConverter;
        _scriptParser = scriptParser;
        _scriptComposer = scriptComposer;
        _whitespaceNormalizer = whitespaceNormalizer;
        _positionManager = positionManager;

        _scriptEditor!.CursorPositionChanged += _scriptEditor_CursorPositionChanged;
        _scriptEditor.TextChanged += _scriptEditor_TextChanged;
        _prevParameterButton!.Clicked += _prevParameterButton_Clicked;
        _nextParameterButton!.Clicked += _nextParameterButton_Clicked;
    }

    protected virtual void OnScriptChanged() { }

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

    private void _scriptEditor_CursorPositionChanged(object? sender, Coordinate e)
    {
        UpdateSelectedInstruction(e);
    }

    private void _scriptEditor_TextChanged(object? sender, string e)
    {
        try
        {
            _codeUnit = _scriptParser.ParseCodeUnit(e);
            Script = _codeUnitConverter.CreateScriptFile(_codeUnit, _ndsInfo.Rom.GameCode);

            UpdateSelectedInstruction(_scriptEditor.GetCursorPosition());

            OnScriptChanged();
        }
        catch (Exception)
        {
            _codeUnit = null;
            Script = null;
        }
    }

    protected void UpdateScript(GdsScriptFile script)
    {
        Script = script;

        _codeUnit = _scriptFileConverter.CreateCodeUnit(script, _ndsInfo.Rom.GameCode);
        _whitespaceNormalizer.NormalizeCodeUnit(_codeUnit);

        string scriptText = _scriptComposer.ComposeCodeUnit(_codeUnit);

        Coordinate currentCursor = _scriptEditor.GetCursorPosition();
        _scriptEditor.SetText(scriptText);
        _scriptEditor.IsReadOnly = false;

        UpdateSelectedInstruction(currentCursor);
    }

    private void UpdateSelectedInstruction(Coordinate coordinate)
    {
        var hasInstructionInfoChanged = false;

        MethodInvocationExpressionSyntax? selectedInvocation = GetSelectedMethodInvocation(coordinate);

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

    private void UpdateSelectedInstruction(MethodInvocationExpressionSyntax? invocation, int parameterIndex)
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

        _instructionIdLabel.Text = $"{description.Id}";
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

    private MethodInvocationExpressionSyntax? GetSelectedMethodInvocation(Coordinate coordinate)
    {
        return _codeUnit?.MethodDeclaration is null ? null : GetSelectedMethodInvocation(coordinate, _codeUnit.MethodDeclaration.Body.Statements);
    }

    private MethodInvocationExpressionSyntax? GetSelectedMethodInvocation(Coordinate coordinate, IReadOnlyList<StatementSyntax> statements)
    {
        if (statements.Count <= 0)
            return null;

        if (_positionManager.Compare(coordinate, statements[0].Location, PositionComparison.SmallerThan))
            return null;

        foreach (StatementSyntax statement in statements)
        {
            MethodInvocationExpressionSyntax? foundInvocation;
            switch (statement)
            {
                case WhileStatementSyntax whileStatement:
                    foundInvocation = GetSelectedMethodInvocation(coordinate, whileStatement.Expression) ??
                                      GetSelectedMethodInvocation(coordinate, whileStatement.Body.Statements);
                    break;

                case IfStatementSyntax ifStatement:
                    foundInvocation = GetSelectedMethodInvocation(coordinate, ifStatement.Expression) ??
                                      GetSelectedMethodInvocation(coordinate, ifStatement.Body.Statements);
                    break;

                case IfElseStatementSyntax ifElseStatement:
                    foundInvocation = GetSelectedMethodInvocation(coordinate, ifElseStatement.Expression) ??
                                      GetSelectedMethodInvocation(coordinate, ifElseStatement.Body.Statements) ??
                                      GetSelectedMethodInvocation(coordinate, ifElseStatement.Else);
                    break;

                case MethodInvocationStatementSyntax invocation:
                    foundInvocation = IsSelectedMethodInvocation(coordinate, invocation.Invocation) ? invocation.Invocation : null;
                    break;

                default:
                    continue;
            }

            if (foundInvocation is not null)
                return foundInvocation;
        }

        return null;
    }

    private MethodInvocationExpressionSyntax? GetSelectedMethodInvocation(Coordinate coordinate, ElseSyntax elseSyntax)
    {
        return elseSyntax switch
        {
            ElseBodySyntax elseBody => GetSelectedMethodInvocation(coordinate, elseBody.Body.Statements),
            ElseIfBodySyntax elseIfBody => GetSelectedMethodInvocation(coordinate, [elseIfBody.If]),
            _ => null
        };
    }

    private MethodInvocationExpressionSyntax? GetSelectedMethodInvocation(Coordinate coordinate, ExpressionSyntax conditionalExpression)
    {
        if (conditionalExpression is UnaryExpressionSyntax unaryExpression)
            return GetSelectedMethodInvocation(coordinate, unaryExpression.Value);

        if (conditionalExpression is MethodInvocationExpressionSyntax invocation)
            return IsSelectedMethodInvocation(coordinate, invocation) ? invocation : null;

        if (conditionalExpression is LogicalExpressionSyntax logicalExpression)
            return GetSelectedMethodInvocation(coordinate, logicalExpression.Left) ??
                   GetSelectedMethodInvocation(coordinate, logicalExpression.Right);

        return null;
    }

    private bool IsSelectedMethodInvocation(Coordinate coordinate, MethodInvocationExpressionSyntax invocation)
    {
        Coordinate startCoordinate = _scriptEditor.GetCharacterCoordinates(invocation.Span.Position);
        if (_positionManager.Compare(coordinate, startCoordinate, PositionComparison.SmallerThan))
            return false;

        Coordinate endCoordinate = _scriptEditor.GetCharacterCoordinates(invocation.Span.EndPosition);
        if (_positionManager.Compare(coordinate, endCoordinate, PositionComparison.GreaterThan))
            return false;

        return true;
    }

    private int GetSelectedParameterIndex(MethodInvocationExpressionSyntax invocation, Coordinate coordinate)
    {
        if (invocation.Parameters.ParameterList is null)
            return 0;

        if (_positionManager.Compare(coordinate, invocation.Parameters.Location, PositionComparison.SmallerThan))
            return 0;

        for (var i = 0; i < invocation.Parameters.ParameterList.Elements.Count; i++)
        {
            LiteralExpressionSyntax parameter = invocation.Parameters.ParameterList.Elements[i];

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

    private void UpdateParameterButtons()
    {
        _prevParameterButton.Enabled = _selectedParameterIndex > 0;
        _nextParameterButton.Enabled = _selectedParameterIndex + 1 < (_selectedInvocation?.Parameters.ParameterList?.Elements.Count ?? 0);
    }
}