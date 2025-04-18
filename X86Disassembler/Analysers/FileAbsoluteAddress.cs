namespace X86Disassembler.Analysers;

public abstract class Address(ulong value, ulong imageBase)
{
    /// <summary>
    /// The actual value of the address, not specifically typed.
    /// </summary>
    protected readonly ulong Value = value;
    
    /// <summary>
    /// PE.ImageBase from which this address is constructed
    /// </summary>
    protected readonly ulong ImageBase = imageBase;
}

/// <summary>
/// Absolute address in the PE file
/// </summary>
public class FileAbsoluteAddress(ulong value, ulong imageBase) : Address(value, imageBase)
{
    public ulong GetValue()
    {
        return Value;
    }

    public virtual VirtualAddress AsImageBaseAddress()
    {
        return new VirtualAddress(Value + ImageBase, ImageBase);
    }

    public virtual FileAbsoluteAddress AsFileAbsolute()
    {
        return this;
    }
}

/// <summary>
/// Address from PE.ImageBase
/// </summary>
public class VirtualAddress : FileAbsoluteAddress
{
    public VirtualAddress(ulong value, ulong imageBase) : base(value, imageBase)
    {
    }

    public override VirtualAddress AsImageBaseAddress()
    {
        return this;
    }

    public override FileAbsoluteAddress AsFileAbsolute()
    {
        return new FileAbsoluteAddress(Value - ImageBase, ImageBase);
    }
}

