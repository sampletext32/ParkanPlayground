namespace ControlLib;

/// <summary>Исключение чтения ресурсов Control (CPT, NDP, CTL, записи агентов): неверный размер, отрицательная длина, остаток данных после разбора.</summary>
public sealed class ControlReadException : Exception
{
    /// <summary>Создаёт исключение с описанием нарушения формата.</summary>
    /// <param name="message">Описание нарушения.</param>
    public ControlReadException(string message) : base(message) { }
}
