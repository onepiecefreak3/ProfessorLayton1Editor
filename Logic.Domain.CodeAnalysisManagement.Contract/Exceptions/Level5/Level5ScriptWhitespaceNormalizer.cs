using System.Runtime.Serialization;

namespace Logic.Domain.CodeAnalysisManagement.Contract.Exceptions.Level5;

public class Level5ScriptWhitespaceNormalizer : Exception
{
    public Level5ScriptWhitespaceNormalizer()
    {
    }

    public Level5ScriptWhitespaceNormalizer(string message) : base(message)
    {
    }

    public Level5ScriptWhitespaceNormalizer(string message, Exception inner) : base(message, inner)
    {
    }

    protected Level5ScriptWhitespaceNormalizer(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}