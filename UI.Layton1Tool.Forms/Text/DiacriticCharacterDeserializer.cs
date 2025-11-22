using Kaligraphy.DataClasses.Parsing;
using Kaligraphy.Parsing;

namespace UI.Layton1Tool.Forms.Text;

internal class DiacriticCharacterDeserializer : CharacterDeserializer
{
    protected override bool TryDeserializeCharacter(CharacterDeserializerContext context, int position, out int length,
        out TextCharacterData? textCharacter)
    {
        length = 0;
        textCharacter = null;

        if (context.Text is null)
            return false;

        if (IsEscapeCharacter(context, position, out length, out char escapeCharacter))
        {
            textCharacter = new EscapeCharacterData { IsVisible = true, IsPersistent = true, Character = escapeCharacter };
            return true;
        }

        return base.TryDeserializeCharacter(context, position, out length, out textCharacter);
    }

    private static bool IsEscapeCharacter(CharacterDeserializerContext context, int position, out int length, out char escapeCharacter)
    {
        length = 0;
        escapeCharacter = '\0';

        if (context.Text?[position] is not '<')
            return false;

        int endIndex = context.Text.IndexOf('>', position + 1);

        if (endIndex < 0)
            return false;

        if (!TryEscapeCharacter(context.Text[(position + 1)..endIndex], out escapeCharacter))
            return false;

        length = endIndex - position + 1;

        return true;
    }

    private static bool TryEscapeCharacter(string sequence, out char escapeCharacter)
    {
        escapeCharacter = sequence switch
        {
            "`a" => 'à',
            "'a" => 'á',
            "^a" => 'â',
            ":a" => 'ä',
            "`e" => 'è',
            "'e" => 'é',
            "^e" => 'ê',
            ":e" => 'ë',
            "`i" => 'ì',
            "'i" => 'í',
            "^i" => 'î',
            ":i" => 'ï',
            "`o" => 'ò',
            "'o" => 'ó',
            "^o" => 'ô',
            ":o" => 'ö',
            "oe" => 'œ',
            "`u" => 'ù',
            "'u" => 'ú',
            "^u" => 'û',
            ":u" => 'ü',
            "`A" => 'À',
            "'A" => 'Á',
            "~A" => 'Ã',
            "^A" => 'Â',
            ":A" => 'Ä',
            "`E" => 'È',
            "'E" => 'É',
            "^E" => 'Ê',
            ":E" => 'Ë',
            "`I" => 'Ì',
            "'I" => 'Í',
            "^I" => 'Î',
            ":I" => 'Ï',
            "`O" => 'Ò',
            "'O" => 'Ó',
            "^O" => 'Ô',
            ":O" => 'Ö',
            "OE" => 'Œ',
            "`U" => 'Ù',
            "'U" => 'Ú',
            "^U" => 'Û',
            ":U" => 'Ü',
            ",c" => 'ç',
            ",C" => 'Ç',
            "~n" => 'ñ',
            "~N" => 'Ñ',
            "ss" => 'ß',
            "^!" => '¡',
            "^?" => '¿',
            _ => '\0'
        };

        return escapeCharacter is not '\0';
    }
}

class EscapeCharacterData : FontCharacterData;