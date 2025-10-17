using Komponent.IO;
using Logic.Domain.Level5Management.Contract.Animations;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using Logic.Domain.Level5Management.DataClasses.Animations;

namespace Logic.Domain.Level5Management.Animations;

class Frame3Reader : IFrame3Reader
{
    public Frame3Container Read(Stream input)
    {
        using var reader = new BinaryReaderX(input, true);

        bool isValid = CheckFileIntegrity(reader);
        input.Position = 0x1E;

        short frameCount = reader.ReadInt16();
        short imageFormat = reader.ReadInt16();

        var context = new AnimationReaderContext
        {
            FrameCount = frameCount,
            ImageFormat = imageFormat,
            IsValid = isValid
        };

        var frames = ReadImageEntries(reader, context);

        input.Position += 0x1E;

        var animationCount = reader.ReadInt32();
        var animationNames = ReadAnimationNames(reader, animationCount);

        input.Position += 4;

        var animations = ReadAnimationEntries(reader, animationCount - 1);

        return new Frame3Container
        {
            ImageFormat = imageFormat,
            ImageEntries = frames,
            AnimationNames = animationNames,
            AnimationEntries = animations
        };
    }

    private Frame3ImageEntry[] ReadImageEntries(BinaryReaderX reader, AnimationReaderContext context)
    {
        var result = new Frame3ImageEntry[context.FrameCount];

        for (var i = 0; i < context.FrameCount; i++)
        {
            context.FrameIndex = i;

            result[i] = ReadImageEntry(reader);

            context.FrameWidth = result[i].width;
            context.FrameHeight = result[i].height;

            result[i].imageFormat = context.IsValid ? context.ImageFormat : DetectImageFormat(reader, result[i], context);

            var dataSize = GetDataSize(result[i], result[i].imageFormat);
            result[i].data = reader.ReadBytes(dataSize);
        }

        return result;
    }

    private Frame3ImageEntry ReadImageEntry(BinaryReaderX reader)
    {
        var width = reader.ReadInt32();
        var height = reader.ReadInt32();
        var unk = reader.ReadInt16();

        var entry = new Frame3ImageEntry
        {
            width = width,
            height = height,
            unk = unk
        };

        return entry;
    }

    private int DetectImageFormat(BinaryReaderX reader, Frame3ImageEntry subEntry, AnimationReaderContext context)
    {
        long bkPos = reader.BaseStream.Position;

        for (var i = 0; i < 1; i++)
        {
            bool wasSuccessful = ApplyImageFormat(reader, subEntry, context, i);
            reader.BaseStream.Position = bkPos;

            if (wasSuccessful)
                return i;
        }

        throw new InvalidOperationException("Could not determine image format.");
    }

    private bool ApplyImageFormat(BinaryReaderX reader, Frame3ImageEntry entry, AnimationReaderContext context, int imageFormat)
    {
        var dataSize = GetDataSize(entry, imageFormat);
        reader.BaseStream.Position += dataSize;

        if (context.FrameIndex + 1 < context.FrameCount)
        {
            var nextFrame = ReadImageEntry(reader);

            if (nextFrame.width is <= 0 or > 256
                || nextFrame.height is <= 0 or > 256
                || nextFrame.unk == 0
                || nextFrame.unk >= 0x400)
                return false;
        }
        else
        {
            reader.BaseStream.Position += 0x1E;
            var animationCount = reader.ReadInt32();

            if (animationCount is <= 0 or >= 64)
                return false;
        }

        return true;
    }

    private int GetDataSize(Frame3ImageEntry entry, int imageFormat)
    {
        int bitDepth = imageFormat switch
        {
            0 => 16,
            _ => throw new InvalidOperationException("Unsupported image format.")
        };

        return entry.width * entry.height * bitDepth / 8;
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
        var check = reader.ReadBytes(0x1E).Sum(x => x);
        if (check is not 0)
            return false;

        var count = reader.ReadInt16();
        var format = reader.ReadInt16();

        for (var i = 0; i < count; i++)
        {
            if (reader.BaseStream.Position + 0xA >= reader.BaseStream.Length)
                return false;

            var entry = ReadImageEntry(reader);

            if (entry.unk is 0)
                return false;

            var dataSize = GetDataSize(entry, format);

            if (reader.BaseStream.Position + dataSize >= reader.BaseStream.Length)
                return false;

            reader.BaseStream.Position += dataSize;
        }

        if (reader.BaseStream.Position + 0x1E >= reader.BaseStream.Length)
            return false;

        check = reader.ReadBytes(0x1E).Sum(x => x);
        if (check is not 0)
            return false;

        if (reader.BaseStream.Position + 4 >= reader.BaseStream.Length)
            return false;

        var animationCount = reader.ReadInt32();
        if (animationCount is 0)
            return false;

        int size = animationCount * 0x1E;
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