namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

public class BodySyntax : SyntaxNode
{
    public SyntaxToken CurlyOpen { get; private set; }
    public IReadOnlyList<StatementSyntax> Statements { get; private set; }
    public SyntaxToken CurlyClose { get; private set; }

    public override SyntaxLocation Location => CurlyOpen.FullLocation;
    public override SyntaxSpan Span => new(CurlyOpen.FullSpan.Position, CurlyClose.FullSpan.EndPosition);

    public BodySyntax(SyntaxToken curlyOpen, IReadOnlyList<StatementSyntax> statements, SyntaxToken curlyClose)
    {
        curlyOpen.Parent = this;
        curlyClose.Parent = this;

        foreach (StatementSyntax expression in statements)
            expression.Parent = this;

        CurlyOpen = curlyOpen;
        Statements = statements;
        CurlyClose = curlyClose;

        Root.Update();
    }

    public void SetCurlyOpen(SyntaxToken curlyOpen, bool updatePosition = true)
    {
        curlyOpen.Parent = this;
        CurlyOpen = curlyOpen;

        if (updatePosition)
            Root.Update();
    }

    public void SetStatements(IReadOnlyList<StatementSyntax> statements, bool updatePosition = true)
    {
        Statements = statements;

        foreach (StatementSyntax expression in Statements)
            expression.Parent = this;

        if (updatePosition)
            Root.Update();
    }

    public void SetCurlyClose(SyntaxToken curlyClose, bool updatePosition = true)
    {
        curlyClose.Parent = this;
        CurlyClose = curlyClose;

        if (updatePosition)
            Root.Update();
    }

    internal override int UpdatePosition(int position, ref int line, ref int column)
    {
        SyntaxToken openToken = CurlyOpen;
        SyntaxToken closeToken = CurlyClose;

        position = openToken.UpdatePosition(position, ref line, ref column);
        foreach (StatementSyntax statement in Statements)
            position = statement.UpdatePosition(position, ref line, ref column);
        position = closeToken.UpdatePosition(position, ref line, ref column);

        CurlyOpen = openToken;
        CurlyClose = closeToken;

        return position;
    }
}