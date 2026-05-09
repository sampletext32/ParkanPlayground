namespace ResTreeLib;

// TRF0 node payload (40 bytes).
// Field naming for the first 16 bytes is based on empirical checks over the mission .trf set.
// ResearchCost/ResearchTime look gameplay-related (strong mutual correlation and upgrade-level scaling).
// ViewPosX/ViewPosY behave like 2D layout coordinates; very large Y outliers are mostly unnamed/off-screen nodes.
public record Trf0Element
{
    public float ResearchCost; // 0..45 in sample files
    public float ResearchTime; // 0..150 in sample files
    public float ViewPosX;     // 0..80 in sample files
    public float ViewPosY;     // 0..1500 in sample files

    public uint OffsetShortName_TRF7;
    public uint OffsetLongName_TRF8;
    public uint OffsetHelpText_TRF9;
    public uint OffsetDescription_TRFA;

    public ushort OffsetAux_TRFB;
    public byte TurretType;
    public byte MainType;
    public byte SubType;
    public byte BuildSubSystem;
    public byte SizeOfType;
    public byte UpgradeLevel;
}

[Flags]
public enum NodeState : byte
{
    Hidden = 0,
    Available = 1 << 0, // 0x01
    Researched = 1 << 1, // 0x02
    Active = 1 << 2 // 0x04: if not set, node is disabled
}
