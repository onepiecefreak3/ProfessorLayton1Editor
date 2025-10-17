using System.Runtime.Serialization;

namespace Logic.Domain.CodeAnalysisManagement.Contract.Exceptions.Level5;

[Serializable]
public class Level5SyntaxFactoryException : Exception
{
    public Level5SyntaxFactoryException()
    {
    }

    public Level5SyntaxFactoryException(string message) : base(message)
    {
    }

    public Level5SyntaxFactoryException(string message, Exception inner) : base(message, inner)
    {
    }

    protected Level5SyntaxFactoryException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}