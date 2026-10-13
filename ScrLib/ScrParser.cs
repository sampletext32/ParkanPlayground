using Common;

namespace ScrLib;

public class ScrParser
{
    public static ScrFile ReadFile(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return ReadFile(fs);
    }

    public static ScrFile ReadFile(Stream fs)
    {
        var magic = fs.ReadInt32LittleEndian();
        var entryCount = fs.ReadInt32LittleEndian();
        List<ScrEntry> entries = [];

        for (var i = 0; i < entryCount; i++)
        {
            var title = fs.ReadLengthPrefixedString();

            // тут игра дополнительно вычитывает ещё 1 байт, видимо как \0 для char*
            fs.ReadByte();

            var index = fs.ReadInt32LittleEndian();
            var instructionCount = fs.ReadInt32LittleEndian();
            List<ScrInstruction> instructions = [];
            for (var i1 = 0; i1 < instructionCount; i1++)
            {
                var nativeActionIndex = fs.ReadInt32LittleEndian();
                var resultVarIndex = fs.ReadInt32LittleEndian();
                var valueOrSourceVarIndex = fs.ReadInt32LittleEndian();
                var type = (ScrInstructionType)fs.ReadInt32LittleEndian();
                var compareOperator = fs.ReadInt32LittleEndian();
                var argumentsCount = fs.ReadInt32LittleEndian();
                List<int> arguments = [];

                for (var i2 = 0; i2 < argumentsCount; i2++)
                {
                    arguments.Add(fs.ReadInt32LittleEndian());
                }

                var fmlFunctionIndex = fs.ReadInt32LittleEndian();
                instructions.Add(new ScrInstruction(
                    nativeActionIndex,
                    resultVarIndex,
                    valueOrSourceVarIndex,
                    type,
                    compareOperator,
                    argumentsCount,
                    arguments,
                    fmlFunctionIndex));
            }

            entries.Add(new ScrEntry(title, index, instructionCount, instructions));
        }

        return new ScrFile(magic, entryCount, entries);
    }
}
