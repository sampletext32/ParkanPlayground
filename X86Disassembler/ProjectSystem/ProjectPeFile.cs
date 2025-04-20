using X86Disassembler.Analysers;

namespace X86Disassembler.ProjectSystem;

public class ProjectPeFile
{
    public string Name { get; set; }

    public string Architecture { get; set; }

    public Address EntryPointAddress { get; set; }

    public Address ImageBase { get; set; }
}