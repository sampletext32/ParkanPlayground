namespace MaterialLib;

/// <summary>
/// Маска полей, которые анимация интерполирует между стадиями соседних ключей (<c>Interpolate</c>, World3D.dll 0x100032D0).
/// Не выбранные поля копируются из стадии начального ключа; <c>Power</c> и альфа diffuse/specular/emissive не интерполируются никогда
/// (раньше бит 0x10 назывался <c>Power</c> по ошибке: он интерполирует только Ambient.A).
/// Если маска 0, копируется вся стадия начального ключа.
/// </summary>
/// <remarks>
/// Статистика по анимациям с двумя и более ключами (194 штуки): маска 0 - 118, 1 - 68 (JET*, SPLASH2, DUST, SMOKE, LASER_G, B_MN_01*...),
/// 3 - 3 (B_MTP_02, B_MTP_02L, B_S13), 7 - 3 (R_RLW_5, R_RMW_9, R_S13), 2 - 1 (WATER_M), 17 - 1 (FIRESTORM).
/// </remarks>
[Flags]
public enum AnimationTarget : int
{
    /// <summary>Ambient RGB (бит 0x01).</summary>
    Ambient = 1,

    /// <summary>Diffuse RGB (бит 0x02).</summary>
    Diffuse = 2,

    /// <summary>Specular RGB (бит 0x04).</summary>
    Specular = 4,

    /// <summary>Emissive RGB (бит 0x08).</summary>
    Emissive = 8,

    /// <summary>Только Ambient.A, то есть прозрачность/яркость (бит 0x10). Power из начального ключа копируется всегда.</summary>
    AmbientAlpha = 16
}
