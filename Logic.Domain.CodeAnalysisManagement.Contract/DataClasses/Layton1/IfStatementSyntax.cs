namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

public class IfStatementSyntax : StatementSyntax
{
    public SyntaxToken If { get; protected set; }
    public ExpressionSyntax Expression { get; private set; }
    public BodySyntax Body { get; private set; }

    public override SyntaxLocation Location => If.FullLocation;
    public override SyntaxSpan Span => new(If.FullSpan.Position, Body.Span.EndPosition);

    public IfStatementSyntax(SyntaxToken ifToken, ExpressionSyntax expression, BodySyntax body)
    {
        ifToken.Parent = this;
        expression.Parent = this;
        body.Parent = this;

        If = ifToken;
        Expression = expression;
        Body = body;

        Root.Update();
    }

    public void SetIf(SyntaxToken ifToken, bool updatePosition = true)
    {
        ifToken.Parent = this;
        If = ifToken;

        if (updatePosition)
            Root.Update();
    }

    public void SetExpression(ExpressionSyntax expression, bool updatePosition = true)
    {
        expression.Parent = this;
        Expression = expression;

        if (updatePosition)
            Root.Update();
    }

    public void SetThenBody(BodySyntax body, bool updatePosition = true)
    {
        body.Parent = this;
        Body = body;

        if (updatePosition)
            Root.Update();
    }

    internal override int UpdatePosition(int position, ref int line, ref int column)
    {
        SyntaxToken ifToken = If;

        position = ifToken.UpdatePosition(position, ref line, ref column);
        position = Expression.UpdatePosition(position, ref line, ref column);
        position = Body.UpdatePosition(position, ref line, ref column);

        If = ifToken;

        return position;
    }
}