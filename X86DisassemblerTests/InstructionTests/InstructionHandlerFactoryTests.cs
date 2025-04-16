using System.Reflection;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers;

namespace X86DisassemblerTests.InstructionTests;

public class InstructionHandlerFactoryTests
{
    [Fact]
    public void Factory_ShouldNotContainDuplicates()
    {
        byte[] code = new byte[] {0xCC, 0xCC, 0xCC};
        var sut = new InstructionHandlerFactory(code, new InstructionDecoder(code, code.Length), code.Length);

        var handlers = (List<IInstructionHandler>) sut.GetType()
            .GetField("_handlers", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(sut)!;

        var distinctHandlersCount = handlers.Distinct()
            .Count();

        Assert.Equal(distinctHandlersCount, handlers.Count);
    }

    [Fact]
    public void Factory_ShouldContainAllKnownHandlers()
    {
        byte[] code = new byte[] {0xCC, 0xCC, 0xCC};
        var sut = new InstructionHandlerFactory(code, new InstructionDecoder(code, code.Length), code.Length);

        var handlers = (List<IInstructionHandler>) sut.GetType()
            .GetField("_handlers", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(sut)!;

        var handlerTypes = typeof(InstructionHandler).Assembly.GetExportedTypes()
            .Where(x => x.IsAssignableTo(typeof(InstructionHandler)) && x is {IsAbstract: false, IsInterface: false})
            .ToList();
        
        foreach (var handlerType in handlerTypes)
        {
            Assert.Contains(handlers, x => x.GetType() == handlerType);
        }
    }
}