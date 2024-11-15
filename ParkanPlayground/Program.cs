using System.Buffers.Binary;
using System.Text;
using NResLib;

var libFile = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\ui\\ui_back.lib";

var parseResult = NResParser.ReadFile(libFile);

if (parseResult.Error != null)
{
    Console.WriteLine(parseResult.Error);
    return;
}

// var libFileName = new FileInfo(libFile).Name;
//
// if (Directory.Exists(libFileName))
// {
//     Directory.Delete(libFileName, true);
// }
//
// var dir = Directory.CreateDirectory(libFileName);
//
// byte[] copyBuffer = new byte[8192];
//
// foreach (var element in elements)
// {
//     nResFs.Seek(element.OffsetInFile, SeekOrigin.Begin);
//     using var destFs = new FileStream(Path.Combine(libFileName, element.FileName), FileMode.CreateNew);
//
//     var totalCopiedBytes = 0;
//     while (totalCopiedBytes < element.ItemLength)
//     {
//         var needReadBytes = Math.Min(element.ItemLength - totalCopiedBytes, copyBuffer.Length);
//         var readBytes = nResFs.Read(copyBuffer, 0, needReadBytes);
//         
//         destFs.Write(copyBuffer, 0, readBytes);
//
//         totalCopiedBytes += readBytes;
//     }
// }