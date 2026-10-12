namespace FxidLib;

/// <summary>
/// Источник «силы» (strength) эффекта: число 0..1, от которого зависят окна активности и интерполяции всех компонентов.
/// Хранится в заголовке FXID по смещению +0x04, <c>EffectHandle::InitFromFxidData</c> копирует его в <c>EffectMainParams.m_StrengthSource</c>.
/// Названия взяты из перечисления Ghidra <c>EEffectStrengthSource</c>, поведение - из <c>FxManager::ComputeEffectStrength</c> (Effect.dll).
/// Ранее ошибочно назывался «TimeMode».
/// </summary>
/// <remarks>
/// Жизнь (life) = (текущее время − начало) / (конец − начало); при <c>Duration = 0</c> источники по жизни дают 0/0, поэтому файлы с нулевой
/// длительностью (120 из 476) всегда используют источники без жизни (0, 4, 5, 14, 15, 16, 17).
/// Статистика по 476 файлам effects.rlb: 1 - 210, 0 - 112, 2 - 79, 15 - 26, 4 - 19, 5 - 18, 14 - 7, 16 - 3, 17 - 2.
/// Остальные значения движок определяет, но в данных они не встречаются.
/// </remarks>
public enum EffectStrengthSource : uint
{
    /// <summary>Постоянная сила: поле <c>unk_0x28_TBD</c> дескриптора эффекта (задаётся во время работы, по умолчанию 0). 112 файлов.</summary>
    Constant = 0,

    /// <summary>Жизнь от 0 до 1. 210 файлов.</summary>
    Life_0to1 = 1,

    /// <summary>Дробная часть жизни («пила» для зацикленных эффектов). 79 файлов.</summary>
    Life_Fract = 2,

    /// <summary>1 − жизнь. В данных не встречается.</summary>
    Life_1minus = 3,

    /// <summary>Прогресс перехода анимации детали, к которой прикреплён эффект (<c>IAnimation.GetPieceAnimTransitionProgress</c>). 19 файлов.</summary>
    AnimParam = 4,

    /// <summary>Тангенциальная скорость объекта (базовое свойство IControl 0x21) относительно предела (0x11): длина вектора. 18 файлов.</summary>
    TangSpeedMag = 5,

    /// <summary>Компонента X тангенциальной скорости относительно предела. В данных не встречается.</summary>
    TangSpeedX = 6,

    /// <summary>Компонента Y тангенциальной скорости относительно предела. В данных не встречается.</summary>
    TangSpeedY = 7,

    /// <summary>Компонента Z тангенциальной скорости относительно предела. В данных не встречается.</summary>
    TangSpeedZ = 8,

    /// <summary>Нормальная скорость (базовое свойство IControl 0x24) относительно предела (0x11): длина вектора. В данных не встречается.</summary>
    NormSpeedMag = 9,

    /// <summary>Компонента X нормальной скорости. В данных не встречается.</summary>
    NormSpeedX = 10,

    /// <summary>Компонента Y нормальной скорости. В данных не встречается.</summary>
    NormSpeedY = 11,

    /// <summary>Компонента Z нормальной скорости. В данных не встречается.</summary>
    NormSpeedZ = 12,

    /// <summary>1 − <c>ILifeSystem.QueryNdpLifeStateRatio</c> (доля целостности объекта). В данных не встречается.</summary>
    LifeSystemInvX = 13,

    /// <summary>1 − базовое свойство IControl 0x31. 7 файлов.</summary>
    ControlInvX = 14,

    /// <summary>max(|тангенциальная скорость| / предел, |нормальная скорость| / предел). Декомпиляция искажена, по ассемблеру не подтверждено. 26 файлов.</summary>
    Max_TangOrNorm = 15,

    /// <summary>Как <see cref="AnimParam"/>, но не меньше предыдущего значения, пока анимация не дошла до 0. 3 файла.</summary>
    AnimMinClamp = 16,

    /// <summary>Как <see cref="AnimParam"/>, но не больше предыдущего значения, пока анимация не дошла до 1. 2 файла.</summary>
    AnimMaxClamp = 17,
}
