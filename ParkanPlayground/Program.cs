using System.Buffers.Binary;
using Common;
using MissionTmaLib.Parsing;
using NResLib;
using ParkanPlayground;

// var cpDatEntryConverter = new CpDatEntryConverter();
// cpDatEntryConverter.Convert();

var converter = new MshConverter();

converter.Convert("E:\\ParkanUnpacked\\fortif.rlb\\133_fr_m_bunker.msh");