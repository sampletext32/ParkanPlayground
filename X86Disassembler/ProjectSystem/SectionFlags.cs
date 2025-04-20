namespace X86Disassembler.ProjectSystem;

[Flags]
public enum SectionFlags
{
    None = 0,
    Code = 1,
    Exec = 2,
    Read = 4,
    Write = 8
}