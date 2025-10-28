using Kanvas.Encoding;
using Konnect.Contract.DataClasses.Plugin.File.Image;
using Konnect.Plugin.File.Image;
using Logic.Domain.Level5Management.Contract.Animations;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using Logic.Domain.Level5Management.Contract.Enums;
using SixLabors.ImageSharp;

namespace Logic.Domain.Level5Management.Animations;

class Frame3Parser(IFrame3Reader reader) : IFrame3Parser
{
    public AnimationSequences Parse(Stream input)
    {
        var container = reader.Read(input);

        return Parse(container);
    }

    public AnimationSequences Parse(Frame3Container container)
    {
        return new AnimationSequences
        {
            Frames = ParseFrames(container),
            Sequences = ParseAnimations(container)
        };
    }

    private AnimationFrame[] ParseFrames(Frame3Container container)
    {
        var result = new List<AnimationFrame>(container.ImageEntries.Length);

        var definition = new EncodingDefinition();
        definition.AddColorEncoding(0, new Rgba(5, 6, 5, "RGB"));

        foreach (var frame in container.ImageEntries)
        {
            var errors = AnimationFrameErrorType.None;
            if (container.ImageFormat != frame.imageFormat)
                errors |= AnimationFrameErrorType.InconsistentEncoding;

            var bitDepth = definition.GetColorEncoding(frame.imageFormat)!.BitDepth;

            var imagePart = new ImageFile(new ImageFileInfo
            {
                ImageData = frame.data,
                BitDepth = bitDepth,
                ImageFormat = frame.imageFormat,
                ImageSize = new Size(frame.width, frame.height)
            }, definition);

            result.Add(new AnimationFrame
            {
                Image = imagePart.GetImage(),
                Errors = errors
            });
        }

        return [.. result];
    }

    private AnimationSequence[] ParseAnimations(Frame3Container container)
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