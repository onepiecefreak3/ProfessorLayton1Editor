﻿namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

public class IfGotoStatementSyntax : StatementSyntax
{
    public SyntaxToken If { get; private set; }
    public ValueExpressionSyntax Value { get; private set; }
    public GotoStatementSyntax Goto { get; private set; }

    public override SyntaxLocation Location => If.FullLocation;
    public override SyntaxSpan Span => new(If.FullSpan.Position, Goto.Span.EndPosition);

    public IfGotoStatementSyntax(SyntaxToken ifToken, ValueExpressionSyntax value, GotoStatementSyntax gotoStatement)
    {
        ifToken.Parent = this;
        value.Parent = this;
        gotoStatement.Parent = this;

        If = ifToken;
        Value = value;
        Goto = gotoStatement;

        Root.Update();
    }

    public void SetIf(SyntaxToken ifToken, bool updatePositions = true)
    {
        ifToken.Parent = this;

        If = ifToken;

        if (updatePositions)
            Root.Update();
    }

    internal override int UpdatePosition(int position, ref int line, ref int column)
    {
        SyntaxToken ifToken = If;

        position = ifToken.UpdatePosition(position, ref line, ref column);
        position = Value.UpdatePosition(position, ref line, ref column);
        position = Goto.UpdatePosition(position, ref line, ref column);

        If = ifToken;

        return position;
    }
}