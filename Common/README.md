# Common: общие типы и чтение бинарных данных

Мелкие типы и расширения, на которые ссылаются остальные библиотеки проекта.

## Чтение (`Common.Extensions`, методы на `Stream`, little-endian)

| Метод | Что читает |
|---|---|
| `ReadInt32LittleEndian`, `ReadUInt32LittleEndian`, `ReadInt16LittleEndian`, `ReadUInt16LittleEndian`, `ReadFloatLittleEndian` | числа |
| `ReadBytesExact(count)` | ровно `count` байт или исключение |
| `ReadFixedString(count)` / `ReadFixedString(ReadOnlySpan<byte>)` | ASCII-строка из буфера фиксированного размера до первого нуля (остаток буфера игнорируется) |
| `ReadNullTerminatedString`, `ReadNullTerminated1251String` | строка до нуля (ASCII / windows-1251) |
| `ReadLengthPrefixedString` | строка с длиной int32 в начале |
| `ReadVector3LittleEndian` | три float подряд |
| `ReadInt32ArrayLittleEndian(count)` | массив int32 |
| `ReadResourceNamedRef` | ссылка на элемент NRes (64 байта) |

## Типы

| Тип | Назначение |
|---|---|
| `Vector3` (`readonly record struct`) | трёхмерный вектор |
| `Quaternion`, `UShortQuaternion` | кватернионы (float и 16-битная форма) |
| `Sphere` | сфера границ (центр и радиус) |
| `IndexedEdge` | ребро по двум индексам |
| `ResourceNamedRef` | ссылка на элемент NRes: имя архива (32 байта) + имя ресурса (32 байта); у `IsEmpty` значение - оба имени пусты |
| `IntFloatValue` | только для отладки: одно и то же 4-байтное значение как int32 и float32 |
