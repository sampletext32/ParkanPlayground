namespace Common;

/// <summary>Трёхмерный вектор (три float, 12 байт в файлах игры).</summary>
/// <param name="X">Координата X.</param>
/// <param name="Y">Координата Y.</param>
/// <param name="Z">Координата Z.</param>
public readonly record struct Vector3(float X, float Y, float Z);
