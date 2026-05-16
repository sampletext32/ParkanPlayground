using System.Buffers.Binary;
using Common;
using NResLib;

namespace ParkanPlayground;

public static class Msh0x08
{
    public static List<AnimationDescriptor> ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "08 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain animation descriptor component (0x08)");
        }

        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);

        var descriptors = new List<AnimationDescriptor>();

        for (var i = 0; i < entry.ElementCount; i++)
        {
            descriptors.Add(new AnimationDescriptor(
                new Vector3(mshFs.ReadFloatLittleEndian(),
                    mshFs.ReadFloatLittleEndian(),
                    mshFs.ReadFloatLittleEndian()),
                mshFs.ReadFloatLittleEndian(),
                new UShortQuaternion(
                    mshFs.ReadUInt16LittleEndian(),
                    mshFs.ReadUInt16LittleEndian(),
                    mshFs.ReadUInt16LittleEndian(),
                    mshFs.ReadUInt16LittleEndian()
                )
            ));
        }

        return descriptors;
    }

    public record AnimationDescriptor(
        Vector3 Position,
        float Time,
        UShortQuaternion Rotation
    );
}
