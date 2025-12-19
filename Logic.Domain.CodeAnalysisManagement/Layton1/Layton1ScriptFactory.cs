using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract;
using Logic.Domain.CodeAnalysisManagement.Layton1.InternalContract.DataClasses;

namespace Logic.Domain.CodeAnalysisManagement.Layton1;

internal class Layton1ScriptFactory : ITokenFactory<Layton1SyntaxToken>
{
    private readonly ICoCoKernel _kernel;

    public Layton1ScriptFactory(ICoCoKernel kernel)
    {
        _kernel = kernel;
    }

    public ILexer<Layton1SyntaxToken> CreateLexer(string text)
    {
        var buffer = _kernel.Get<IBuffer<int>>(
            new ConstructorParameter("text", text));
        return _kernel.Get<ILexer<Layton1SyntaxToken>>(
            new ConstructorParameter("buffer", buffer));
    }

    public IBuffer<Layton1SyntaxToken> CreateTokenBuffer(ILexer<Layton1SyntaxToken> lexer)
    {
        return _kernel.Get<IBuffer<Layton1SyntaxToken>>(new ConstructorParameter("lexer", lexer));
    }
}