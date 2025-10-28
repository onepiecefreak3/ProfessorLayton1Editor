﻿using System.Text;
using Komponent.IO;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using Logic.Domain.Level5Management.Contract.Script.Gds;

namespace Logic.Domain.Level5Management.Script.Gds;

class GdsScriptWriter(IGdsScriptComposer composer) : IGdsScriptWriter
{
    private static readonly Encoding SjisEncoding = Encoding.GetEncoding("Shift-JIS");

    public void Write(GdsScriptFile script, Stream output)
    {
        GdsArgument[] arguments = composer.Compose(script);

        Write(arguments, output);
    }

    public void Write(GdsArgument[] arguments, Stream output)
    {
        using var writer = new BinaryWriterX(output, true);

        output.Position = 4;
        foreach (GdsArgument argument in arguments)
        {
            writer.Write(argument.type);

            switch (argument.type)
            {
                case 0:
                    writer.Write((short)argument.value!);
                    break;

                case 1:
                case 6:
                case 7:
                    writer.Write((int)argument.value!);
                    break;

                case 2:
                    writer.Write((float)argument.value!);
                    break;

                case 3:
                    var stringValue = (string)argument.value!;

                    byte[] stringBytes = SjisEncoding.GetBytes(stringValue + '\0');
                    writer.Write((short)stringBytes.Length);
                    writer.Write(stringBytes);
                    break;

                case 8:
                case 9:
                case 11:
                case 12:
                    break;

                default:
                    throw new InvalidOperationException($"Unknown argument type {argument.type}.");
            }
        }

        output.Position = 0;
        writer.Write((int)output.Length - 4);

        output.Position = output.Length;
    }
}