﻿using System.Runtime.Serialization;

namespace Logic.Domain.CodeAnalysisManagement.Contract.Exceptions.Level5;

public class Level5ScriptComposerException:Exception
{
    public Level5ScriptComposerException()
    {
    }

    public Level5ScriptComposerException(string message) : base(message)
    {
    }

    public Level5ScriptComposerException(string message, Exception inner) : base(message, inner)
    {
    }

    protected Level5ScriptComposerException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}