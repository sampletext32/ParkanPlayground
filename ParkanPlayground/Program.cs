using VarsetLib;


// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\default.scr";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\scr_pl_1.scr";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\scream.scr";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\scream1.scr";
// var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS";
var path = "C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS\\varset.var";
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

var items = VarsetParser.Parse(path);

Console.WriteLine(items.Count);