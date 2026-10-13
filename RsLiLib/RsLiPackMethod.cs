namespace RsLiLib;

/// <summary>
/// Способ упаковки данных записи NL-архива (поле <c>Method</c> записи, маска 0x1E0; <c>rsGetPackMethod</c> возвращает <c>flags &amp; 0x1C0</c>).
/// Значения подтверждены ветвлением <c>rsLoad</c> (Ngi32.dll 0x10013C60).
/// </summary>
public enum RsLiPackMethod
{
    /// <summary>Данные хранятся как есть.</summary>
    None = 0x000,

    /// <summary>Только XOR-шифр (потоковый, ключ - 16-битное поле записи <c>SortIndexOrKey</c>).</summary>
    Xor = 0x020,

    /// <summary>LZSS: кольцевой буфер 4096 байт, длина 3..18, начальная позиция записи 0xFEE (проверено на данных: 2 записи gamefont.rlb).</summary>
    Lzss = 0x040,

    /// <summary>XOR-шифр, затем LZSS.</summary>
    XorLzss = 0x060,

    /// <summary>LZHUF (LZSS + адаптивный Хаффман, N = 4096, F = 60); по коду игры, на данных не проверено - в файлах первой части не встречается.</summary>
    Lzhuf = 0x080,

    /// <summary>XOR-шифр, затем LZHUF.</summary>
    XorLzhuf = 0x0A0,

    /// <summary>Сырой deflate без заголовка zlib (проверено на данных: sprites.lib).</summary>
    Deflate = 0x100
}
