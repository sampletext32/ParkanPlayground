using System.Text;
using System.Text.Json;
using Common;
using ControlLib;
using MshLib;
using NResLib;
using WeaLib;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
//
// var ctl = ControlResourceReader.ReadCtlFile("E:\\ParkanUnpacked\\bases.rlb\\2_CTLD_r_l_03.ctl");
// var cpt = ControlResourceReader.ReadCptFile("E:\\ParkanUnpacked\\bases.rlb\\18_CTPT_R_L_03.CPT");
// var ndp = ControlResourceReader.ReadNdpFile("E:\\ParkanUnpacked\\bases.rlb\\14_NDPR_r_l_03.ndp");

// var ctl = ControlResourceReader.ReadCtlFile("E:\\ParkanUnpacked\\turrets.rlb\\30_CTLD_o_tur_lt_02.ctl");
// var cpt = ControlResourceReader.ReadCptFile("E:\\ParkanUnpacked\\turrets.rlb\\162_CTPT_o_tur_lt_02.cpt");
// var ndp = ControlResourceReader.ReadNdpFile("E:\\ParkanUnpacked\\turrets.rlb\\119_NDPR_o_tur_la_02.ndp");

// var ctl = ControlResourceReader.ReadCtlFile("E:\\ParkanUnpacked\\guns.rlb\\86_CTLD_o_gun_la_01.ctl");
// var cpt = ControlResourceReader.ReadCptFile("E:\\ParkanUnpacked\\guns.rlb\\364_CTPT_o_gun_la_01.cpt");
// var ndp = ControlResourceReader.ReadNdpFile("E:\\ParkanUnpacked\\guns.rlb\\309_NDPR_o_gun_la_01.ndp");

// ControlResourceDump.DumpAll(ctl, cpt, ndp, Console.Out);

List<string> allNres =
[
    "animals.rlb",
    "bases.rlb",
    "behpsp.res",
    "effects.rlb",
    "fortif.rlb",
    // "gamefont.rlb", // NL
    "guns.rlb",
    "intsys.rlb",
    "lightmap.lib",
    "Material.lib",
    "objects.dlb",
    "objects.rlb",
    "Palettes.lib",
    "parts.rlb",
    "static.rlb",
    "sys.lib",
    "system.rlb",
    "Textures.lib",
    "turrets.rlb",
    "voices.lib",
    "weapon.rlb"
];

// CONFIRMED:
// all .msh have .wea in the same archive with the same name, so no cross-referencing

foreach (var nres in allNres)
{
    var path = Path.Combine("C:\\IronStrategy", nres);

    using var nResFs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    var archive = NResParser.ReadFile(nResFs).Archive!;
    
    foreach (var archiveFile in archive.Files)
    {
        if (archiveFile.FileType != "MESH") continue;

        // Console.WriteLine($"Found mesh {archiveFile.FileName} in {nres}");

        var entry = Path.GetFileNameWithoutExtension(archiveFile.FileName);
        var weaEntry = archive.Files.FirstOrDefault(x => x.FileType == "WEAR" &&
                                                         string.Equals(x.FileName, $"{entry}.wea",
                                                             StringComparison.OrdinalIgnoreCase));
        if (weaEntry is null)
        {
            Console.WriteLine($"Mesh {archiveFile.FileName} in {nres} has no WEA");
            continue;
        }

        // Console.WriteLine($"Has matching wear {weaEntry.FileName}");
        
        var weaData = new byte[weaEntry.FileLength];

        nResFs.Seek(weaEntry.OffsetInFile, SeekOrigin.Begin);
        nResFs.ReadExactly(weaData, 0, weaData.Length);

        var weaContent = Encoding.UTF8.GetString(weaData);
        var wea = WeaParser.ReadText(weaContent);

        _ = 5;
    }
}

// foreach (var item in ctl.ItemRecords)
// {
//     Console.WriteLine(
//         $"0x{item.Offset:X4}-0x{item.EndOffset:X4} " +
//         $"type={item.ItemType:X} piece={item.LocalPieceIndex} " +
//         $"name='{item.ItemName}' bindings=[{string.Join(", ", item.PieceBindingIndices)}]");
// }
