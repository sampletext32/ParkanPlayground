using System.Buffers.Binary;
using Common;
using MissionTmaLib.Parsing;
using NResLib;
using ParkanPlayground;
using VarsetLib;

// var cpDatEntryConverter = new CpDatEntryConverter();
// cpDatEntryConverter.Convert();

// var converter = new MshConverter();

// converter.Convert("E:\\ParkanUnpacked\\fortif.rlb\\133_fr_m_bunker.msh");
// converter.Convert("C:\\Program Files (x86)\\Nikita\\Iron Strategy\\DATA\\MAPS\\SC_1\\Land.msh");
// converter.Convert("E:\\ParkanUnpacked\\fortif.rlb\\73_fr_m_brige.msh");
// converter.Convert("E:\\ParkanUnpacked\\intsys.rlb\\277_MESH_o_pws_l_01.msh");
// converter.Convert("E:\\ParkanUnpacked\\static.rlb\\2_MESH_s_stn_0_01.msh");
// converter.Convert("E:\\ParkanUnpacked\\bases.rlb\\25_MESH_R_H_02.msh");


foreach (var path in Directory.EnumerateFiles("E:\\ParkanUnpacked\\behpsp.res"))
{
    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

    var file = BinaryVarsetFileParser.Parse(fs);

    _ = 5;
}

{
    var fs = new FileStream("E:\\ParkanUnpacked\\behpsp.res\\31_00 00 00 00_prof_generator.var", FileMode.Open,
        FileAccess.Read, FileShare.Read);

    var file = BinaryVarsetFileParser.Parse(fs);

    _ = 5;
}