using Common;

namespace MissionTmaLib;

/// <summary>Информация об объекте миссии из mission TMA.</summary>
/// <param name="Type">Тип объекта: 0 = здание, 1 = бот, 2 = окружение.</param>
/// <param name="UnknownFlags">Неизвестные флаги объекта.</param>
/// <param name="DatString">Путь к DAT ресурсу объекта.</param>
/// <param name="OwningClanIndex">Индекс клана-владельца. Может быть -1, если объект никому не принадлежит. Некоторые объекты окружения иногда почему-то принадлежат клану отличному от -1</param>
/// <param name="Order">Порядок объекта. Для зданий парсер добавляет int.MaxValue.</param>
/// <param name="Position">Позиция объекта.</param>
/// <param name="Rotation">Поворот объекта.</param>
/// <param name="Scale">Масштаб объекта. Для старых feature set может быть принудительно (1, 1, 1).</param>
/// <param name="UnknownString2">Неизвестная строка, например у HERO бывает пустой.</param>
/// <param name="UnknownInt4">Неизвестное целочисленное поле.</param>
/// <param name="UnknownInt5">Неизвестное целочисленное поле.</param>
/// <param name="UnknownInt6">Неизвестное целочисленное поле.</param>
/// <param name="Settings">Настройки объекта.</param>
public record class GameObjectInfo(
    GameObjectType Type,
    int UnknownFlags,
    string DatString,
    int OwningClanIndex,
    int Order,
    Vector3 Position,
    Vector3 Rotation,
    Vector3 Scale,
    string UnknownString2,
    int UnknownInt4,
    int UnknownInt5,
    int UnknownInt6,
    GameObjectSettings Settings);
