using Common;

namespace ParkanPlayground.Effects;

/// <summary>
/// Static reader methods for parsing FXID effect definition structures from binary streams.
/// </summary>
public static class FxidReader
{
    /// <summary>
    /// Reads a Vector3 (3 floats: X, Y, Z) from the binary stream.
    /// </summary>
    public static Vector3 ReadVector3(BinaryReader br)
    {
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        float z = br.ReadSingle();
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Reads the 60-byte effect header from the binary stream.
    /// </summary>
    public static EffectHeader ReadEffectHeader(BinaryReader br)
    {
        EffectHeader h;
        h.ComponentCount = br.ReadUInt32();
        h.Unknown1 = br.ReadUInt32();
        h.Duration = br.ReadSingle();
        h.Unknown2 = br.ReadSingle();
        h.Flags = br.ReadUInt32();
        h.Unknown3 = br.ReadUInt32();
        h.Reserved = br.ReadBytes(24);
        h.ScaleX = br.ReadSingle();
        h.ScaleY = br.ReadSingle();
        h.ScaleZ = br.ReadSingle();
        return h;
    }

    /// <summary>
    /// Reads a BillboardComponentData (type 1) from the binary stream.
    /// </summary>
    public static BillboardComponentData ReadBillboardComponent(BinaryReader br, uint typeAndFlags)
    {
        BillboardComponentData d;
        d.TypeAndFlags = typeAndFlags;
        d.Unknown04 = br.ReadSingle();
        d.ScalarAMin = br.ReadSingle();
        d.ScalarAMax = br.ReadSingle();
        d.ScalarAExp = br.ReadSingle();
        d.ActiveTimeStart = br.ReadSingle();
        d.ActiveTimeEnd = br.ReadSingle();
        d.SampleSpreadParam = br.ReadSingle();
        d.Unknown20 = br.ReadUInt32();
        d.PrimarySampleCount = br.ReadUInt32();
        d.SecondarySampleCount = br.ReadUInt32();
        d.ExtentVec0 = ReadVector3(br);
        d.ExtentVec1 = ReadVector3(br);
        d.ExtentVec2 = ReadVector3(br);
        d.ExponentTriplet0 = ReadVector3(br);
        d.RadiusTriplet0 = ReadVector3(br);
        d.RadiusTriplet1 = ReadVector3(br);
        d.RadiusTriplet2 = ReadVector3(br);
        d.ExponentTriplet1 = ReadVector3(br);
        d.NoiseAmplitude = br.ReadSingle();
        d.Reserved = br.ReadBytes(0x50);
        return d;
    }

    /// <summary>
    /// Reads a SoundComponentData (type 2) from the binary stream.
    /// </summary>
    public static SoundComponentData ReadSoundComponent(BinaryReader br, uint typeAndFlags)
    {
        SoundComponentData d;
        d.TypeAndFlags = typeAndFlags;
        d.PlayMode = br.ReadUInt32();
        d.StartTime = br.ReadSingle();
        d.EndTime = br.ReadSingle();
        d.Pos0 = ReadVector3(br);
        d.Pos1 = ReadVector3(br);
        d.Offset0 = ReadVector3(br);
        d.Offset1 = ReadVector3(br);
        d.Scalar0Min = br.ReadSingle();
        d.Scalar0Max = br.ReadSingle();
        d.Scalar1Min = br.ReadSingle();
        d.Scalar1Max = br.ReadSingle();
        d.SoundFlags = br.ReadUInt32();
        d.SoundNameAndReserved = br.ReadBytes(0x40);
        return d;
    }

    /// <summary>
    /// Reads an AnimParticleComponentData (type 3) from the binary stream.
    /// </summary>
    public static AnimParticleComponentData ReadAnimParticleComponent(BinaryReader br, uint typeAndFlags)
    {
        AnimParticleComponentData d;
        d.TypeAndFlags = typeAndFlags;
        d.Unknown04 = br.ReadSingle();
        d.ScalarAMin = br.ReadSingle();
        d.ScalarAMax = br.ReadSingle();
        d.ScalarAExp = br.ReadSingle();
        d.ActiveTimeStart = br.ReadSingle();
        d.ActiveTimeEnd = br.ReadSingle();
        d.SampleSpreadParam = br.ReadSingle();
        d.Unknown20 = br.ReadUInt32();
        d.PrimarySampleCount = br.ReadUInt32();
        d.SecondarySampleCount = br.ReadUInt32();
        d.ExtentVec0 = ReadVector3(br);
        d.ExtentVec1 = ReadVector3(br);
        d.ExtentVec2 = ReadVector3(br);
        d.ExponentTriplet0 = ReadVector3(br);
        d.RadiusTriplet0 = ReadVector3(br);
        d.RadiusTriplet1 = ReadVector3(br);
        d.RadiusTriplet2 = ReadVector3(br);
        d.ExponentTriplet1 = ReadVector3(br);
        d.NoiseAmplitude = br.ReadSingle();
        d.Reserved = br.ReadBytes(0x38);
        return d;
    }

    /// <summary>
    /// Reads an AnimBillboardComponentData (type 4) from the binary stream.
    /// </summary>
    public static AnimBillboardComponentData ReadAnimBillboardComponent(BinaryReader br, uint typeAndFlags)
    {
        AnimBillboardComponentData d;
        d.TypeAndFlags = typeAndFlags;
        d.Unknown04 = br.ReadSingle();
        d.ScalarAMin = br.ReadSingle();
        d.ScalarAMax = br.ReadSingle();
        d.ScalarAExp = br.ReadSingle();
        d.ActiveTimeStart = br.ReadSingle();
        d.ActiveTimeEnd = br.ReadSingle();
        d.SampleSpreadParam = br.ReadSingle();
        d.Unknown20 = br.ReadUInt32();
        d.PrimarySampleCount = br.ReadUInt32();
        d.SecondarySampleCount = br.ReadUInt32();
        d.ExtentVec0 = ReadVector3(br);
        d.ExtentVec1 = ReadVector3(br);
        d.ExtentVec2 = ReadVector3(br);
        d.ExponentTriplet0 = ReadVector3(br);
        d.RadiusTriplet0 = ReadVector3(br);
        d.RadiusTriplet1 = ReadVector3(br);
        d.RadiusTriplet2 = ReadVector3(br);
        d.ExponentTriplet1 = ReadVector3(br);
        d.NoiseAmplitude = br.ReadSingle();
        d.Reserved = br.ReadBytes(0x3C);
        return d;
    }

    /// <summary>
    /// Reads a TrailComponentData (type 5) from the binary stream.
    /// </summary>
    public static TrailComponentData ReadTrailComponent(BinaryReader br, uint typeAndFlags)
    {
        TrailComponentData d;
        d.TypeAndFlags = typeAndFlags;
        d.Unknown04To10 = br.ReadBytes(0x10);
        d.SegmentCount = br.ReadUInt32();
        d.Param0 = br.ReadSingle();
        d.Param1 = br.ReadSingle();
        d.Unknown20 = br.ReadUInt32();
        d.Unknown24 = br.ReadUInt32();
        d.ActiveTimeStart = br.ReadSingle();
        d.ActiveTimeEnd = br.ReadSingle();
        d.TextureNameAndReserved = br.ReadBytes(0x40);
        return d;
    }

    /// <summary>
    /// Reads a PointComponentData (type 6) from the binary stream.
    /// Note: Point components have no payload beyond the typeAndFlags header.
    /// </summary>
    public static PointComponentData ReadPointComponent(uint typeAndFlags)
    {
        PointComponentData d;
        d.TypeAndFlags = typeAndFlags;
        return d;
    }

    /// <summary>
    /// Reads a PlaneComponentData (type 7) from the binary stream.
    /// </summary>
    public static PlaneComponentData ReadPlaneComponent(BinaryReader br, uint typeAndFlags)
    {
        PlaneComponentData d;
        d.Base = ReadAnimParticleComponent(br, typeAndFlags);
        d.ExtraPlaneParam0 = br.ReadUInt32();
        d.ExtraPlaneParam1 = br.ReadUInt32();
        return d;
    }

    /// <summary>
    /// Reads a ModelComponentData (type 8) from the binary stream.
    /// </summary>
    public static ModelComponentData ReadModelComponent(BinaryReader br, uint typeAndFlags)
    {
        ModelComponentData d;
        d.TypeAndFlags = typeAndFlags;
        d.Unk04 = br.ReadBytes(0x14);
        d.ActiveTimeStart = br.ReadSingle();
        d.ActiveTimeEnd = br.ReadSingle();
        d.Unknown20 = br.ReadUInt32();
        d.InstanceCount = br.ReadUInt32();
        d.BasePos = ReadVector3(br);
        d.OffsetPos = ReadVector3(br);
        d.ScatterExtent = ReadVector3(br);
        d.Axis0 = ReadVector3(br);
        d.Axis1 = ReadVector3(br);
        d.Axis2 = ReadVector3(br);
        d.Reserved70 = br.ReadBytes(0x18);
        d.RadiusTriplet0 = ReadVector3(br);
        d.RadiusTriplet1 = ReadVector3(br);
        d.ReservedA0 = br.ReadBytes(0x18);
        d.TextureNameAndFlags = br.ReadBytes(0x40);
        return d;
    }

    /// <summary>
    /// Reads an AnimModelComponentData (type 9) from the binary stream.
    /// </summary>
    public static AnimModelComponentData ReadAnimModelComponent(BinaryReader br, uint typeAndFlags)
    {
        AnimModelComponentData d;
        d.TypeAndFlags = typeAndFlags;
        d.AnimSpeed = br.ReadSingle();
        d.MinTime = br.ReadSingle();
        d.MaxTime = br.ReadSingle();
        d.Exponent = br.ReadSingle();
        d.Reserved14 = br.ReadBytes(0x14);
        d.DirVec0 = ReadVector3(br);
        d.Reserved34 = br.ReadBytes(0x0C);
        d.RadiusTriplet0 = ReadVector3(br);
        d.DirVec1 = ReadVector3(br);
        d.RadiusTriplet1 = ReadVector3(br);
        d.ExtentVec0 = ReadVector3(br);
        d.ExtentVec1 = ReadVector3(br);
        d.Reserved7C = br.ReadBytes(0x0C);
        d.TextureNameAndFlags = br.ReadBytes(0x48);
        return d;
    }

    /// <summary>
    /// Reads a CubeComponentData (type 10) from the binary stream.
    /// </summary>
    public static CubeComponentData ReadCubeComponent(BinaryReader br, uint typeAndFlags)
    {
        CubeComponentData d;
        d.Base = ReadAnimBillboardComponent(br, typeAndFlags);
        d.ExtraCubeParam0 = br.ReadUInt32();
        return d;
    }
}
