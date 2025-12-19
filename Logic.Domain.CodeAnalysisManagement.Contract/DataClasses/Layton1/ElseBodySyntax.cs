namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

public class ElseBodySyntax : ElseSyntax
{
    public SyntaxToken Else { get; private set; }
    public BodySyntax Body { get; private set; }

    public override SyntaxLocation Location => Else.FullLocation;
    public override SyntaxSpan Span => new(Else.FullSpan.Position, Body.Span.EndPosition);

    public ElseBodySyntax(SyntaxToken elseToken, BodySyntax body)
    {
        elseToken.Parent = this;
        body.Parent = this;

        Else = elseToken;
        Body = body;

        Root.Update();
    }

    public void SetElse(SyntaxToken elseToken, bool updatePosition = true)
    {
        elseToken.Parent = this;
        Else = elseToken;

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
        SyntaxToken elseToken = Else;

        position = elseToken.UpdatePosition(position, ref line, ref column);
        position = Body.UpdatePosition(position, ref line, ref column);

        Else = elseToken;

        return position;
    }
}