using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;
using Logic.Domain.CodeAnalysisManagement.Contract.Exceptions.Level5;

namespace Logic.Domain.CodeAnalysisManagement.Contract.Level5;

[MapException(typeof(Level5ScriptParserException))]
public interface ILayton1ScriptParser
{
    CodeUnitSyntax ParseCodeUnit(string text);
}