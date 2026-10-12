namespace MaterialLib;

/// <summary>
/// Файл материала MAT0 из архива <c>Material.lib</c>: набор стадий (цвета и текстура) и анимаций, переключающих стадии по времени.
/// Игра разбирает его в <c>MatManager::LoadMaterialByName</c> (World3D.dll) в дескриптор <c>MaterialGlobalDescriptor</c> (0x170 байт).
/// </summary>
/// <param name="FileName">Имя материала из метаданных NRes (в самом содержимом MAT0 имени нет).</param>
/// <param name="Version">Значение attr1 (ElementCount) из метаданных NRes. Биты: 0 - флаг текстур 0x200000, 1 - материал поддерживает bump,
/// 2..5 - индекс режима смешивания (<see cref="BlendMode"/>), 6 - флаг 0x40 (в данных не встречается).
/// В <c>Material.lib</c> (628 материалов): 0 - 53, 2 - 324, 4 - 183, 5 - 1, 8 - 67.</param>
/// <param name="Magic1">Значение attr2 из метаданных NRes - версия формата содержимого: поля класса поверхности читаются при &gt;= 2,
/// скорость поверхности при &gt; 2, потеря жизни при &gt; 3. Во всех 628 материалах равно 6.</param>
/// <param name="BlendMode">Режим смешивания: <c>(Version &gt;&gt; 2) &amp; 0xF</c>. Индекс профиля состояний рендера Ngi32.
/// В данных: 0 - 377 материалов, 1 - 184, 2 - 67.</param>
/// <param name="SupportsBumpMapping">Бит 1 версии: у устройства без bump текстуры грузятся с флагом 0x80000.
/// Установлен у 324 материалов из 628 (все материалы с версией 2).</param>
/// <param name="SurfaceMaterial">Класс поверхности (байт 4 содержимого): по нему <c>Control.dll</c> выбирает слот эффекта взрыва в EXPL
/// и список команд хука 10..20. Читается при <c>Magic1 &gt;= 2</c>, иначе <see cref="SurfaceMaterial.None"/>.
/// В Ghidra World3D.dll поле названо <c>SourceBlendMode</c> ошибочно.</param>
/// <param name="Reserved05">Байт 5 содержимого: читается при <c>Magic1 &gt;= 2</c>, иначе 0xFF. Движок кладёт его в поле
/// <c>DestBlendMode</c> дескриптора, но нигде не использует; в данных всегда 255 (все 628 материалов).</param>
/// <param name="SurfaceSpeedScale">Множитель скорости юнита на этой поверхности (<c>surface_speed_scale</c> в World3D.dll; юнит берёт его
/// в <c>runtime_tangential_speed_scale</c>). Читается при <c>Magic1 &gt; 2</c>, по умолчанию 1.0. Во всех 628 материалах равен 1.0.</param>
/// <param name="SurfaceLifeDrainPerSec">Потеря жизни юнита в секунду, пока он стоит на поверхности (<c>surface_life_drain_per_sec</c>; юнит берёт
/// в <c>passive_life_drain_rate</c>). Читается при <c>Magic1 &gt; 3</c>, по умолчанию 0. Ненулевое значение у 4 материалов:
/// <c>ENV_LAVA_BOT</c> и <c>WATER_BOT</c> - 10000, <c>B_S0_DAM</c> и <c>B_DD1DK_DAM</c> - 1000.</param>
/// <param name="Stages">Стадии материала в порядке чтения: всего 1811 на 628 материалов (1 стадия у 386 материалов, максимум 29).</param>
/// <param name="Animations">Анимации материала: всего 685 (1 анимация у 583 материалов, 2 - у 43, 8 - у 2).
/// Номер анимации выбирается индексом анимации; вне диапазона заменяется на 0.</param>
public record MaterialFile(
    string FileName,
    uint Version,
    int Magic1,
    MaterialBlendMode BlendMode,
    bool SupportsBumpMapping,
    SurfaceMaterial SurfaceMaterial,
    byte Reserved05,
    float SurfaceSpeedScale,
    float SurfaceLifeDrainPerSec,
    List<MaterialStage> Stages,
    List<MaterialAnimation> Animations);
