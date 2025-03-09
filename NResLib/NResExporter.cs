namespace NResLib;

public class NResExporter
{
    public static void Export(NResArchive archive, string directory, string nResPath)
    {
        var openedFileName = Path.GetFileName(nResPath)!;
        var targetDirectoryPath = Path.Combine(directory, openedFileName);

        if (!Directory.Exists(targetDirectoryPath))
        {
            Directory.CreateDirectory(targetDirectoryPath);
        }

        using var fs = new FileStream(nResPath, FileMode.Open);

        foreach (var archiveFile in archive.Files)
        {
            fs.Seek(archiveFile.OffsetInFile, SeekOrigin.Begin);

            var buffer = new byte[archiveFile.FileLength];
            
            fs.ReadExactly(buffer, 0, archiveFile.FileLength);

            var extension = Path.GetExtension(archiveFile.FileName);
            var fileName = Path.GetFileNameWithoutExtension(archiveFile.FileName);

            if (extension == "")
            {
                extension = ".bin";
            }
            
            var targetFilePath = Path.Combine(targetDirectoryPath, $"{archiveFile.Index}_{archiveFile.FileType}_{fileName}{extension}");
            
            File.WriteAllBytes(targetFilePath, buffer);
        }
    }
}