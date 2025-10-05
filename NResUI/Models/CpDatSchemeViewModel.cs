using CpDatLib;
using ScrLib;

namespace NResUI.Models;

public class CpDatSchemeViewModel
{
    public bool HasFile { get; set; }
    public string? Error { get; set; }

    public CpDatScheme? CpDatScheme { get; set; }

    public List<(int Level, CpDatEntry Entry)> FlatList { get; set; } = [];

    public string? Path { get; set; }

    public void SetParseResult(CpDatParseResult parseResult, string path)
    {
        CpDatScheme = parseResult.Scheme;
        Error = parseResult.Error;
        HasFile = true;
        Path = path;

        if (CpDatScheme is not null)
        {
            RebuildFlatList();
        }
    }

    public void RebuildFlatList()
    {
        FlatList = [];

        if (CpDatScheme is null)
        {
            return;
        }
            
        CollectEntries(CpDatScheme.Root, 0);
            
        void CollectEntries(CpDatEntry entry, int level)
        {
            FlatList.Add((level, entry));
            foreach (var child in entry.Children)
            {
                CollectEntries(child, level + 1);
            }
        }
    }
}