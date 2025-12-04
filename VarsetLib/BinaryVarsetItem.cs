using System.Diagnostics;
using Common;

namespace VarsetLib;

[DebuggerDisplay("{DebugDisplay}")]
public abstract record BinaryVarsetItem()
{
    protected abstract string DebugDisplay { get; }
}

public record BinaryVarsetItem<TValue>(
    int ValueLength, // длина значения
    BinaryVarsetValueType ValueType, // тип значения
    TValue Magic3, // кажется 0 всегда
    string Name, // имя переменной
    string String2, // кажется всегда пусто
    TValue Magic4,
    TValue Magic5,
    TValue Magic6, // минимум
    TValue Magic7 // максимум
) : BinaryVarsetItem
{
    protected override string DebugDisplay => $"{typeof(TValue).Name}, {ValueLength} bytes. magic3: {Magic3}. {Name,-30} {String2} - {Magic4} {Magic5} {Magic6} {Magic7}";
};

public enum BinaryVarsetValueType
{
    Float0 = 0,
    Int = 1,
    Bool = 2,
    Float = 3,
    CharPtr = 4,
    Dword = 5
}