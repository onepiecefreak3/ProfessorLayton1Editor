namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

public class WhileStatementSyntax : StatementSyntax
{
    public SyntaxToken While { get; protected set; }
    public ExpressionSyntax Expression { get; private set; }
    public BodySyntax Body { get; private set; }

    public override SyntaxLocation Location => While.FullLocation;
    public override SyntaxSpan Span => new(While.FullSpan.Position, Body.Span.EndPosition);

    public WhileStatementSyntax(SyntaxToken whileToken, ExpressionSyntax expression, BodySyntax body)
    {
        whileToken.Parent = this;
        expression.Parent = this;
        body.Parent = this;

        While = whileToken;
        Expression = expression;
        Body = body;

        Root.Update();
    }

    public void SetWhile(SyntaxToken whileToken, bool updatePosition = true)
    {
        whileToken.Parent = this;
        While = whileToken;

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
        SyntaxToken whileToken = While;

        position = whileToken.UpdatePosition(position, ref line, ref column);
        position = Expression.UpdatePosition(position, ref line, ref column);
        position = Body.UpdatePosition(position, ref line, ref column);

        While = whileToken;

        return position;
    }
}