namespace ScrLib;

/// <summary>
/// Формула из <c>.fml</c>: одна строка вида <c>FUNCTION( имя, выражение, комментарий )</c>.
/// Проверено по 50 файлам (1975 формул): имя и комментарий пусты во всех формулах, выражение есть всегда.
/// </summary>
/// <param name="Index">Номер формулы с 0 (порядок строк в файле); на него ссылается <see cref="ScrInstruction.FmlFunctionIndex"/>.</param>
/// <param name="Name">Первое поле (имя); в данных игры всегда пустое.</param>
/// <param name="Expression">Арифметическое выражение над переменными <c>varset.var</c> и числами (например <c>dTemp3 + 0.5*dTemp3*fDifficulty</c>).</param>
/// <param name="Comment">Третье поле (комментарий); в данных игры всегда пустое.</param>
public record FmlFormula(int Index, string Name, string Expression, string Comment);
