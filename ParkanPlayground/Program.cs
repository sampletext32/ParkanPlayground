using System.Buffers.Binary;
using System.Numerics;
using System.Text.Json;
using ScrLib;
using VarsetLib;


// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\default.scr";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\scr_pl_1.scr";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\scream.scr";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\scream1.scr";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\varset.var";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\preload.lda";
//
// var fs = new FileStream(path, FileMode.Open);
//
// var count = fs.ReadInt32LittleEndian();
//
// Span<byte> data = stackalloc byte[0x124];
//
// for (var i = 0; i < count; i++)
// {
//     fs.ReadExactly(data);
// }
//
// Console.WriteLine(
//     fs.Position == fs.Length
// );

// var items = VarsetParser.Parse(path);

// Console.WriteLine(items.Count);

// Span<byte> flt = stackalloc byte[4];
// flt[0] = 0x7f;
// flt[1] = 0x7f;
// flt[2] = 0xff;
// flt[3] = 0xff;
// var f = BinaryPrimitives.ReadSingleBigEndian(flt);
//
// Console.WriteLine(f);

// return;

// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MisLoad.dll";
var path = "C:\\ParkanUnpacked\\Land.msh\\2_03 00 00 00_Land.bin";

var fs = new FileStream(path, FileMode.Open);
var outputFs = new FileStream("Land.obj", FileMode.Create);
var sw = new StreamWriter(outputFs);

List<Vector3D> points = [];
var count = 0;
while (fs.Position < fs.Length)
{
    var x = fs.ReadFloatLittleEndian();
    var y = fs.ReadFloatLittleEndian();
    var z = fs.ReadFloatLittleEndian();

    var vertex = new Vector3D(x, y, z);
    sw.WriteLine($"v {x} {y} {z}");

    var seenIndex = points.FindIndex(vec => vec == vertex);
    if (seenIndex != -1)
    {
        vertex.Duplicates = seenIndex;
    }
    
    points.Add(vertex);
    count++;
}

File.WriteAllText("human-readable.json", JsonSerializer.Serialize(points, new JsonSerializerOptions()
{
    WriteIndented = true
}));

Console.WriteLine($"Total vertices: {count}");


// for (int i = 0; i < count / 4; i++)

public record Vector3D(float X, float Y, float Z)
{
    public int Duplicates { get; set; }
}
// var indices = string.Join(" ", Enumerable.Range(1, count));
//
// sw.WriteLine($"l {indices}");

//
// fs.Seek(0x1000, SeekOrigin.Begin);
//
// byte[] buf = new byte[34];
// fs.ReadExactly(buf);
//
// var disassembler = new SharpDisasm.Disassembler(buf, ArchitectureMode.x86_32);
// foreach (var instruction in disassembler.Disassemble())
// {
//     Console.WriteLine($"{instruction.PC - instruction.Offset}: {instruction}");
//
//     new Instruction()
//     {
//         Action = instruction.Mnemonic.ToString(),
//         Arguments = {instruction.Operands[0].ToString()}
//     };
// }

public class Instruction
{
    public string Action { get; set; } = "";

    public List<string> Arguments { get; set; } = [];
}
