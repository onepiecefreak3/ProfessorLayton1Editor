namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

public class MethodInvocationStatementSyntax : StatementSyntax
{
    public MethodInvocationExpressionSyntax Invocation { get; private set; }
    public SyntaxToken Semicolon { get; private set; }

    public override SyntaxLocation Location => Invocation.Location;
    public override SyntaxSpan Span => new(Invocation.Span.Position, Semicolon.FullSpan.EndPosition);

    public MethodInvocationStatementSyntax(MethodInvocationExpressionSyntax invocation, SyntaxToken semicolon)
    {
        invocation.Parent = this;
        semicolon.Parent = this;

        Invocation = invocation;
        Semicolon = semicolon;

        Root.Update();
    }

    public void SetInvocation(MethodInvocationExpressionSyntax invocation, bool updatePositions = true)
    {
        invocation.Parent = this;

        Invocation = invocation;

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
        SyntaxToken semicolon = Semicolon;

        position = Invocation.UpdatePosition(position, ref line, ref column);
        position = semicolon.UpdatePosition(position, ref line, ref column);

        Semicolon = semicolon;

        return position;
    }
}