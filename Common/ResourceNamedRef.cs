namespace Common;

/// <summary>
/// Ссылка на элемент архива NRes: имя архива (32 байта) и имя элемента (32 байта), 64 байта подряд.
/// Встречается в записях агентов, CTL/NDP (ресурсы предметов и команд), FXID (ресурсы компонентов) и EXPL (эффекты по материалу).
/// </summary>
/// <param name="ArchiveName">Имя архива NRes (например, <c>effects.rlb</c>).</param>
/// <param name="ResourceName">Имя элемента внутри архива.</param>
/// <remarks>
/// Обе строки оканчиваются нулём; байты после первого нуля в файлах не очищены и содержат мусор
/// (например, float 2.0234 в буфере имени архива у компонентов FXID типа 3), поэтому значима только часть до нуля.
/// </remarks>
public readonly record struct ResourceNamedRef(string ArchiveName, string ResourceName)
{
    /// <summary>Размер записи в файле: два буфера по 32 байта.</summary>
    public const int Size = 0x40;

    /// <summary>Размер одного буфера имени в байтах.</summary>
    public const int NameBufferSize = 32;

    /// <summary>Ссылка пуста: оба имени пустые (ресурс не задан).</summary>
    public bool IsEmpty => string.IsNullOrEmpty(ArchiveName) && string.IsNullOrEmpty(ResourceName);

    /// <summary>Читает ссылку из 64 байт: имя архива, затем имя элемента, каждое до первого нуля.</summary>
    /// <param name="data">Не менее <see cref="Size"/> байт.</param>
    public static ResourceNamedRef Read(ReadOnlySpan<byte> data) =>
        new(Extensions.ReadFixedString(data[..NameBufferSize]), Extensions.ReadFixedString(data.Slice(NameBufferSize, NameBufferSize)));

    /// <inheritdoc />
    public override string ToString() => IsEmpty ? "<empty>" : $"{ArchiveName} / {ResourceName}";
}
