using System.Buffers.Binary;
using System.Runtime.CompilerServices;

// ПОКА НЕ ПОНЯТНО КАК ОНО КОДИРУЕТСЯ

// using var fs = new FileStream("gamefont.rlb", FileMode.Open, FileAccess.ReadWrite);
//
// int fileCount, c, i;
// byte b, n;
//
// fs.Seek(4, SeekOrigin.Current);
//
// Span<byte> buf2 = stackalloc byte[2];
// Span<byte> buf4 = stackalloc byte[4];
//
// fs.ReadExactly(buf2);
//
// fileCount = BinaryPrimitives.ReadInt16LittleEndian(buf2);
//
// fs.Seek(16 - 2, SeekOrigin.Current);
//
// b = (byte) fs.ReadByte();
// c = (byte) fs.ReadByte();
//
// fs.Seek(12 - 2, SeekOrigin.Current);
//
// Span<byte> buf1 = stackalloc byte[1];
//
// if (32 * fileCount > 0)
// {
//     for (i = 0; i < 32 * fileCount; ++i)
//     {
//         b = (byte) ((b * 2) ^ c);
//         n = (byte) c;
//         c = (int) ((n >> 1) ^ b);
//
//         // Read, modify, and write the byte at the current position
//         byte originalByte = (byte) fs.ReadByte();
//         fs.Seek(-1, SeekOrigin.Current); // Move back one byte to overwrite it
//         buf1[0] = (byte) (originalByte ^ b);
//         fs.Write(buf1);
//     }
// }

int v40 = 0;
var flags = 0;
var file_bytes = File.ReadAllBytes("gamefont.rlb");
var FileSize = file_bytes.Length;
var file_bytes_copy = file_bytes;
var v8 = flags;
if ((flags & 2) != 0)
{
    if (file_bytes[FileSize - 6] != 'A' && file_bytes[FileSize - 5] != 'O')
        throw new Exception("4");
    v40 = BinaryPrimitives.ReadInt32LittleEndian(
        file_bytes.Skip(FileSize - 4)
            .Take(4)
            .ToArray()
    );
}

if (file_bytes[0] != (byte)'N' || file_bytes[1] != (byte)'L' || file_bytes[2] != 0 || file_bytes[3] != 1)
    throw new Exception("4");
var file_count = BinaryPrimitives.ReadInt16LittleEndian(
    file_bytes.Skip(4)
        .Take(2)
        .ToArray()
);
var file_count_copy = file_count;
var section_mem_ptr = new byte[32 * file_count];

var section_count = 0;
var sixth_byte = file_bytes_copy[5];
var sixth_byte_shifted_by_1_byte = file_bytes_copy[5] >> 8;
if (32 * file_count > 0)
{
    do
    {
        sixth_byte = (byte) (sixth_byte_shifted_by_1_byte ^ (2 * sixth_byte));
        sixth_byte_shifted_by_1_byte = sixth_byte ^ (sixth_byte_shifted_by_1_byte >> 1);
        section_mem_ptr[section_count] = (byte) (sixth_byte ^ file_bytes_copy[section_count + 32]);
        ++section_count;
    } while (section_count < 32 * file_count);

    v8 = flags;
}

for (var i = 0; i < section_mem_ptr.Length; i++)
{
    
}

File.WriteAllBytes("gamefont-dump.rlb", file_bytes);