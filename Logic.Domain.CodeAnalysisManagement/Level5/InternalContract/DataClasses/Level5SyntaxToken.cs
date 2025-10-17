using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

namespace Logic.Domain.CodeAnalysisManagement.Level5.InternalContract.DataClasses;

public struct Level5SyntaxToken
{
    public SyntaxTokenKind Kind { get; }
    public string Text { get; }

    public int Position { get; }
    public int Line { get; }
    public int Column { get; }

    public Level5SyntaxToken(SyntaxTokenKind kind, int position, int line, int column, string? text = null)
    {
        Text = text ?? string.Empty;
        Kind = kind;
        Position = position;
        Line = line;
        Column = column;
    }
}