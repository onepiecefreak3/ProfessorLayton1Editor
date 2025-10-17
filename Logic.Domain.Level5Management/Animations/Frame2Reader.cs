using Komponent.IO;
using Logic.Domain.Level5Management.Contract.Animations;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using Logic.Domain.Level5Management.DataClasses.Animations;

namespace Logic.Domain.Level5Management.Animations;

class Frame2Reader : IFrame2Reader
{
    public Frame2Container Read(Stream input)
    {
        using var reader = new BinaryReaderX(input, true);

        bool isValid = CheckFileIntegrity(reader);
        input.Position = 0;

        short frameCount = reader.ReadInt16();
        short imageFormat = reader.ReadInt16();
        int colorCount = reader.ReadInt32();

        var context = new AnimationReaderContext
        {
            FrameCount = frameCount,
            ImageFormat = imageFormat,
            ColorCount = colorCount,
            IsValid = isValid
        };

        var frames = ReadImageEntries(reader, context);
        var paletteData = ReadPaletteData(reader, colorCount);

        input.Position += 0x1E;

        var animationCount = reader.ReadInt32();
        var animationNames = ReadAnimationNames(reader, animationCount);

        input.Position += 4;

        var animations = ReadAnimationEntries(reader, animationCount - 1);

        return new Frame2Container
        {
            ImageFormat = imageFormat,
            ImageEntries = frames,
            PaletteData = paletteData,
            AnimationNames = animationNames,
            AnimationEntries = animations
        };
    }

    private Frame2ImageEntry[] ReadImageEntries(BinaryReaderX reader, AnimationReaderContext context)
    {
        var result = new Frame2ImageEntry[context.FrameCount];

        for (var i = 0; i < context.FrameCount; i++)
        {
            context.FrameIndex = i;

            result[i] = ReadImageEntry(reader);

            context.FrameWidth = result[i].width;
            context.FrameHeight = result[i].height;
            context.PartCount = result[i].partCount;

            result[i].entries = ReadImageSubEntries(reader, context);
        }

        return result;
    }

    private Frame2ImageEntry ReadImageEntry(BinaryReaderX reader)
    {
        var width = reader.ReadInt16();
        var height = reader.ReadInt16();
        var partCount = reader.ReadInt32();

        return new Frame2ImageEntry
        {
            width = width,
            height = height,
            partCount = partCount
        };
    }

    private Frame2ImageSubEntry[] ReadImageSubEntries(BinaryReaderX reader, AnimationReaderContext context)
    {
        var result = new Frame2ImageSubEntry[context.PartCount];

        for (var i = 0; i < context.PartCount; i++)
        {
            context.PartIndex = i;

            result[i] = ReadImageSubEntry(reader);
            result[i].imageFormat = context.IsValid ? context.ImageFormat : DetectImageFormat(reader, result[i], context);

            var dataSize = GetDataSize(result[i], result[i].imageFormat);
            result[i].data = reader.ReadBytes(dataSize);
        }

        return result;
    }

    private Frame2ImageSubEntry ReadImageSubEntry(BinaryReaderX reader)
    {
        return new Frame2ImageSubEntry
        {
            unk = reader.ReadInt32(),
            x = reader.ReadInt16(),
            y = reader.ReadInt16(),
            widthShift = reader.ReadInt16(),
            heightShift = reader.ReadInt16()
        };
    }

    private int DetectImageFormat(BinaryReaderX reader, Frame2ImageSubEntry subEntry, AnimationReaderContext context)
    {
        long bkPos = reader.BaseStream.Position;

        for (var i = 3; i < 5; i++)
        {
            bool wasSuccessful = ApplyImageFormat(reader, subEntry, context, i);
            reader.BaseStream.Position = bkPos;

            if (wasSuccessful)
                return i;
        }

        throw new InvalidOperationException("Could not determine image format.");
    }

    private bool ApplyImageFormat(BinaryReaderX reader, Frame2ImageSubEntry subEntry, AnimationReaderContext context, int imageFormat)
    {
        var dataSize = GetDataSize(subEntry, imageFormat);
        reader.BaseStream.Position += dataSize;

        if (context.PartIndex + 1 < context.PartCount)
        {
            var nextPart = ReadImageSubEntry(reader);

            if (nextPart.heightShift > 5
                || nextPart.widthShift > 5
                || nextPart.x % 8 != 0
                || nextPart.y % 8 != 0
                || nextPart.x >= 256
                || nextPart.y >= 256)
                return false;

            if (nextPart is { x: <= 0, y: <= 0 })
                return false;

            if (nextPart.x <= subEntry.x && nextPart.y <= subEntry.y)
                return false;
        }
        else if (context.FrameIndex + 1 < context.FrameCount)
        {
            var nextFrame = ReadImageEntry(reader);

            if (nextFrame.width is <= 0 or > 256
                || nextFrame.height is <= 0 or > 256
                || nextFrame.partCount == 0
                || nextFrame.partCount >= 0x400)
                return false;

            var nextPart = ReadImageSubEntry(reader);

            if (nextPart.heightShift > 5
                || nextPart.widthShift > 5
                || nextPart.x % 8 != 0
                || nextPart.y % 8 != 0)
                return false;

            if (nextPart is { x: > 0, y: > 0 })
                return false;
        }
        else
        {
            reader.BaseStream.Position += ((context.ColorCount * 2 + 3) & ~3) + 0x1E;
            var animationCount = reader.ReadInt32();

            if (animationCount is <= 0 or >= 64)
                return false;
        }

        return true;
    }

    private int GetDataSize(Frame2ImageSubEntry subEntry, int imageFormat)
    {
        var bitDepth = imageFormat == 3 ? 4 : 8;

        var width = 8 << subEntry.widthShift;
        var height = 8 << subEntry.heightShift;

        return width * height * bitDepth / 8;
    }

    private byte[] ReadPaletteData(BinaryReaderX reader, int colorCount)
    {
        return reader.ReadBytes(colorCount * 2);
    }

    private string[] ReadAnimationNames(BinaryReaderX reader, int count)
    {
        var result = new string[count];

        for (var i = 0; i < count; i++)
        {
            var pos = reader.BaseStream.Position;
            result[i] = reader.ReadNullTerminatedString();

            reader.BaseStream.Position = pos + 0x1E;
        }

        return result;
    }

    private AnimationEntry[] ReadAnimationEntries(BinaryReaderX reader, int count)
    {
        var result = new AnimationEntry[count];

        for (var i = 0; i < count; i++)
            result[i] = ReadAnimationEntry(reader);

        return result;
    }

    private AnimationEntry ReadAnimationEntry(BinaryReaderX reader)
    {
        var count = reader.ReadInt32();

        return new AnimationEntry
        {
            entries = ReadAnimationSubEntries(reader, count)
        };
    }

    private AnimationSubEntry[] ReadAnimationSubEntries(BinaryReaderX reader, int count)
    {
        var result = new AnimationSubEntry[count];

        var nextStepIndexes = ReadIntegers(reader, count);
        var frameCounters = ReadIntegers(reader, count);
        var frameIndexes = ReadIntegers(reader, count);

        for (var i = 0; i < count; i++)
        {
            result[i] = new AnimationSubEntry
            {
                nextStepIndex = nextStepIndexes[i],
                frameCounter = frameCounters[i],
                frameIndex = frameIndexes[i]
            };
        }

        return result;
    }

    private int[] ReadIntegers(BinaryReaderX reader, int count)
    {
        var result = new int[count];

        for (var i = 0; i < count; i++)
            result[i] = reader.ReadInt32();

        return result;
    }

    private bool CheckFileIntegrity(BinaryReaderX reader)
    {
        var count = reader.ReadInt16();
        var format = reader.ReadInt16();
        int colorCount = reader.ReadInt32();

        if (colorCount is 0 or > 256)
            return false;

        for (var i = 0; i < count; i++)
        {
            if (reader.BaseStream.Position + 8 >= reader.BaseStream.Length)
                return false;

            var entry = ReadImageEntry(reader);

            if (entry.partCount >= 0x400)
                return false;

            for (var j = 0; j < entry.partCount; j++)
            {
                if (reader.BaseStream.Position + 0xC >= reader.BaseStream.Length)
                    return false;

                var subEntry = ReadImageSubEntry(reader);
                var dataSize = GetDataSize(subEntry, format);

                if (reader.BaseStream.Position + dataSize >= reader.BaseStream.Length)
                    return false;

                reader.BaseStream.Position += dataSize;
            }
        }

        int size = (colorCount * 2 + 3) & ~3;
        if (reader.BaseStream.Position + size >= reader.BaseStream.Length)
            return false;

        reader.BaseStream.Position += size;

        if (reader.BaseStream.Position + 0x1E >= reader.BaseStream.Length)
            return false;

        var check = reader.ReadBytes(0x1E).Sum(x => x);
        if (check is not 0)
            return false;

        if (reader.BaseStream.Position + 4 >= reader.BaseStream.Length)
            return false;

        var animationCount = reader.ReadInt32();
        if (animationCount is 0)
            return false;

        size = animationCount * 0x1E;
        if (reader.BaseStream.Position + size >= reader.BaseStream.Length)
            return false;

        reader.BaseStream.Position += size;

        if (reader.BaseStream.Position + 4 >= reader.BaseStream.Length)
            return false;

        check = reader.ReadInt32();
        if (check is not 0)
            return false;

        for (var i = 0; i < animationCount - 1; i++)
        {
            if (reader.BaseStream.Position + 4 >= reader.BaseStream.Length)
                return false;

            var stepCount = reader.ReadInt32();
            if (stepCount is 0)
                return false;

            size = stepCount * 0xC;
            if (reader.BaseStream.Position + size > reader.BaseStream.Length)
                return false;

            reader.BaseStream.Position += size;
        }

        if (reader.BaseStream.Position != reader.BaseStream.Length)
        {
            check = reader.ReadInt16();
            if (check != 0x1234)
                return false;
        }

        return true;
    }
}