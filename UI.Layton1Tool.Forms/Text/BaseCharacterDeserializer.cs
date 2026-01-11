using Kaligraphy.DataClasses.Parsing;
using Kaligraphy.Parsing;

namespace UI.Layton1Tool.Forms.Text;

internal class BaseCharacterDeserializer : BaseCharacterDeserializer<CharacterDeserializerContext>;

internal class BaseCharacterDeserializer<TContext> : CharacterDeserializer<TContext>
    where TContext : CharacterDeserializerContext, new()
{
    protected override bool TryDeserializeControlCode(TContext context, int position, out int length, out ControlCodeCharacterData? controlCode)
    {
        length = 0;
        controlCode = null;

        if (context.Text is null)
            return false;

        if (IsControlCode(context, position, out length))
        {
            switch (context.Text[position + 1])
            {
                case 'c':
                    controlCode = new BreakCharacterData { IsVisible = false, IsPersistent = false };
                    return true;

                case 'l':
                    controlCode = new DialogueBreakCharacterData { IsVisible = false, IsPersistent = false };
                    return true;

                case 'p':
                    controlCode = new PauseCharacterData { IsVisible = false, IsPersistent = false };
                    return true;

                case 'w':
                    controlCode = length > 2 ?
                        new WaitCharacterData { IsVisible = false, IsPersistent = false, Time = int.Parse(context.Text[(position + 2)..(position + length)]) } :
                        new WaitCharacterData { IsVisible = false, IsPersistent = false, Time = 0 };
                    return true;

                default:
                    controlCode = new AnimationCharacterData { IsVisible = false, IsPersistent = false, Index = int.Parse(context.Text[(position + 1)..(position + length)]) };
                    return true;
            }
        }

        if (IsInvocation(context, position, out length))
        {
            string invocation = context.Text[(position + 1)..(position + length - 1)];
            controlCode = new InvocationCharacterData { IsVisible = false, IsPersistent = false, Parameters = invocation.Split(' ') };
            return true;
        }

        return false;
    }

    private static bool IsControlCode(TContext context, int position, out int length)
    {
        length = 0;

        if (context.Text?[position] is not '@')
            return false;

        if (context.Text[position + 1] is 'w')
        {
            int index = position + 2;
            while (index < context.Text.Length && context.Text[index] is >= '0' and <= '9')
                index++;

            length = index - position;
        }
        else if (context.Text[position + 1] is >= '0' and <= '9')
        {
            int index = position + 2;
            while (index < context.Text.Length && context.Text[index] is >= '0' and <= '9')
                index++;

            length = index - position;
        }
        else if (context.Text[position + 1] is 'c' or 'l' or 'p')
        {
            length = 2;
        }
        else
        {
            return false;
        }

        return true;
    }

    private bool IsInvocation(TContext context, int position, out int length)
    {
        length = 0;

        if (context.Text?[position] is not '&')
            return false;

        int endIndex = context.Text.IndexOf('&', position + 1);

        if (endIndex < 0)
            return false;

        length = endIndex - position + 1;
        return true;
    }
}

class BreakCharacterData : ControlCodeCharacterData;

class DialogueBreakCharacterData : ControlCodeCharacterData;

class PauseCharacterData : ControlCodeCharacterData;

class WaitCharacterData : ControlCodeCharacterData
{
    public required int Time { get; set; }
}

class AnimationCharacterData : ControlCodeCharacterData
{
    public required int Index { get; set; }
}

class InvocationCharacterData : ControlCodeCharacterData
{
    public required string[] Parameters { get; set; }
}