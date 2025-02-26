using System.Numerics;

namespace NResUI.Models;

public class BinaryExplorerViewModel
{
    public bool HasFile { get; set; }
    public string? Error { get; set; }

    public string Path { get; set; } = "";
    public byte[] Data { get; set; } = [];

    public List<Region> Regions { get; set; } = [];

    public Vector4 NextColor;
}

public class Region
{
    public int Begin { get; set; }

    public int Length { get; set; }

    public uint Color { get; set; }

    public string? Value;
}