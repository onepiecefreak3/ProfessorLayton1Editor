using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

namespace Logic.Domain.CodeAnalysisManagement.Layton1.InternalContract.DataClasses;

public struct Layton1SyntaxToken
{
    public SyntaxTokenKind Kind { get; }
    public string Text { get; }

    public int Position { get; }
    public int Line { get; }
    public int Column { get; }

    public Layton1SyntaxToken(SyntaxTokenKind kind, int position, int line, int column, string? text = null)
    {
        Text = text ?? string.Empty;
        Kind = kind;
        Position = position;
        Line = line;
        Column = column;
    }
}