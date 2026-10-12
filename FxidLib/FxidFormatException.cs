namespace FxidLib;

/// <summary>Исключение: содержимое FXID не соответствует раскладке, проверенной по оригинальным файлам игры.</summary>
/// <param name="message">Описание нарушения (усечение, неизвестный тип компонента, несовпадение размеров).</param>
public sealed class FxidFormatException(string message) : Exception(message);
