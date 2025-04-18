using X86Disassembler.Analysers;

namespace X86Disassembler.ProjectSystem;

public class ProjectPeFile
{
    public string Name { get; set; }

    public string Architecture { get; set; }

    public Address EntryPointAddress { get; set; }

    public Address ImageBase { get; set; }
}

public class ProjectPeFileSection
{
    public string Name { get; set; }

    public Address VirtualAddress { get; set; }

    public ulong Size { get; set; }

    public SectionFlags Flags { get; set; }
}

[Flags]
public enum SectionFlags
{
    None = 0,
    Code = 1,
    Exec = 2,
    Read = 4,
    Write = 8
}