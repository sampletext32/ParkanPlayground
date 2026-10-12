using Common;

namespace ControlLib;

/// <summary>
/// Корневые параметры CTL (0x6C байт): базовые лимиты движения объекта (<c>base_control_limits</c> в <c>CControlBase</c>, потолок и база для расчётов).
/// Рабочие лимиты (<c>runtime_control_limits</c>) копируются из них и пересчитываются от повреждений, мощности двигателей, целостности колёс и скорости
/// (<c>RecomputeEffectiveMovementLimits</c>, Control.dll 0x10014630).
/// </summary>
/// <param name="Offset">Смещение блока от начала файла (для отладки).</param>
/// <param name="MaxTangentialAccel">+0x00: максимальное тангенциальное ускорение по осям объекта. В данных X 0..90, Y 0..150, Z 0..90.</param>
/// <param name="Unknown0C">+0x0C: не выяснено. В данных 0 у 446 файлов и 0.1 (как float) у 3.</param>
/// <param name="Unknown10">+0x10: не выяснено. В данных 0 у 440 файлов, иначе 0.6 (5 файлов), 0.1 (3), 0.49 (1) как float.</param>
/// <param name="Unknown14">+0x14: не выяснено. В данных 0 у 446 файлов и 0.1 (как float) у 3.</param>
/// <param name="MaxTangentialSpeed">+0x18: максимальная тангенциальная скорость по осям; <c>Y</c> - скорость движения вперёд (у снарядов - скорость вылета).
/// В данных X 0..20, Y 0..10000, Z 0..35.</param>
/// <param name="MaxAngularSpeed">+0x24: максимальная угловая скорость по осям. В данных X 0..17, Y 0..17, Z 0..25.12.</param>
/// <param name="ExternalTorqueResponseScale">+0x30: множитель реакции на внешний момент по осям. В данных 0..1.</param>
/// <param name="VisualEulerAngleLimit">+0x3C: предел визуальных углов Эйлера (крен/тангаж/рыскание). В данных 0..6.28 (2π).</param>
/// <param name="DetachCleanupDelayMs">+0x48: задержка удаления объекта после отцепления (команда <c>SCHEDULE_DETACH_OR_KILL</c>), мс.
/// В данных: 0 - 304 файла, 5000 - 89, 2000, 3000 - по 18, 1000 - 17, 11000 - 3 (у снарядов-строителей <c>bld_*</c>).</param>
/// <param name="Unknown4C">+0x4C: не выяснено (float). В данных 0 у 433 файлов, иначе 0.1..0.2 (10 различных значений).</param>
/// <param name="Unknown50">+0x50: не выяснено (float). В данных 0 у 433 файлов, иначе 0.1, 0.15 и один раз 1000.</param>
/// <param name="MovementPhysicsMode">+0x54: режим физики движения; ненулевое значение означает, что объект подвержен гравитации
/// (<c>movement_physics_mode != 0</c> попадает в CP6 орудий). В данных: 0 - 432 файла, 2 - 13, 3 - 4.</param>
/// <param name="Unknown58">+0x58: не выяснено (float); у орудий используется в CP8 как предел дальности (<c>field12_0x58</c>, не проверено). В данных −1 у 396 файлов, иначе 300..1000 (18 различных значений).</param>
/// <param name="SlopeLimitAngle">+0x5C: предельный угол уклона, рад. В данных 1.5708 (π/2) - 432 файла, 0.6 - 13, 1.52 - 4.</param>
/// <param name="Unknown60">+0x60: не выяснено. В данных: 0 - 313 файлов, 3 - 67, 4 - 37, 2 - 15 (7 различных значений).</param>
/// <param name="Unknown64">+0x64: не выяснено (float). В данных −1 у 368 файлов, 0 - у 67, иначе 40000..3000000 (8 различных значений).</param>
/// <param name="MobilityIntegrityAmount">+0x68: максимальная целостность подвижности (<c>mobility_integrity_amount</c>; делится на текущую при расчёте скорости).
/// В данных <c>FLT_MAX</c> (3.4e38, подвижность не ограничена) у 429 файлов, у остальных 2775..1e7 (19 различных значений).</param>
public sealed record CtlRootControlParams(
    long Offset,
    Vector3 MaxTangentialAccel,
    uint Unknown0C,
    uint Unknown10,
    uint Unknown14,
    Vector3 MaxTangentialSpeed,
    Vector3 MaxAngularSpeed,
    Vector3 ExternalTorqueResponseScale,
    Vector3 VisualEulerAngleLimit,
    int DetachCleanupDelayMs,
    float Unknown4C,
    float Unknown50,
    int MovementPhysicsMode,
    float Unknown58,
    float SlopeLimitAngle,
    uint Unknown60,
    float Unknown64,
    float MobilityIntegrityAmount);
