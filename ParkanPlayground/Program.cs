using NResLib;
using ResTreeLib;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

foreach (var trfFile in Directory.EnumerateFiles("C:\\Program Files (x86)\\Nikita\\Iron Strategy\\MISSIONS\\SCRIPTS", "*.trf"))
{
    
    using var fs = new FileStream(trfFile, FileMode.Open, FileAccess.Read, FileShare.Read);
    
    var nres = NResParser.ReadFile(trfFile);
    
    var resTree = ResTreeParser.Parse(nres.Archive!, fs);
    Console.WriteLine(trfFile);
    _ = 5;
}