namespace MaterialLib;

/// <summary>
/// Класс поверхности материала (байт 4 содержимого MAT0). Игра использует его для выбора слота эффекта в EXPL
/// (<c>slot_by_surface[class]</c>, для 255 - слот 0) и как номер хука <c>10 + класс</c> в списках команд CTL юнита, стоящего на поверхности
/// (<c>CControlClassic::BTLU_frame_update</c> при смене класса под юнитом).
/// </summary>
/// <remarks>
/// Числа проверены по данным; названия выведены из имён материалов и суффиксов эффектов взрыва, в самой игре строковых имён нет.
/// Статистика по 628 материалам: класс 255 - 216, 5 - 262, 6 - 60, 1 - 37, 8 - 23, 9 - 12, 2 - 7, 0 - 3, остальные по 2.
/// </remarks>
public enum SurfaceMaterial : byte
{
    /// <summary>Класс 0: 3 материала (L08, L09, TOK41_PLC).</summary>
    Class0 = 0,

    /// <summary>Класс 1 («st», камень): 37 материалов - ландшафт L*, STONE*, B_S0, WATER_BOT.</summary>
    Stone = 1,

    /// <summary>Класс 2: 7 материалов ландшафта (L00, L01, L19, L20, L28, L32, L40).</summary>
    Class2 = 2,

    /// <summary>Класс 3: 2 материала (L23, L26).</summary>
    Class3 = 3,

    /// <summary>Класс 4: 2 материала (L18, L27).</summary>
    Class4 = 4,

    /// <summary>Класс 5 («m», металл): 262 материала - R_*, B_* (юниты и здания).</summary>
    Metal = 5,

    /// <summary>Класс 6 («w», дерево): 60 материалов - деревья, кора.</summary>
    Wood = 6,

    /// <summary>Класс 7 («wb/wl/wm/wt», вода): 2 материала - WATER, WATER_M.</summary>
    Water = 7,

    /// <summary>Класс 8 («al»): 23 материала B_MTP_*.</summary>
    Class8 = 8,

    /// <summary>Класс 9 («an», живое): 12 материалов - BIRD_*, TOK11, TOK42W, R_PG9.</summary>
    Class9 = 9,

    /// <summary>Класс 10 («s»): 2 материала - TOK51, B_A_BRIGE.</summary>
    Class10 = 10,

    /// <summary>Класс не задан: для EXPL используется слот 0. 216 материалов (SKY, DEFAULT, EXPL*, B_PG2...).</summary>
    None = 0xFF,
}
