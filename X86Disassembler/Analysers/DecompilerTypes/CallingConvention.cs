namespace X86Disassembler.Analysers.DecompilerTypes;

/// <summary>
/// Represents a calling convention used by a function
/// </summary>
public enum CallingConvention
{
    /// <summary>
    /// C declaration calling convention (caller cleans the stack)
    /// Parameters are pushed right-to-left
    /// EAX, ECX, EDX are caller-saved
    /// EBX, ESI, EDI, EBP are callee-saved
    /// Return value in EAX (or EDX:EAX for 64-bit values)
    /// </summary>
    Cdecl,
    
    /// <summary>
    /// Standard calling convention (callee cleans the stack)
    /// Parameters are pushed right-to-left
    /// EAX, ECX, EDX are caller-saved
    /// EBX, ESI, EDI, EBP are callee-saved
    /// Return value in EAX (or EDX:EAX for 64-bit values)
    /// </summary>
    Stdcall,
    
    /// <summary>
    /// Fast calling convention
    /// First two parameters in ECX and EDX, rest on stack right-to-left
    /// EAX, ECX, EDX are caller-saved
    /// EBX, ESI, EDI, EBP are callee-saved
    /// Return value in EAX
    /// Callee cleans the stack
    /// </summary>
    Fastcall,
    
    /// <summary>
    /// This calling convention (C++ member functions)
    /// 'this' pointer in ECX, other parameters pushed right-to-left
    /// EAX, ECX, EDX are caller-saved
    /// EBX, ESI, EDI, EBP are callee-saved
    /// Return value in EAX
    /// Caller cleans the stack
    /// </summary>
    Thiscall,
    
    /// <summary>
    /// Microsoft vectorcall convention
    /// First six parameters in registers (XMM0-XMM5 for floating point, ECX, EDX, R8, R9 for integers)
    /// Additional parameters pushed right-to-left
    /// Return value in EAX or XMM0
    /// </summary>
    Vectorcall,
    
    /// <summary>
    /// Unknown calling convention
    /// </summary>
    Unknown
}
