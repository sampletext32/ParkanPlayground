using System.Buffers.Binary;

var fileBytes = File.ReadAllBytes("C:\\Program Files (x86)\\Nikita\\Iron Strategy\\gamefont.rlb");

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