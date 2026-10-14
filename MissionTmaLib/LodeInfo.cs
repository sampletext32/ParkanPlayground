using Common;

namespace MissionTmaLib;

/// <summary>Залежь полезных ископаемых (LodeInfo в MisLoad.dll, 0x1C байт).</summary>
/// <param name="Position">Позиция залежи.</param>
/// <param name="Flag">Поле +0x0C: 0 или 1, AI читает как bool.</param>
/// <param name="Uninitialized">Поле +0x10: игра не читает его из файла, значение в памяти не определено.</param>
/// <param name="UnknownFloat">Поле +0x14 (возможно запас); не выяснено.</param>
/// <param name="UnknownInt3">Поле +0x18; не выяснено.</param>
public record LodeInfo(Vector3 Position, int Flag, int Uninitialized, float UnknownFloat, int UnknownInt3);
