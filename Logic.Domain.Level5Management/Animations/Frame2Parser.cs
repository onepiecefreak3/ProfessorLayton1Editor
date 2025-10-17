using Kanvas.Encoding;
using Kanvas;
using Kanvas.Swizzle;
using Konnect.Contract.DataClasses.Plugin.File.Image;
using Konnect.Plugin.File.Image;
using Logic.Domain.Level5Management.Contract.Animations;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Logic.Domain.Level5Management.Contract.Enums;

namespace Logic.Domain.Level5Management.Animations;

class Frame2Parser(IFrame2Reader reader) : IFrame2Parser
{
    public AnimationSequences Parse(Stream input)
    {
        var container = reader.Read(input);

        return Parse(container);
    }

    public AnimationSequences Parse(Frame2Container container)
    {
        return new AnimationSequences
        {
            Frames = ParseFrames(container),
            Sequences = ParseAnimations(container)
        };
    }

    private AnimationFrame[] ParseFrames(Frame2Container container)
    {
        var result = new List<AnimationFrame>(container.ImageEntries.Length);

        var definition = new EncodingDefinition();
        definition.AddPaletteEncoding(0, new Rgba(5, 5, 5, "BGR"));
        definition.AddIndexEncoding(3, ImageFormats.I4(Komponent.Contract.Enums.BitOrder.LeastSignificantBitFirst), [0]);
        definition.AddIndexEncoding(4, ImageFormats.I8(), [0]);

        foreach (var frame in container.ImageEntries)
        {
            var finalImage = new Image<Rgba32>(frame.width, frame.height);

            var errors = AnimationFrameErrorType.None;
            foreach (var part in frame.entries)
            {
                if (container.ImageFormat != part.imageFormat)
                    errors |= AnimationFrameErrorType.InconsistentEncoding;

                if (part.x >= frame.width || part.y >= frame.height)
                {
                    errors |= AnimationFrameErrorType.InconsistentCoordinates;
                    continue;
                }

                var bitDepth = definition.GetIndexEncoding(part.imageFormat)!.IndexEncoding.BitDepth;

                var imagePart = new ImageFile(new ImageFileInfo
                {
                    ImageData = part.data,
                    PaletteData = container.PaletteData,
                    BitDepth = bitDepth,
                    PaletteBitDepth = 16,
                    ImageFormat = part.imageFormat,
                    PaletteFormat = 0,
                    ImageSize = new Size(8 << part.widthShift, 8 << part.heightShift),
                    RemapPixels = context => new NitroSwizzle(context),
                    Quantize = context => context.WithColorCount(256)
                }, definition);

                finalImage.Mutate(context => context.DrawImage(imagePart.GetImage(), new Point(part.x, part.y), 1f));
            }

            result.Add(new AnimationFrame
            {
                Image = finalImage,
                Errors = errors
            });
        }

        return [.. result];
    }

    private AnimationSequence[] ParseAnimations(Frame2Container container)
    {
        var result = new AnimationSequence[container.AnimationEntries.Length];

        for (var i = 0; i < container.AnimationEntries.Length; i++)
        {
            result[i] = new AnimationSequence
            {
                Name = container.AnimationNames[i + 1],
                Steps = ParseAnimationSteps(container.AnimationEntries[i].entries)
            };
        }

        return result;
    }

    private AnimationStep[] ParseAnimationSteps(AnimationSubEntry[] entries)
    {
        var result = new AnimationStep[entries.Length];

        for (var i = 0; i < entries.Length; i++)
        {
            result[i] = new AnimationStep
            {
                NextStepIndex = entries[i].nextStepIndex,
                FrameCounter = entries[i].frameCounter,
                FrameIndex = entries[i].frameIndex
            };
        }

        return result;
    }
}