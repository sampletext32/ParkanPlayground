using System.Buffers.Binary;
using System.Text;using ParkanPlayground;

var libFile = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\ui\\ui_back.lib";

using FileStream nResFs = new FileStream(libFile, FileMode.Open);

Span<byte> buffer = stackalloc byte[4];

nResFs.ReadExactly(buffer);

var nResHeader = BinaryPrimitives.ReadInt32LittleEndian(buffer);

var str = Encoding.ASCII.GetString(buffer);

Console.WriteLine($"NRES Header: {nResHeader:X} - {str}");

// ----
nResFs.ReadExactly(buffer);

var version = BinaryPrimitives.ReadInt32LittleEndian(buffer);

Console.WriteLine($"VERSION: {version:X}");

// ----
nResFs.ReadExactly(buffer);

var elementCount = BinaryPrimitives.ReadInt32LittleEndian(buffer);

Console.WriteLine($"ElementCount: {elementCount}");

// ----
nResFs.ReadExactly(buffer);

var totalLength = BinaryPrimitives.ReadInt32LittleEndian(buffer);

Console.WriteLine($"TOTAL_LENGTH: {totalLength}");

// ----

nResFs.Seek(-elementCount * 64, SeekOrigin.End);

_ = 5;

var elements = new List<ListMetadataItem>(elementCount);

Span<byte> metaDataBuffer = stackalloc byte[64];
for (int i = 0; i < elementCount; i++)
{
    nResFs.ReadExactly(metaDataBuffer);

    var itemType = Encoding.ASCII.GetString(metaDataBuffer.Slice(0, 8));

    var itemLength = BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer.Slice(12, 4));

    var fileNameBlock = metaDataBuffer.Slice(20, 20);
    var len = fileNameBlock.IndexOf((byte)'\0');
    if (len == -1) len = 20; // whole 20 bytes is a filename
    var fileName = Encoding.ASCII.GetString(fileNameBlock.Slice(0, len));

    var offsetInFile = BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer.Slice(56, 4));

    var lastMagicNumber = BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer.Slice(60, 4));

    Console.WriteLine(
        $"File {i+1}: \n" +
        $"\tType: {itemType}\n" +
        $"\tItemLength: {itemLength}\n" +
        $"\tFileName: {fileName}\n" +
        $"\tOffsetInFile: {offsetInFile}\n" +
        $"\tLastMagicNumber: {lastMagicNumber}"
    );
    
    elements.Add(new ListMetadataItem(itemType, itemLength, fileName, offsetInFile));
    
    metaDataBuffer.Clear();
}

var libFileName = new FileInfo(libFile).Name;

if (Directory.Exists(libFileName))
{
    Directory.Delete(libFileName, true);
}

var dir = Directory.CreateDirectory(libFileName);

byte[] copyBuffer = new byte[8192];

foreach (var element in elements)
{
    nResFs.Seek(element.OffsetInFile, SeekOrigin.Begin);
    using var destFs = new FileStream(Path.Combine(libFileName, element.FileName), FileMode.CreateNew);

    var totalCopiedBytes = 0;
    while (totalCopiedBytes < element.ItemLength)
    {
        var needReadBytes = Math.Min(element.ItemLength - totalCopiedBytes, copyBuffer.Length);
        var readBytes = nResFs.Read(copyBuffer, 0, needReadBytes);
        
        destFs.Write(copyBuffer, 0, readBytes);

        totalCopiedBytes += readBytes;
    }
}