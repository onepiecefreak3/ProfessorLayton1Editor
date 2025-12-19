namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

public class ElseIfBodySyntax : ElseSyntax
{
    public SyntaxToken Else { get; private set; }
    public StatementSyntax If { get; private set; }

    public override SyntaxLocation Location => Else.FullLocation;
    public override SyntaxSpan Span => new(Else.FullSpan.Position, If.Span.EndPosition);

    public ElseIfBodySyntax(SyntaxToken elseToken, StatementSyntax ifSyntax)
    {
        elseToken.Parent = this;
        ifSyntax.Parent = this;

        Else = elseToken;
        If = ifSyntax;

        Root.Update();
    }

    public void SetElse(SyntaxToken elseToken, bool updatePosition = true)
    {
        elseToken.Parent = this;
        Else = elseToken;

        if (updatePosition)
            Root.Update();
    }

    public void SetIf(StatementSyntax ifSyntax, bool updatePosition = true)
    {
        ifSyntax.Parent = this;
        If = ifSyntax;

        if (updatePosition)
            Root.Update();
    }

    internal override int UpdatePosition(int position, ref int line, ref int column)
    {
        SyntaxToken elseToken = Else;

        position = elseToken.UpdatePosition(position, ref line, ref column);
        position = If.UpdatePosition(position, ref line, ref column);

        Else = elseToken;

        return position;
    }
}