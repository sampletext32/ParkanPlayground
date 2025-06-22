using System.Buffers.Binary;
using System.Text;

namespace NResLib;

public class NResPacker
{
    public static string Pack(NResArchive archive, string srcNresPath, string contentDirectoryPath, string targetFileDirectoryPath)
    {
        var diskFiles = Directory.GetFiles(contentDirectoryPath)
            .Select(Path.GetFileName)
            .ToList();

        var fileOffset = 16; // 16 по умолчанию, т.к. есть заголовок в 16 байт.

        var metadataItems = new List<ListMetadataItem>();

        foreach (var archiveFile in archive.Files)
        {
            var extension = Path.GetExtension(archiveFile.FileName);
            var fileName = Path.GetFileNameWithoutExtension(archiveFile.FileName);

            if (extension == "")
            {
                extension = ".bin";
            }
            
            var targetFileName = $"{archiveFile.Index}_{archiveFile.FileType}_{fileName}{extension}";

            if (diskFiles.All(x => x != targetFileName))
            {
                return $"Не найдён файл {targetFileName}";
            }

            var filePath = Path.Combine(contentDirectoryPath, targetFileName);

            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
            {
                throw new Exception();
            }

            var newFileLength = (int)fileInfo.Length;
            
            var listItem = new ListMetadataItem(
                archiveFile.FileType,
                archiveFile.ElementCount,
                archiveFile.Magic1,
                newFileLength,
                archiveFile.ElementSize,
                archiveFile.FileName,
                archiveFile.Magic3,
                archiveFile.Magic4,
                archiveFile.Magic5,
                archiveFile.Magic6,
                fileOffset,
                archiveFile.Index
            );

            fileOffset += newFileLength;
            
            metadataItems.Add(listItem);
        }
        
        var totalFileLength = 
            16 + // заголовок 
            metadataItems.Sum(x => x.FileLength) + // сумма длин всех файлов 
            metadataItems.Count * 64; // длина всех метаданных
        
        var header = new NResArchiveHeader(archive.Header.NRes, archive.Header.Version, archive.Header.FileCount, totalFileLength);

        var targetArchive = new NResArchive(header, metadataItems);

        // имя архива = имени папки в которую архив распаковывали
        string targetArchiveFileName = Path.GetFileName(srcNresPath)!;
        
        var targetArchivePath = Path.Combine(targetFileDirectoryPath, targetArchiveFileName);
        
        using var fs = new FileStream(targetArchivePath, FileMode.CreateNew);

        Span<byte> span = stackalloc byte[4];
        
        span.Clear();
        Encoding.ASCII.GetBytes(header.NRes, span);
        fs.Write(span);
        BinaryPrimitives.WriteInt32LittleEndian(span, header.Version);
        fs.Write(span);
        BinaryPrimitives.WriteInt32LittleEndian(span, header.FileCount);
        fs.Write(span);
        BinaryPrimitives.WriteInt32LittleEndian(span, header.TotalFileLengthBytes);
        fs.Write(span);

        foreach (var archiveFile in targetArchive.Files)
        {
            var extension = Path.GetExtension(archiveFile.FileName);
            var fileName = Path.GetFileNameWithoutExtension(archiveFile.FileName);

            if (extension == "")
            {
                extension = ".bin";
            }

            var targetFileName = $"{archiveFile.Index}_{archiveFile.FileType}_{fileName}{extension}";
            
            var filePath = Path.Combine(contentDirectoryPath, targetFileName);
            using var srcFs = new FileStream(filePath, FileMode.Open);
            
            srcFs.CopyTo(fs);
        }

        Span<byte> fileNameSpan = stackalloc byte[20];

        foreach (var archiveFile in targetArchive.Files)
        {
            span.Clear();
            Encoding.ASCII.GetBytes(archiveFile.FileType, span);
            fs.Write(span);
            
            BinaryPrimitives.WriteUInt32LittleEndian(span, archiveFile.ElementCount);
            fs.Write(span);
            BinaryPrimitives.WriteInt32LittleEndian(span, archiveFile.Magic1);
            fs.Write(span);
            BinaryPrimitives.WriteInt32LittleEndian(span, archiveFile.FileLength);
            fs.Write(span);
            BinaryPrimitives.WriteInt32LittleEndian(span, archiveFile.ElementSize);
            fs.Write(span);
            
            fileNameSpan.Clear();
            Encoding.ASCII.GetBytes(archiveFile.FileName, fileNameSpan);
            fs.Write(fileNameSpan);

            BinaryPrimitives.WriteInt32LittleEndian(span, archiveFile.Magic3);
            fs.Write(span);
            BinaryPrimitives.WriteInt32LittleEndian(span, archiveFile.Magic4);
            fs.Write(span);
            BinaryPrimitives.WriteInt32LittleEndian(span, archiveFile.Magic5);
            fs.Write(span);
            BinaryPrimitives.WriteInt32LittleEndian(span, archiveFile.Magic6);
            fs.Write(span);
            BinaryPrimitives.WriteInt32LittleEndian(span, archiveFile.OffsetInFile);
            fs.Write(span);
            BinaryPrimitives.WriteInt32LittleEndian(span, archiveFile.Index);
            fs.Write(span);
        }

        fs.Flush();
        
        return "Запакован архив";
    }
}