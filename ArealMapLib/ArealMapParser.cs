using Common;
using NResLib;

namespace ArealMapLib;

/// <summary>Разбор карты ареалов (<c>Land.map</c>). Структура получена из <c>SystemArealMap::Load</c> (<c>ArealMap.dll</c> 0x1001EA60).</summary>
public static class ArealMapParser
{
    /// <summary>Тип записи ArealMap в NRes.</summary>
    private const uint ArealMapTypeId = 0x0C;

    /// <summary>Читает <c>Land.map</c> целиком.</summary>
    /// <param name="path">Путь к файлу <c>Land.map</c>.</param>
    public static ArealMapFile ReadFile(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        var nres = NResParser.ReadFile(fs);
        if (nres.Archive is null)
        {
            throw new ArealMapFormatException($"Не NRes: {nres.Error}");
        }

        var item = nres.Archive.Files.FirstOrDefault(f => f.TypeId == ArealMapTypeId)
                   ?? throw new ArealMapFormatException("В файле нет записи ArealMap (тип 0x0C)");

        fs.Position = item.OffsetInFile;
        var data = fs.ReadBytesExact(item.FileLength);

        using var ms = new MemoryStream(data, writable: false);
        return Parse(ms, (int) item.ElementCount, item.FileLength);
    }

    /// <summary>Разбирает содержимое записи ArealMap.</summary>
    /// <param name="stream">Поток с данными записи (с её начала).</param>
    /// <param name="arealCount">Число ареалов: <c>ElementCount</c> записи каталога NRes.</param>
    /// <param name="length">Длина записи в байтах (<c>FileLength</c>): после чтения всё должно быть использовано.</param>
    public static ArealMapFile Parse(Stream stream, int arealCount, int length)
    {
        var areals = new ArealRecord[arealCount];
        for (var i = 0; i < arealCount; i++)
        {
            var center = stream.ReadVector3LittleEndian();
            _ = stream.ReadInt32LittleEndian(); // +0x0C: всегда 0
            var area = stream.ReadFloatLittleEndian();
            var normal = stream.ReadVector3LittleEndian();
            var flag = stream.ReadInt32LittleEndian();
            _ = stream.ReadInt32LittleEndian(); // +0x24: всегда 0
            var mask = stream.ReadInt32LittleEndian();
            _ = stream.ReadInt32LittleEndian(); // +0x2C: всегда 0
            var vertexCount = stream.ReadInt32LittleEndian();
            var linkCount = stream.ReadInt32LittleEndian(); // +0x34: игра читает за рёбрами ещё блоки связей; во всех картах 0

            if (vertexCount < 3 || vertexCount > 4096)
            {
                throw new ArealMapFormatException($"Ареал {i}: недопустимое число вершин {vertexCount}");
            }

            if (linkCount != 0)
            {
                throw new ArealMapFormatException($"Ареал {i}: число связей {linkCount} не равно 0 - такой вариант не встречается в данных игры и не разобран");
            }

            var vertices = new Vector3[vertexCount];
            for (var v = 0; v < vertexCount; v++)
            {
                vertices[v] = stream.ReadVector3LittleEndian();
            }

            var edges = new ArealEdge[vertexCount];
            for (var v = 0; v < vertexCount; v++)
            {
                var neighbour = stream.ReadInt32LittleEndian();
                var neighbourEdge = stream.ReadInt32LittleEndian();
                edges[v] = new ArealEdge(neighbour, neighbourEdge);
            }

            areals[i] = new ArealRecord(center, area, normal, flag, mask, vertices, edges);
        }

        var width = stream.ReadInt32LittleEndian();
        var height = stream.ReadInt32LittleEndian();
        if (width <= 0 || height <= 0 || (long) width * height > 1_000_000)
        {
            throw new ArealMapFormatException($"Недопустимый размер сетки {width} x {height}");
        }

        var cells = new ushort[width * height][];
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var n = stream.ReadUInt16LittleEndian();
                var list = new ushort[n];
                for (var k = 0; k < n; k++)
                {
                    list[k] = stream.ReadUInt16LittleEndian();
                    if (list[k] >= arealCount)
                    {
                        throw new ArealMapFormatException($"Ячейка ({x}, {y}): индекс ареала {list[k]} вне диапазона 0..{arealCount - 1}");
                    }
                }

                cells[x * height + y] = list;
            }
        }

        if (stream.Position != length)
        {
            throw new ArealMapFormatException($"Прочитано {stream.Position} байт из {length}");
        }

        return new ArealMapFile(areals, width, height, cells);
    }
}
