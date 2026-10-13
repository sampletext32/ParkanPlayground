namespace ArealMapLib;

/// <summary>Нарушение формата записи ArealMap: непредвиденные данные или несовпадение размеров.</summary>
public sealed class ArealMapFormatException : Exception
{
    /// <summary>Создаёт исключение с описанием.</summary>
    /// <param name="message">Что именно не совпало.</param>
    public ArealMapFormatException(string message) : base(message)
    {
    }
}
