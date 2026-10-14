using Common;

namespace MissionTmaLib;

/// <summary>Информация об объекте миссии из mission TMA.</summary>
/// <param name="Type">Тип объекта: 0 = здание, 1 = бот, 2 = окружение.</param>
/// <param name="Flags">Флаги объекта (flags в GameObjectFileData).</param>
/// <param name="DatString">Путь к DAT ресурсу объекта.</param>
/// <param name="OwningClanIndex">Индекс клана-владельца. Может быть -1, если объект никому не принадлежит. Некоторые объекты окружения иногда почему-то принадлежат клану отличному от -1</param>
/// <param name="Order">Порядок объекта. Для зданий парсер добавляет int.MaxValue.</param>
/// <param name="Position">Позиция объекта.</param>
/// <param name="Rotation">Поворот объекта.</param>
/// <param name="Scale">Масштаб объекта. Для старых feature set может быть принудительно (1, 1, 1).</param>
/// <param name="ShortName">Короткое имя объекта / имя схемы (short_name, 12 символов); у HERO бывает пустым.</param>
/// <param name="UnknownInt4">Не выяснено (в данных 0 или 1).</param>
/// <param name="HostBuildingLogicId">LogicalID здания-хозяина (для объектов, привязанных к зданию).</param>
/// <param name="HostSlotIndex">Номер слота в здании-хозяине.</param>
/// <param name="Settings">Настройки объекта.</param>
public record class GameObjectInfo(
    GameObjectType Type,
    int Flags,
    string DatString,
    int OwningClanIndex,
    int Order,
    Vector3 Position,
    Vector3 Rotation,
    Vector3 Scale,
    string ShortName,
    int UnknownInt4,
    int HostBuildingLogicId,
    int HostSlotIndex,
    GameObjectSettings Settings);
