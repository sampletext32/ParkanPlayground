using Common;

namespace MissionTmaLib;

/// <summary>Место миграции клана (структура MigrationPlace в MisLoad.dll, 0x14 байт).</summary>
/// <param name="Reserved">Читается и не используется игрой (в данных 29 миссий - небольшие числа).</param>
/// <param name="Position">Центр места.</param>
/// <param name="Radius">Радиус места.</param>
/// <param name="ApproachRadiusBase">Базовый радиус подхода; AI использует его при расчёте дистанции миграции (MigrateTask).</param>
public record MigrationPlace(int Reserved, Vector3 Position, float Radius, float ApproachRadiusBase);
