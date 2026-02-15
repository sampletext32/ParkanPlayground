namespace ResTreeLib;

// Result DTO for easier use

public record Trf0Element
{
    public float ResearchCost;     // field_0
    public float ResearchTime;     // field_4
    public float ViewPosX;         // field_8
    public float ViewPosY;         // field_12
    public uint OffsetShortName_TRF7;   // offset_into_TRF7
    public uint OffsetLongName_TRF8;    // offset_into_TRF8
    public uint OffsetHelpText_TRF9;    // offset_into_TRF9
    public uint OffsetDescription_TRFA; // offset_into_TRFA
    public ushort OffsetAux_TRFB;       // offset_into_TRFB
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
    Hidden     = 0,
    Available  = 1 << 0, // 0x01
    Researched = 1 << 1, // 0x02
    Active     = 1 << 2  // 0x04 - If not set, node is disabled
}