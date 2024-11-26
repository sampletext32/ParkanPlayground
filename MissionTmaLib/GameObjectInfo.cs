namespace MissionTmaLib;

public class GameObjectInfo
{
    // 0 - здание, 1 - бот, 2 - окружение
    public GameObjectType Type { get; set; }

    public int UnknownFlags { get; set; }

    public string DatString { get; set; }

    /// <summary>
    /// Индекс клана, которому принадлежит объект
    /// </summary>
    /// <remarks>
    /// <para>
    /// Некоторые объекты окружения иногда почему-то принадлежат клану отличному от -1
    /// </para>
    /// <para>
    /// Может быть -1, если объект никому не принадлежит, я такое встречал только у объектов окружения
    /// </para>
    /// </remarks>
    public int OwningClanIndex { get; set; }

    public int UnknownInt3 { get; set; }

    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public string UnknownString2 { get; set; }

    public int UnknownInt4 { get; set; }
    public int UnknownInt5 { get; set; }
    public int UnknownInt6 { get; set; }

    public GameObjectSettings Settings { get; set; }
}