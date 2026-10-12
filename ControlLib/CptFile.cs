namespace ControlLib;

/// <summary>
/// Файл контрольных точек CPT (тип NRes <c>CTPT</c>): именованные точки объекта - якоря эффектов, оси стволов, контакты колёс, камера, центр башни.
/// Игра загружает его как вторую ссылку записи агента (<c>CControlBase</c> хранит их как <c>CPT refs</c>).
/// </summary>
/// <param name="SourceName">Имя исходного файла или <c>null</c>, если читалось из потока.</param>
/// <param name="Count">Число записей. В данных (первая часть игры): 225 файлов, 1267 точек, от 0 до 36 точек в файле.</param>
/// <param name="Records">Записи точек: положение и направление.</param>
/// <param name="Names">Имена точек в том же порядке, что и <paramref name="Records"/> (после всех записей идут имена по 32 байта ASCII на каждую точку).</param>
public sealed record CptFile(
    string? SourceName,
    uint Count,
    IReadOnlyList<CptRecordRaw> Records,
    IReadOnlyList<string> Names);
