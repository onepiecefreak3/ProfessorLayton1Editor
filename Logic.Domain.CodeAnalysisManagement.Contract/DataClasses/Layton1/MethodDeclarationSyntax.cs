namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

public class MethodDeclarationSyntax : SyntaxNode
{
    public SyntaxToken Identifier { get; private set; }
    public MethodDeclarationParametersSyntax Parameters { get; private set; }
    public BodySyntax Body { get; private set; }

    public override SyntaxLocation Location => Identifier.FullLocation;
    public override SyntaxSpan Span => new(Identifier.FullSpan.Position, Body.Span.EndPosition);

    public MethodDeclarationSyntax(SyntaxToken identifier, MethodDeclarationParametersSyntax parameters, BodySyntax body)
    {
        identifier.Parent = this;
        parameters.Parent = this;
        body.Parent = this;

        Identifier = identifier;
        Parameters = parameters;
        Body = body;

        Root.Update();
    }

    public void SetIdentifier(SyntaxToken identifier, bool updatePosition = true)
    {
        identifier.Parent = this;
        Identifier = identifier;

        if (updatePosition)
            Root.Update();
    }

    public void SetParameters(MethodDeclarationParametersSyntax parameters, bool updatePosition = true)
    {
        parameters.Parent = this;
        Parameters = parameters;

        if (updatePosition)
            Root.Update();
    }

    public void SetBody(BodySyntax body, bool updatePosition = true)
    {
        body.Parent = this;
        Body = body;

        if (updatePosition)
            Root.Update();
    }

    internal override int UpdatePosition(int position, ref int line, ref int column)
    {
        SyntaxToken identifier = Identifier;

        position = identifier.UpdatePosition(position, ref line, ref column);
        position = Parameters.UpdatePosition(position, ref line, ref column);
        position = Body.UpdatePosition(position, ref line, ref column);

        Identifier = identifier;

        return position;
    }
}