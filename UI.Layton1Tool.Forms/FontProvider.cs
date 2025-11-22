using CrossCutting.Core.Contract.EventBrokerage;
using Kaligraphy.Contract.Rendering;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using Konnect.Plugin.File.Font;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Font;
using UI.Layton1Tool.Forms.InternalContract;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Messages.Enums;

namespace UI.Layton1Tool.Forms;

internal class FontProvider : IFontProvider
{
    private readonly Dictionary<Layton1NdsRom, IGlyphProvider> _questionFonts = [];
    private readonly Dictionary<Layton1NdsRom, IGlyphProvider> _eventFonts = [];
    private readonly Dictionary<Layton1NdsRom, IGlyphProvider> _furiganaFonts = [];

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PathProvider _pathProvider;

    public FontProvider(IEventBroker eventBroker, ILayton1NdsFileManager fileManager, ILayton1PathProvider pathProvider)
    {
        _eventBroker = eventBroker;
        _fileManager = fileManager;
        _pathProvider = pathProvider;

        eventBroker.Subscribe<FileContentModifiedMessage>(ProcessFileContentModified);
    }

    public IGlyphProvider? GetQuestionFont(Layton1NdsRom rom)
    {
        if (_questionFonts.TryGetValue(rom, out IGlyphProvider? font))
            return font;

        if (rom.Version is GameVersion.Japan or GameVersion.JapanFriendly && _eventFonts.TryGetValue(rom, out font))
            return _questionFonts[rom] = font;

        string fontPath = GetQuestionFontPath(rom.Version);

        if (!_fileManager.TryGet(rom, fontPath, out Layton1NdsFile? fontFile))
            return null;

        if (_fileManager.Parse(fontFile, FileType.Font) is not NftrData fontData)
            return null;

        return _questionFonts[rom] = new FontPluginGlyphProvider(fontData.Characters);
    }

    public IGlyphProvider? GetEventFont(Layton1NdsRom rom)
    {
        if (_eventFonts.TryGetValue(rom, out IGlyphProvider? font))
            return font;

        if (rom.Version is GameVersion.Japan or GameVersion.JapanFriendly && _questionFonts.TryGetValue(rom, out font))
            return _eventFonts[rom] = font;

        string fontPath = GetEventFontPath(rom.Version);

        if (!_fileManager.TryGet(rom, fontPath, out Layton1NdsFile? fontFile))
            return null;

        if (_fileManager.Parse(fontFile, FileType.Font) is not NftrData fontData)
            return null;

        return _eventFonts[rom] = new FontPluginGlyphProvider(fontData.Characters);
    }

    public IGlyphProvider? GetFuriganaFont(Layton1NdsRom rom)
    {
        if (_furiganaFonts.TryGetValue(rom, out IGlyphProvider? font))
            return font;

        string? fontPath = GetFuriganaFontPath(rom.Version);

        if (fontPath is null || !_fileManager.TryGet(rom, fontPath, out Layton1NdsFile? fontFile))
            return null;

        if (_fileManager.Parse(fontFile, FileType.Font) is not NftrData fontData)
            return null;

        return _furiganaFonts[rom] = new FontPluginGlyphProvider(fontData.Characters);
    }

    public void Free(Layton1NdsRom rom)
    {
        _questionFonts.Remove(rom);
        _eventFonts.Remove(rom);
        _furiganaFonts.Remove(rom);
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (message.Content is not NftrData fontData)
            return;

        FontType type;
        IGlyphProvider font;

        if (message.File.Path == GetQuestionFontPath(message.File.Rom.Version))
        {
            type = FontType.Question;
            font = _questionFonts[message.File.Rom] = new FontPluginGlyphProvider(fontData.Characters);
        }
        else if (message.File.Path == GetEventFontPath(message.File.Rom.Version))
        {
            type = FontType.Event;
            font = _eventFonts[message.File.Rom] = new FontPluginGlyphProvider(fontData.Characters);
        }
        else if (message.File.Path == GetFuriganaFontPath(message.File.Rom.Version))
        {
            type = FontType.Furigana;
            font = _furiganaFonts[message.File.Rom] = new FontPluginGlyphProvider(fontData.Characters);
        }
        else
        {
            return;
        }

        RaiseFontModified(message.File, font, type);
    }

    private void RaiseFontModified(Layton1NdsFile file, IGlyphProvider font, FontType type)
    {
        _eventBroker.Raise(new FontModifiedMessage(file, font, type));
    }

    private string GetQuestionFontPath(GameVersion version)
    {
        switch (version)
        {
            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                return _pathProvider.GetFullDirectory("font/fontq.NFTR", version);

            case GameVersion.Korea:
                return _pathProvider.GetFullDirectory("font/", version, TextLanguage.Korean) + "fontevent.NFTR";

            case GameVersion.JapanFriendly:
                return _pathProvider.GetFullDirectory("font/", version, TextLanguage.Japanese) + "font.NFTR";

            case GameVersion.Japan:
                return _pathProvider.GetFullDirectory("font/font.NFTR", version);

            default:
                throw new InvalidOperationException($"Unknown game version {version}.");
        }
    }

    private string GetEventFontPath(GameVersion version)
    {
        switch (version)
        {
            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                return _pathProvider.GetFullDirectory("font/fontevent.NFTR", version);

            case GameVersion.Korea:
                return _pathProvider.GetFullDirectory("font/", version, TextLanguage.Korean) + "fontevent.NFTR";

            case GameVersion.JapanFriendly:
                return _pathProvider.GetFullDirectory("font/", version, TextLanguage.Japanese) + "font.NFTR";

            case GameVersion.Japan:
                return _pathProvider.GetFullDirectory("font/font.NFTR", version);

            default:
                throw new InvalidOperationException($"Unknown game version {version}.");
        }
    }

    private string? GetFuriganaFontPath(GameVersion version)
    {
        switch (version)
        {
            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
            case GameVersion.Korea:
                return null;

            case GameVersion.JapanFriendly:
                return _pathProvider.GetFullDirectory("font/", version, TextLanguage.Japanese) + "rubi_font.NFTR";

            default:
                throw new InvalidOperationException($"Unknown game version {version}.");
        }
    }
}