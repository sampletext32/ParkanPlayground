namespace WeaLib;

/// <summary>
/// Parsed `.wea` material table. The game loads this separately from MSH/CTL data.
/// </summary>
/// <param name="FileName">Source file name or path supplied by the caller.</param>
/// <param name="Materials">Material id-to-name rows before the optional LIGHTMAPS section.</param>
/// <param name="Lightmaps">Optional lightmap id-to-name rows after the LIGHTMAPS marker.</param>
public sealed record WeaFile(
    string FileName,
    IReadOnlyList<WeaMaterialRef> Materials,
    IReadOnlyList<WeaLightmapRef> Lightmaps);

/// <summary>Material row from a `.wea` file: `{id} {material_name}`.</summary>
public readonly record struct WeaMaterialRef(int Id, string Name);

/// <summary>Lightmap row from the optional `.wea` LIGHTMAPS section.</summary>
public readonly record struct WeaLightmapRef(int Id, string Name);
