namespace FxidLib;

/// <summary>
/// Флаги заголовка FXID (+0x10). Источники: функции Effect.dll <c>FxManager::UpdateEffectHandle</c>, <c>FxManager::ComputeEffectStrength</c>,
/// <c>EffectHandle::EvaluateComponentsAndMergeBounds</c> (прежде «UpdateDust»), <c>EffectHandle::RenderEffectComponents</c>,
/// <c>EffectHandle::InitFromFxidData</c>. Пометка «проверено» означает, что поведение подтверждено чтением кода, «возможно» - выведено по использованию.
/// </summary>
/// <remarks>
/// Количество файлов с флагом (из 476): 0x1 - 52, 0x2 - 106, 0x4 - 108, 0x8 - 51, 0x10 - 130, 0x20 - 56, 0x40 - 7, 0x100 - 110,
/// 0x400 - 113, 0x800 - 148, 0x1000 - 7, 0x2000 - 1, 0x8000 - 5; 0x80 и 0x200 не встречаются.
/// Сочетаний значений 17: чаще всего 0 (122 файла), 0x16 (101), 0xD00 (75), 0x9 (44), 0x800 (36), 0xD20 (35).
/// </remarks>
[Flags]
public enum FxidEffectFlags : uint
{
    /// <summary>Флагов нет (122 файла).</summary>
    None = 0,

    /// <summary>Проверено: к силе добавляется случайное смещение ±<c>StrengthJitter</c>/2, результат ограничивается 0..1.</summary>
    RandomizeStrength = 0x0001,

    /// <summary>Проверено: когда сила достигает 1 и больше, дескриптор эффекта удаляется.</summary>
    AutoDeleteAtFull = 0x0002,

    /// <summary>Проверено: после первого обновления эффект отцепляется от детали привязки (<c>ResetTransformToParent</c>) и остаётся на месте.</summary>
    DetachAfterEmit = 0x0004,

    /// <summary>Проверено: к мировой позиции добавляется случайное смещение по каждой оси с амплитудой <c>RandShift</c> (+0x18..0x20). В данных поля <c>RandShift</c> везде 0.</summary>
    RandomOffset = 0x0008,

    /// <summary>Проверено: эффект включён независимо от флагов и состояния детали привязки (см. <see cref="EnabledInAnyPieceState"/>).</summary>
    AlwaysEnabled = 0x0010,

    /// <summary>Проверено (<c>ComputeEffectStrength</c>): треугольная форма, <c>v &gt;= 0.5 ? 1 − v : v</c>, затем <c>v * 2</c>. Применяется после <see cref="MultiplyByLife"/>.</summary>
    TriangularShape = 0x0020,

    /// <summary>Проверено (<c>InitFromFxidData</c>): <c>isActive = !(Flags &amp; 0x40)</c>; эффект нужно включать отдельно (<c>ActivateEffect</c>).</summary>
    StartInactive = 0x0040,

    /// <summary>
    /// Проверено (<c>ComputeEffectStrength</c>): сила равна 0, пока бит 0 <c>FxManager.environment_flags</c> сброшен (бит ставят и сбрасывают два
    /// сообщения окружения, смысл не выяснен). В README ранее 0x80 и 0x100 были перепутаны. В данных не встречается.
    /// </summary>
    OnlyWhenEnvBit0On = 0x0080,

    /// <summary>Проверено (<c>ComputeEffectStrength</c>): сила равна 0, пока бит 0 <c>environment_flags</c> установлен.</summary>
    OnlyWhenEnvBit0Off = 0x0100,

    /// <summary>Проверено (<c>ComputeEffectStrength</c>): сила умножается на прогресс жизни, до <see cref="TriangularShape"/>.</summary>
    MultiplyByLife = 0x0200,

    /// <summary>
    /// Проверка загороженности (вероятно): <c>InitFromFxidData</c> ставит <c>stateFlags |= 0x100|0x10</c>. <c>RenderEffectComponents</c>
    /// периодически пускает луч (<c>FindNearestIntersectedFaceInSystemArealMap</c>) от точки привязки, и компоненты рисуются, только когда
    /// луч свободен (бит 3 <c>stateFlags</c>). Концы луча не подтверждены.
    /// </summary>
    HideWhenOccluded_maybe = 0x0400,

    /// <summary>
    /// Проверено (<c>RenderEffectComponents</c>): эффект рисуется только когда вызов отрисовки передал дополнительный признак (<c>param_3 != 0</c>);
    /// без него менеджер ставит <c>environment_flags |= 2</c> («есть обычные эффекты»). Что за дополнительный проход - не выяснено.
    /// </summary>
    RenderOnlyInExtraPass_maybe = 0x0800,

    /// <summary>
    /// Проверено: ставит <c>environment_flags |= 0x10</c>, из-за чего <c>FRAME_UPDATE</c> пересчитывает точку привязки и вызывает
    /// <c>NotifyAttachPointMoved</c> у эффектов (шлейф начинается из точки привязки текущего кадра). Сообщение отправляется, только если менеджер
    /// привязан через <c>UpdateCachedLocalPoint</c>; эффекты, порождённые командами CTL (лазеры), не привязаны, поэтому их шлейф начинается в точке порождения.
    /// </summary>
    TracksAttachPoint_maybe = 0x1000,

    /// <summary>Передаётся (бит 13) последним аргументом вызова отрисовки компонентов. Значение не выяснено; в данных встречается в 1 файле.</summary>
    Unknown0x2000_TBD = 0x2000,

    /// <summary>Проверено: разрешает эффект, когда состояние детали привязки ненулевое (<c>enabled = pieceFlagsOk &amp;&amp; (state == 0 || this) || AlwaysEnabled</c>). 5 файлов.</summary>
    EnabledInAnyPieceState = 0x8000,
}
