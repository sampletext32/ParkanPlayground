using System.Reflection;
using System.Text;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers;
using Xunit;
using Xunit.Abstractions;

namespace X86DisassemblerTests;

/// <summary>
/// Debug test to find missing handler registrations
/// </summary>
public class DebugHandlerRegistration
{
    private readonly ITestOutputHelper _output;

    public DebugHandlerRegistration(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Find which handlers are not registered in the factory
    /// </summary>
    [Fact]
    public void FindMissingHandlers()
    {
        // Create a factory
        byte[] codeBuffer = new byte[1];
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        var sut = new InstructionHandlerFactory(codeBuffer, decoder, codeBuffer.Length);

        // Get the handlers registered in the factory
        var handlers = (List<IInstructionHandler>)sut.GetType()
            .GetField("_handlers", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(sut)!;

        // Get all handler types in the assembly
        var handlerTypes = typeof(InstructionHandler).Assembly.GetExportedTypes()
            .Where(x => x.IsAssignableTo(typeof(InstructionHandler)) && x is { IsAbstract: false, IsInterface: false })
            .ToList();

        // Find missing handlers
        var missingHandlers = new StringBuilder();
        foreach (var handlerType in handlerTypes)
        {
            if (!handlers.Any(x => x.GetType() == handlerType))
            {
                missingHandlers.AppendLine($"Missing handler: {handlerType.FullName}");
            }
        }

        // Output missing handlers
        if (missingHandlers.Length > 0)
        {
            _output.WriteLine("The following handlers are not registered in the factory:");
            _output.WriteLine(missingHandlers.ToString());
            Assert.False(true, "Missing handlers detected");
        }
        else
        {
            _output.WriteLine("All handlers are registered correctly.");
        }
    }
}
