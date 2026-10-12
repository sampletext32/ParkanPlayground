namespace MaterialLib;

/// <summary>
/// Анимация материала: список ключей, по которым движок выбирает стадию по времени (<c>MatManager::AnimateFrame</c>)
/// или по фазе 0..1 (<c>GetMaterialPhase</c>) и смешивает соседние стадии по маске <see cref="Target"/>.
/// </summary>
/// <param name="Target">Маска интерполируемых полей (биты 3..31 слова анимации). В данных (685 анимаций): 0 - 609, 1 - 68, 3 - 3, 7 - 3, 2 - 1, 17 - 1.</param>
/// <param name="LoopMode">Режим повтора (биты 0..2 слова анимации). В данных: 0 - 607, 1 - 58, 2 - 19, 3 - 1.</param>
/// <param name="Keys">Ключи анимации (1848 всего). Ключ хранит стадию и <b>конец</b> своего интервала в миллисекундах.</param>
/// <param name="TargetDescription">Готовое описание <see cref="Target"/> для интерфейса (вычисляется один раз при разборе).</param>
public record MaterialAnimation(
    AnimationTarget Target,
    AnimationLoopMode LoopMode,
    List<AnimKey> Keys,
    string TargetDescription);
