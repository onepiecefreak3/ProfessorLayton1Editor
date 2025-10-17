﻿using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract;
using Logic.Domain.CodeAnalysisManagement.Level5.InternalContract.DataClasses;

namespace Logic.Domain.CodeAnalysisManagement.Level5;

internal class Level5ScriptFactory : ITokenFactory<Level5SyntaxToken>
{
    private readonly ICoCoKernel _kernel;

    public Level5ScriptFactory(ICoCoKernel kernel)
    {
        _kernel = kernel;
    }

    public ILexer<Level5SyntaxToken> CreateLexer(string text)
    {
        var buffer = _kernel.Get<IBuffer<int>>(
            new ConstructorParameter("text", text));
        return _kernel.Get<ILexer<Level5SyntaxToken>>(
            new ConstructorParameter("buffer", buffer));
    }

    public IBuffer<Level5SyntaxToken> CreateTokenBuffer(ILexer<Level5SyntaxToken> lexer)
    {
        return _kernel.Get<IBuffer<Level5SyntaxToken>>(new ConstructorParameter("lexer", lexer));
    }
}