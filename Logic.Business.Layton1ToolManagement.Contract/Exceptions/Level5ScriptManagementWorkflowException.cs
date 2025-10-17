using System.Runtime.Serialization;

namespace Logic.Business.Layton1ToolManagement.Contract.Exceptions;

[Serializable]
public class Layton1ToolManagementException : Exception
{
    public Layton1ToolManagementException()
    {
    }

    public Layton1ToolManagementException(string message) : base(message)
    {
    }

    public Layton1ToolManagementException(string message, Exception inner) : base(message, inner)
    {
    }

    protected Layton1ToolManagementException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}