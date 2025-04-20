using X86Disassembler.Analysers;

namespace X86Disassembler.ProjectSystem;

public class ProjectPeFileSection
{
    public string Name { get; set; }

    public Address VirtualAddress { get; set; }

    public ulong Size { get; set; }

    public SectionFlags Flags { get; set; }
}