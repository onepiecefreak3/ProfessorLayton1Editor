namespace Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

public enum SyntaxTokenKind
{
    Comma,
    Colon,
    Semicolon,
    Slash,
    Minus,

    ParenOpen,
    ParenClose,
    CurlyOpen,
    CurlyClose,

    Trivia,

    StringLiteral,
    NumericLiteral,
    UnsignedNumericLiteral,
    HashNumericLiteral,
    HashStringLiteral,
    FloatingNumericLiteral,

    Identifier,

    ReturnKeyword,
    BreakKeyword,
    NotKeyword,
    OrKeyword,
    AndKeyword,
    IfKeyword,
    ElseKeyword,
    WhileKeyword,
    TrueKeyword,
    FalseKeyword,

    EndOfFile
}