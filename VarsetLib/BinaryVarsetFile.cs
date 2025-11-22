namespace VarsetLib;

/// <summary>
/// Бинарный файл, который можно найти в behpsp.res
/// </summary>
public record BinaryVarsetFile(int Count, List<BinaryVarsetItem> Items);
