using System.Text.Json;
using Common;
using ControlLib;
using MshLib;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
//
var ctl = ControlResourceReader.ReadCtlFile("E:\\ParkanUnpacked\\bases.rlb\\2_CTLD_r_l_03.ctl");
var cpt = ControlResourceReader.ReadCptFile("E:\\ParkanUnpacked\\bases.rlb\\18_CTPT_R_L_03.CPT");
var ndp = ControlResourceReader.ReadNdpFile("E:\\ParkanUnpacked\\bases.rlb\\14_NDPR_r_l_03.ndp");

// var ctl = ControlResourceReader.ReadCtlFile("E:\\ParkanUnpacked\\turrets.rlb\\30_CTLD_o_tur_lt_02.ctl");
// var cpt = ControlResourceReader.ReadCptFile("E:\\ParkanUnpacked\\turrets.rlb\\162_CTPT_o_tur_lt_02.cpt");
// var ndp = ControlResourceReader.ReadNdpFile("E:\\ParkanUnpacked\\turrets.rlb\\119_NDPR_o_tur_la_02.ndp");

// var ctl = ControlResourceReader.ReadCtlFile("E:\\ParkanUnpacked\\guns.rlb\\86_CTLD_o_gun_la_01.ctl");
// var cpt = ControlResourceReader.ReadCptFile("E:\\ParkanUnpacked\\guns.rlb\\364_CTPT_o_gun_la_01.cpt");
// var ndp = ControlResourceReader.ReadNdpFile("E:\\ParkanUnpacked\\guns.rlb\\309_NDPR_o_gun_la_01.ndp");

ControlResourceDump.DumpAll(ctl, cpt, ndp, Console.Out);

// foreach (var item in ctl.ItemRecords)
// {
//     Console.WriteLine(
//         $"0x{item.Offset:X4}-0x{item.EndOffset:X4} " +
//         $"type={item.ItemType:X} piece={item.LocalPieceIndex} " +
//         $"name='{item.ItemName}' bindings=[{string.Join(", ", item.PieceBindingIndices)}]");
// }