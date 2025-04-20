using X86Disassembler.Analysers;
using X86Disassembler.PE;
using X86Disassembler.ProjectSystem;

namespace X86Disassembler;

public class Program
{
    private const string FilePath = @"C:\Program Files (x86)\Nikita\Iron Strategy\Terrain.dll";

    public static void Main(string[] args)
    {
        byte[] fileBytes = File.ReadAllBytes(FilePath);
        PeFile peFile = new PeFile(fileBytes);
        peFile.Parse();

        var projectPeFile = new ProjectPeFile()
        {
            ImageBase = new VirtualAddress(0, peFile.OptionalHeader.ImageBase),
            Architecture = peFile.OptionalHeader.Is64Bit()
                ? "64-bit"
                : "32-bit",
            Name = Path.GetFileName(FilePath),
            EntryPointAddress = new FileAbsoluteAddress(peFile.OptionalHeader.AddressOfEntryPoint, peFile.OptionalHeader.ImageBase)
        };
    }
}