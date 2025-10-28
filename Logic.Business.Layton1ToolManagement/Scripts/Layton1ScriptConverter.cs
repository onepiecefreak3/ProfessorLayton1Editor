using Logic.Business.Layton1ToolManagement.InternalContract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using Logic.Domain.Level5Management.Contract.Script.Gds;

namespace Logic.Business.Layton1ToolManagement.Scripts;

class Layton1ScriptConverter(
    IGdsScriptParser scriptParser,
    IGdsScriptWriter scriptWriter,
    ILayton1ScriptFileConverter scriptFileConverter,
    ILayton1ScriptCodeUnitConverter scriptUnitConverter,
    ILevel5ScriptWhitespaceNormalizer whitespaceNormalizer)
    : ILayton1ScriptConverter
{
    public CodeUnitSyntax Parse(Stream input, string gameCode)
    {
        GdsScriptFile scriptFile = scriptParser.Parse(input);
        CodeUnitSyntax codeUnit = scriptFileConverter.CreateCodeUnit(scriptFile, gameCode);

        whitespaceNormalizer.NormalizeCodeUnit(codeUnit);

        return codeUnit;
    }

    public Stream Compose(CodeUnitSyntax syntax, string gameCode)
    {
        var output = new MemoryStream();

        GdsScriptFile scriptFile = scriptUnitConverter.CreateScriptFile(syntax, gameCode);
        scriptWriter.Write(scriptFile, output);

        output.Position = 0;

        return output;
    }
}