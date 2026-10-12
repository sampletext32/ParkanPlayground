namespace ControlLib;

/// <summary>
/// Привязка детали (0x30 байт): связывает локальную деталь меша с необязательными контрольными точками, диапазоном анимации и значением по умолчанию.
/// Предметы ссылаются на привязки; значение по умолчанию служит запасным значением и начальным прогрессом анимации. Движок строит из привязки
/// канал анимации (<c>ControlPieceAnimChannel</c>), значение которого 0..1 интерполируется между <c>Anim1</c> и <c>Anim2</c>.
/// В данных 628 привязок: 0..37 на файл.
/// </summary>
/// <param name="Offset">Смещение записи от начала файла (для отладки).</param>
/// <param name="LocalPieceIndex">+0x00: локальный индекс детали. У присоединённых объектов 0 - виртуальный корень, N &gt; 0 - первая деталь этого msh_tag + N − 1. В данных 0..29.</param>
/// <param name="Anim1Start">+0x04: начало диапазона анимации 1 (время кадров меша). Во всех 628 привязках отрицательно (диапазон 1 не задан).</param>
/// <param name="Anim1EndOrDuration">+0x08: конец (или длительность) диапазона 1.</param>
/// <param name="Anim2Start">+0x0C: начало диапазона анимации 2. Отрицательно у 194 привязок (без диапазона 2).</param>
/// <param name="Anim2EndOrDuration">+0x10: конец (или длительность) диапазона 2.</param>
/// <param name="AnimBlendWeight">+0x14: вес смешивания диапазонов; во всех привязках 1.0.</param>
/// <param name="DefaultValue">+0x18: значение по умолчанию канала 0..1 (поза при загрузке: <c>начало + default * (конец − начало)</c>). В данных 0..0.5.</param>
/// <param name="PrimaryCptRefIndex">+0x1C: основная контрольная точка (дуло, ось) или −1. Нет у 490 привязок.</param>
/// <param name="SecondaryCptRefIndex">+0x20: вторичная контрольная точка (направление) или −1. Нет у 245 привязок.</param>
/// <param name="ChannelSpeed">+0x24: скорость канала анимации, ед./с; время перехода = <c>|Δ| * 1000 / ChannelSpeed</c> мс (<c>CItemBase::ExtendTransitionEndTimeForBinding</c>).
/// У орудий 4.0. Наиболее частые значения: 2.5 - 158 привязок, 1.5 - 73, 0.75 - 67, 0.6 - 65, 2 - 65, 0.5 - 58 (22 различных).</param>
/// <param name="YawNormalizationAngle">+0x28: угол нормализации рыскания; применение не проверено. В данных: 1.0 - 354, 2π (6.28) - 112, π/2 (1.5708) - 105, 1.9199 - 11, 0.9 - 10 (12 различных).</param>
/// <param name="Flags">+0x2C: флаги привязки. 0x01 - значение по умолчанию циклическое (wrap), 0x02 - инвертировать значение по умолчанию,
/// 0x04 - пропустить инициализацию анимации, 0x08 - авто-добавление/ссылка из системы предметов, 0x10 - значение канала пишется в фазу материала детали
/// (<c>IAnimation::SetPieceMaterialPhase</c>), 0x40 - <c>COPY_PREVIOUS_VALUE</c> (канал копирует предыдущий; автомат выстрела такие привязки пропускает),
/// 0x100 - условная ссылка (добавляется в рантайме по флагам детали MSH, в данных не встречается).
/// В данных: 0x04 - 246 привязок, 0 - 119, 0x01 - 70, 0x08 - 59, 0x03 - 58, 0x02 - 41, 0x15 - 12, 0x40 - 12, 0x48 - 9, 0x0C - 2.</param>
public sealed record CtlPieceBindingRecordRaw(
    long Offset,
    int LocalPieceIndex,
    float Anim1Start,
    float Anim1EndOrDuration,
    float Anim2Start,
    float Anim2EndOrDuration,
    float AnimBlendWeight,
    float DefaultValue,
    int PrimaryCptRefIndex,
    int SecondaryCptRefIndex,
    float ChannelSpeed,
    float YawNormalizationAngle,
    uint Flags);
