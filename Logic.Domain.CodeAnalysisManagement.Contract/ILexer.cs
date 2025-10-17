using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.CodeAnalysisManagement.Contract.Exceptions;

namespace Logic.Domain.CodeAnalysisManagement.Contract;

[MapException(typeof(LexerException))]
public interface ILexer<out TToken> where TToken : struct
{
    bool IsEndOfInput { get; }

    TToken Read();
}