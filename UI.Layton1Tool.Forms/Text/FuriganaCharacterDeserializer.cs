using Kaligraphy.DataClasses.Parsing;
using Kaligraphy.Parsing;

namespace UI.Layton1Tool.Forms.Text;

internal class FuriganaCharacterDeserializer : CharacterDeserializer<FuriganaCharacterDeserializerContext>
{
    private readonly bool _ignoreFurigana;

    public FuriganaCharacterDeserializer(bool ignoreFurigana)
    {
        _ignoreFurigana = ignoreFurigana;
    }

    protected override bool TryDeserializeCharacter(FuriganaCharacterDeserializerContext context, int position, out int length,
        out TextCharacterData? textCharacter)
    {
        length = 0;
        textCharacter = null;

        if (context.Text is null)
            return false;

        if (IsFuriganaStart(context, position, out length))
        {
            context.IsFuriganaBottom = true;
            context.IsFuriganaTop = false;

            textCharacter = new FuriganaStartCharacterData { IsVisible = false, IsPersistent = false, Character = context.Text[position] };
            return true;
        }

        if (IsFuriganaSplit(context, position, out length))
        {
            context.IsFuriganaBottom = false;
            context.IsFuriganaTop = true;

            textCharacter = new FuriganaSplitCharacterData { IsVisible = false, IsPersistent = false, Character = context.Text[position] };
            return true;
        }

        if (IsFuriganaEnd(context, position, out length))
        {
            context.IsFuriganaBottom = false;
            context.IsFuriganaTop = false;

            textCharacter = new FuriganaEndCharacterData { IsVisible = false, IsPersistent = false, Character = context.Text[position] };
            return true;
        }

        if (IsLineBreak(context, position, out length, out string lineBreak))
        {
            textCharacter = new LineBreakCharacterData { IsVisible = false, LineBreak = lineBreak };
            return true;
        }

        length = 1;

        textCharacter = new FontCharacterData { IsVisible = !(_ignoreFurigana && context.IsFuriganaTop), IsPersistent = !(_ignoreFurigana && context.IsFuriganaTop), Character = context.Text[position] };
        return true;
    }

    private static bool IsFuriganaStart(FuriganaCharacterDeserializerContext context, int position, out int length)
    {
        length = 0;

        if (context.Text?[position] is not '[')
            return false;

        int endIndex = context.Text.IndexOf(']', position + 1);

        if (endIndex < 0)
            return false;

        length = 1;

        return true;
    }

    private static bool IsFuriganaSplit(FuriganaCharacterDeserializerContext context, int position, out int length)
    {
        length = 0;

        if (!context.IsFuriganaBottom)
            return false;

        if (context.Text?[position] is not '/')
            return false;

        length = 1;

        return true;
    }

    private static bool IsFuriganaEnd(FuriganaCharacterDeserializerContext context, int position, out int length)
    {
        length = 0;

        if (context is { IsFuriganaBottom: false, IsFuriganaTop: false })
            return false;

        if (context.Text?[position] is not ']')
            return false;

        length = 1;

        return true;
    }
}

internal class FuriganaCharacterDeserializerContext : CharacterDeserializerContext
{
    public bool IsFuriganaBottom { get; set; }
    public bool IsFuriganaTop { get; set; }
}

class FuriganaStartCharacterData : FontCharacterData;

class FuriganaSplitCharacterData : FontCharacterData;

class FuriganaEndCharacterData : FontCharacterData;