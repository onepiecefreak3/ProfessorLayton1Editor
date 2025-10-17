﻿namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

public class GotoLabelStatementSyntax : StatementSyntax
{
    public LiteralExpressionSyntax Label { get; private set; }
    public SyntaxToken Colon { get; private set; }

    public override SyntaxLocation Location => Label.Location;
    public override SyntaxSpan Span => new(Label.Span.Position, Colon.FullSpan.EndPosition);

    public GotoLabelStatementSyntax(LiteralExpressionSyntax label, SyntaxToken colon)
    {
        label.Parent = this;
        colon.Parent = this;

        Label = label;
        Colon = colon;

        Root.Update();
    }

    public void SetLabel(LiteralExpressionSyntax label, bool updatePosition = true)
    {
        label.Parent = this;
        Label = label;

        if (updatePosition)
            Root.Update();
    }

    public void SetColon(SyntaxToken colon, bool updatePosition = true)
    {
        colon.Parent = this;
        Colon = colon;

        if (updatePosition)
            Root.Update();
    }

    internal override int UpdatePosition(int position, ref int line, ref int column)
    {
        SyntaxToken colon = Colon;

        position = Label.UpdatePosition(position, ref line, ref column);
        position = colon.UpdatePosition(position, ref line, ref column);

        Colon = colon;

        return position;
    }
}