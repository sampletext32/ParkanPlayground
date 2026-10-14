namespace TblLib;

/// <summary>Строка таблицы привязок ввода (<c>m1.tbl</c>, <c>m2.tbl</c>, <c>hero.tbl</c>) - одна привязка клавиши или оси мыши к команде.</summary>
/// <param name="Device">Устройство ввода.</param>
/// <param name="Modifier">Клавиша-модификатор (<c>SCAN_NULL</c> - без модификатора).</param>
/// <param name="Scan">Клавиша или ось (<c>SCAN_W</c>, <c>SCAN_MOUSE_X</c>...).</param>
/// <param name="IsPress">true - нажатие, false - отпускание.</param>
/// <param name="ItemClass">Класс предмета (<c>CICLS_*</c>); <c>CICLS_UNKNOWN</c> - сам объект.</param>
/// <param name="Command">Команда (<c>MCMD_*</c>).</param>
/// <param name="Value">Значение команды.</param>
/// <param name="ItemIndex">Номер предмета класса: -1 - все, 1..n - n-й.</param>
/// <param name="Mode">Режим: <c>0</c>, <c>MAN_WRAP</c>, <c>MAN_NOTWRAP</c> или состояние <c>CIS_*</c>.</param>
/// <param name="RampStep">Шаг нарастания значения.</param>
/// <param name="RampTimeMs">Время нарастания в миллисекундах.</param>
/// <param name="Comment">Комментарий после <c>//</c>.</param>
public record TblBinding(
    TblDevice Device,
    string Modifier,
    string Scan,
    bool IsPress,
    string ItemClass,
    string Command,
    float Value,
    int ItemIndex,
    string Mode,
    float RampStep,
    int RampTimeMs,
    string Comment);
