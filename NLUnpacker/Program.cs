using System.Buffers.Binary;
using System.Text;

// Unfinished

var fileBytes = File.ReadAllBytes("C:\\Program Files (x86)\\Nikita\\Iron Strategy\\gamefont-1.rlb");

var header = fileBytes.AsSpan().Slice(0, 32);

var nlHeaderBytes = header.Slice(0, 2);
var mustBeZero = header[2];
var mustBeOne = header[3];
var numberOfEntriesBytes = header.Slice(4, 2);
var sortingFlagBytes = header.Slice(14, 2);
var decryptionKeyBytes = header.Slice(20, 2);

var numberOfEntries = BinaryPrimitives.ReadInt16LittleEndian(numberOfEntriesBytes);
var sortingFlag = BinaryPrimitives.ReadInt16LittleEndian(sortingFlagBytes);
var decryptionKey = BinaryPrimitives.ReadInt16LittleEndian(decryptionKeyBytes);

var headerSize = numberOfEntries * 32;

var decryptedHeader = new byte[headerSize];

var keyLow = decryptionKeyBytes[0];
var keyHigh = decryptionKeyBytes[1];
for (var i = 0; i < headerSize; i++)
{
    byte tmp = (byte)((keyLow << 1) ^ keyHigh);
    keyLow = tmp;
    keyHigh = (byte)((keyHigh >> 1) ^ tmp);
    decryptedHeader[i] = (byte)(fileBytes[32 + i] ^ tmp);
}

var decryptedHeaderString = Encoding.ASCII.GetString(decryptedHeader, 0, headerSize);
var entries = decryptedHeader.Chunk(32).ToArray();
var entriesStrings = entries.Select(x => Encoding.ASCII.GetString(x, 0, x.Length)).ToArray();

File.WriteAllBytes("export.nl", decryptedHeader);
var fileCount = BinaryPrimitives.ReadInt16LittleEndian(fileBytes.AsSpan().Slice(4, 2));

var decodedHeader = new byte[fileCount * 32];

byte key1 = fileBytes[20];
byte key2 = fileBytes[21];
var decodeIndex = 0;

Console.WriteLine($"Keys: {key1} {key2}");

Console.WriteLine("Iteration " + decodeIndex.ToString("00") + "| " + string.Join(" ", decodedHeader.Take(20).Select(x => x.ToString("X2"))));

do
{

    key1 = (byte) (key2 ^ (key1 << 1));
    key2 = (byte) (key1 ^ (key2 >> 1));
    decodedHeader[decodeIndex] = (byte) (key1 ^ fileBytes[decodeIndex + 32]);
    Console.WriteLine($"Keys: {key1} {key2}");
    Console.WriteLine("Iteration " + decodeIndex.ToString("00") + "| " + string.Join(" ", decodedHeader.Take(20).Select(x => x.ToString("X2"))));
    decodeIndex++;
} while (decodeIndex < fileCount * 32);

File.WriteAllBytes("encoding_table.bin", decodedHeader);