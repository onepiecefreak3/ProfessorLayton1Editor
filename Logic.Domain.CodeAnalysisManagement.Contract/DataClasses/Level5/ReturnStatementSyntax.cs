﻿namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

public class ReturnStatementSyntax : StatementSyntax
{
    public SyntaxToken Return { get; private set; }
    public ValueExpressionSyntax? ValueExpression { get; private set; }
    public SyntaxToken Semicolon { get; private set; }

    public override SyntaxLocation Location => Return.FullLocation;
    public override SyntaxSpan Span => new(Return.FullSpan.Position, Semicolon.FullSpan.EndPosition);

    public ReturnStatementSyntax(SyntaxToken returnToken, ValueExpressionSyntax? valueExpression, SyntaxToken semicolon)
    {
        returnToken.Parent = this;
        if (valueExpression != null)
            valueExpression.Parent = this;
        semicolon.Parent = this;

        Return = returnToken;
        ValueExpression = valueExpression;
        Semicolon = semicolon;

        Root.Update();
    }

    public void SetReturn(SyntaxToken returnToken, bool updatePositions = true)
    {
        returnToken.Parent = this;
        Return = returnToken;

        if (updatePositions)
            Root.Update();
    }

    public void SetValue(ValueExpressionSyntax? valueExpression, bool updatePositions = true)
    {
        if (valueExpression != null)
            valueExpression.Parent = this;
        ValueExpression = valueExpression;

        if (updatePositions)
            Root.Update();
    }

    public void SetSemicolon(SyntaxToken semicolon, bool updatePositions = true)
    {
        semicolon.Parent = this;
        Semicolon = semicolon;

        if (updatePositions)
            Root.Update();
    }

    internal override int UpdatePosition(int position, ref int line, ref int column)
    {
        SyntaxToken returnToken = Return;
        SyntaxToken semicolon = Semicolon;

        position = returnToken.UpdatePosition(position, ref line, ref column);
        if (ValueExpression != null)
            position = ValueExpression.UpdatePosition(position, ref line, ref column);
        position = semicolon.UpdatePosition(position, ref line, ref column);

        Return = returnToken;
        Semicolon = semicolon;

        return position;
    }
}