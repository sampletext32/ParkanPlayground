namespace ControlLib;

/// <summary>
/// Цепочка состояний: последовательность состояний, соединённых рёбрами веса 0 в матрице весов CTL (принудительное продолжение). Вместе они дают целую фазу
/// движения, например шаг: у <c>R_H_02</c> состояния 9..14 проигрывают 17→19, 19→21, … , 27→29.
/// </summary>
/// <param name="StateIndices">Индексы состояний в порядке проигрывания.</param>
/// <param name="Start">Время шкалы в начале цепочки.</param>
/// <param name="End">Время шкалы в конце цепочки (меньше <paramref name="Start"/> - цепочка идёт назад).</param>
public sealed record CtlStateChain(IReadOnlyList<int> StateIndices, float Start, float End);
