namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

public class MethodInvocationExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken Identifier { get; private set; }
    public MethodInvocationParametersSyntax Parameters { get; private set; }

    public override SyntaxLocation Location => Identifier.FullLocation;
    public override SyntaxSpan Span => new(Identifier.FullSpan.Position, Parameters.Span.EndPosition);

    public MethodInvocationExpressionSyntax(SyntaxToken identifier, MethodInvocationParametersSyntax parameters)
    {
        identifier.Parent = this;
        parameters.Parent = this;

        Identifier = identifier;
        Parameters = parameters;

        Root.Update();
    }

    public void SetIdentifier(SyntaxToken identifier, bool updatePosition = true)
    {
        identifier.Parent = this;
        Identifier = identifier;

        if (updatePosition)
            Root.Update();
    }

    public void SetParameters(MethodInvocationParametersSyntax parameters, bool updatePosition = true)
    {
        parameters.Parent = this;
        Parameters = parameters;

        if (updatePosition)
            Root.Update();
    }

    internal override int UpdatePosition(int position, ref int line, ref int column)
    {
        SyntaxToken identifier = Identifier;

        position = identifier.UpdatePosition(position, ref line, ref column);
        position = Parameters.UpdatePosition(position, ref line, ref column);

        Identifier = identifier;

        return position;
    }
}