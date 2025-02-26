using System.Buffers.Binary;
using System.Text;
using NResLib;
using ParkanPlayground;

// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\default.scr";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\scr_pl_1.scr";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\scream.scr";
var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\scream1.scr";

using var fs = new FileStream(path, FileMode.Open);

// тут всегда число 59 (0x3b) - это число известных игре скриптов
var magic = fs.ReadInt32LittleEndian();

Console.WriteLine($"Count: {magic}");

var entryCount = fs.ReadInt32LittleEndian();

Console.WriteLine($"EntryCount: {entryCount}");

for (var i = 0; i < entryCount; i++)
{
    Console.WriteLine($"Entry: {i}");
    var str = fs.ReadLengthPrefixedString();
    
    Console.WriteLine($"\tStr: {str}");
    
    // тут игра дополнительно вычитывает ещё 1 байт, видимо как \0 для char*
    fs.ReadByte();

    var index = fs.ReadInt32LittleEndian();
    Console.WriteLine($"\tIndex: {index}");
    var innerCount = fs.ReadInt32LittleEndian();
    Console.WriteLine($"\tInnerCount: {innerCount}");
    for (var i1 = 0; i1 < innerCount; i1++)
    {
        var scriptIndex = fs.ReadInt32LittleEndian();
        var unkInner2 = fs.ReadInt32LittleEndian();
        var unkInner3 = fs.ReadInt32LittleEndian();
        var unkInner4 = fs.ReadInt32LittleEndian();
        var unkInner5 = fs.ReadInt32LittleEndian();
        
        Console.WriteLine($"\t\tScriptIndex: {scriptIndex}");
        Console.WriteLine($"\t\tUnkInner2: {unkInner2}");
        Console.WriteLine($"\t\tUnkInner3: {unkInner3}");
        Console.WriteLine($"\t\tUnkInner4: {unkInner4}");
        Console.WriteLine($"\t\tUnkInner5: {unkInner5}");
        
        var scriptArgumentsCount = fs.ReadInt32LittleEndian();
        Console.WriteLine($"\t\tScript Arguments Count: {scriptArgumentsCount}");
        
        for (var i2 = 0; i2 < scriptArgumentsCount; i2++)
        {
            var scriptArgument = fs.ReadInt32LittleEndian();
            Console.WriteLine($"\t\t\t{scriptArgument}");
        }

        var unkInner7 = fs.ReadInt32LittleEndian();
        
        Console.WriteLine($"\t\tUnkInner7 {unkInner7}");
        Console.WriteLine("---");
    }
}
