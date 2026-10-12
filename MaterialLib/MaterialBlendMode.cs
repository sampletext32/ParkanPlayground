namespace MaterialLib;

/// <summary>
/// Режим смешивания материала: биты 2..5 версии MAT0 = индекс в таблице профилей состояний рендера Ngi32.dll
/// (<c>g_DefaultRenderStateProfiles</c>, <c>3DRender::SelectRenderStateProfile</c>). Профиль задаёт SRCBLEND, DESTBLEND, ALPHAFUNC,
/// ALPHATESTENABLE и ALPHABLENDENABLE; если устройство режим не поддерживает, <c>CShade::InitAlphaBlendModeTranslateTable</c>
/// подставляет ближайший меньший поддерживаемый. Путь вызовов: <c>EffectContents::Draw</c> → <c>CShade::RenderEffect</c> →
/// <c>PrimitiveMaterial.device_caps</c> → <c>SelectRenderStateProfile</c>.
/// </summary>
/// <remarks>Статистика по 628 материалам <c>Material.lib</c>: режимов 3, 4, 5 в данных нет.</remarks>
public enum MaterialBlendMode : uint
{
    /// <summary>ONE/ZERO, тест и смешивание выключены: непрозрачные материалы. В данных 377 материалов.</summary>
    Opaque = 0,

    /// <summary>SRCALPHA/ZERO с alpha-тестом ALPHAFUNC = GREATEREQUAL: вырезка по альфе (SKY, EXPL1, SHOOT1, B_PG2...). В данных 184 материала.</summary>
    SrcAlphaZeroAlphaTest = 1,

    /// <summary>SRCALPHA/ONE с alpha-тестом GREATEREQUAL: аддитивное, взвешенное альфой. Материалы эффектов (JET*, LIGHT1, LASER_R, GLOW_R). В данных 67 материалов.</summary>
    Additive = 2,

    /// <summary>ZERO/SRCCOLOR: умножение фона на цвет источника. В данных не встречается.</summary>
    Multiply = 3,

    /// <summary>SRCALPHA/INVSRCALPHA: обычная прозрачность. В данных не встречается.</summary>
    Alpha = 4,

    /// <summary>DESTCOLOR/SRCCOLOR: модуляция в два раза. В данных не встречается.</summary>
    Modulate2X = 5,
}
