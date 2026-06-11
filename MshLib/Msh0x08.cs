using Common;
using NResLib;

namespace MshLib;

public static class Msh0x08
{
    public static List<MshTransformKeyframe> ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "08 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain animation descriptor component (0x08)");
        }

        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);

        var descriptors = new List<MshTransformKeyframe>();

        for (var i = 0; i < entry.ElementCount; i++)
        {
            descriptors.Add(new MshTransformKeyframe(
                new Vector3(mshFs.ReadFloatLittleEndian(),
                    mshFs.ReadFloatLittleEndian(),
                    mshFs.ReadFloatLittleEndian()),
                mshFs.ReadFloatLittleEndian(),
                new UShortQuaternion(
                    mshFs.ReadInt16LittleEndian(),
                    mshFs.ReadInt16LittleEndian(),
                    mshFs.ReadInt16LittleEndian(),
                    mshFs.ReadInt16LittleEndian()
                )
            ));
        }

        return descriptors;
    }
}

public record MshTransformKeyframe(
    Vector3 Position,
    float Time,
    UShortQuaternion Rotation
);
