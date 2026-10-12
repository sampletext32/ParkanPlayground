namespace ScrLib;

/// <summary>
/// Файл формул <c>.fml</c> (текстовый, рядом с одноимённым <c>.scr</c>). Число формул равно числу инструкций Execute
/// с <see cref="ScrEntryInner.NativeActionIndex"/> = -1 и <see cref="ScrEntryInner.ValueOrSourceVarIndex"/> = -1 в <c>.scr</c> (проверено по 50 парам, 1975 формул).
/// </summary>
/// <param name="Formulas">Формулы в порядке файла.</param>
public record FmlFile(List<FmlFormula> Formulas);
