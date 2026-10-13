namespace ControlLib;

/// <summary>
/// Анимационные поля узла графа состояний (<c>ControlStateNode</c>, 0xA0 байта). Состояние проигрывает отрезок общей шкалы времени MSH (компонент 0x13):
/// движок (<c>CControlBase::AdvanceControlStateSimulation</c>, Control.dll 0x10006190) передаёт анимации два диапазона через
/// <c>IAnimation::SetBlendingAnimsTimeAndDuration</c>, а прогресс 0..1 и вес - через <c>SetAnimProgressAndWeight</c>. Время семпла для каждого диапазона:
/// <c>start + (end - start) * progress</c> (<c>CAniMesh::RebuildDirtyPieceMatrices</c>, AniMesh.dll 0x10009220); итоговая поза - смесь поз по диапазону 1
/// и диапазону 2 с весом диапазона 2. Диапазон с отрицательным временем в смешивании не участвует. Если <c>end &lt; start</c>, отрезок проигрывается в обратную сторону.
/// </summary>
/// <param name="Flags">+0x04: флаги. Читаются: 0x1 - состояние можно выбрать целью (<c>SelectNextControlState</c>), 0x10000 - движение по скорости состояния
/// (<c>SetWorldVelocityAndIntegrate</c>), иначе по смещению корня анимации; в первом случае длительность ограничена так, чтобы путь не превышал 5 единиц,
/// 0x40000 - передаётся как флаг в <c>ClampAndTransformTangentialSpeedToWorld</c>, 0x100000 - статичное состояние: позиция не интегрируется, состояние
/// держится <paramref name="DurationMs"/>, 0x1000000 - к длительности добавляется случайный разброс до ±12.5%. Смысл остальных битов не выяснен.</param>
/// <param name="Anim1Start">+0x0C: начало диапазона 1 (время шкалы MSH); 0 у большинства состояний ходьбы - поза покоя.</param>
/// <param name="Anim1End">+0x10: конец диапазона 1.</param>
/// <param name="Anim2Start">+0x14: начало диапазона 2.</param>
/// <param name="Anim2End">+0x18: конец диапазона 2.</param>
/// <param name="Anim2WeightAtMinSpeed">+0x1C: вес диапазона 2 при минимальной скорости состояния; с ростом скорости до максимума вес растёт до 1
/// (<c>anim_blend_end_weight = (1 - s) * w + s</c>, где <c>s</c> - доля скорости между min и max). В файле поле не имеет имени, назначение выведено из кода.</param>
/// <param name="DurationMs">+0x20: длительность, мс. Не ноль - это и есть длительность состояния; ноль - игра считает её по смещению корня и скорости
/// (<c>(w * длина2 + (1 - w) * длина1) / скорость</c>, зажато в 0.01..5 с).</param>
/// <param name="MinTangentialSpeed">+0x24: минимальная тангенциальная скорость по осям (x, y, z).</param>
/// <param name="MaxTangentialSpeed">+0x30: максимальная тангенциальная скорость по осям (x, y, z).</param>
/// <param name="Anim1RootMotionLength">+0x64: длина смещения корня на диапазоне 1.</param>
/// <param name="Anim2RootMotionLength">+0x80: длина смещения корня на диапазоне 2.</param>
public sealed record CtlStateAnimation(
    uint Flags,
    float Anim1Start,
    float Anim1End,
    float Anim2Start,
    float Anim2End,
    float Anim2WeightAtMinSpeed,
    float DurationMs,
    (float X, float Y, float Z) MinTangentialSpeed,
    (float X, float Y, float Z) MaxTangentialSpeed,
    float Anim1RootMotionLength,
    float Anim2RootMotionLength)
{
    /// <summary>Читает поля из <see cref="CtlStateRecordRaw.FixedBody"/> (0x9C байта, раскладка <c>ControlStateNode</c>).</summary>
    public static CtlStateAnimation FromFixedBody(byte[] body)
    {
        if (body.Length < 0x9C)
            throw new ArgumentException("Тело состояния короче 0x9C байт.", nameof(body));

        float F(int offset) => BitConverter.ToSingle(body, offset);

        return new CtlStateAnimation(
            Flags: BitConverter.ToUInt32(body, 0x04),
            Anim1Start: F(0x0C),
            Anim1End: F(0x10),
            Anim2Start: F(0x14),
            Anim2End: F(0x18),
            Anim2WeightAtMinSpeed: F(0x1C),
            DurationMs: F(0x20),
            MinTangentialSpeed: (F(0x24), F(0x28), F(0x2C)),
            MaxTangentialSpeed: (F(0x30), F(0x34), F(0x38)),
            Anim1RootMotionLength: F(0x64),
            Anim2RootMotionLength: F(0x80));
    }
}
