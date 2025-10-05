namespace VarsetLib;

public class VarsetParser
{
    public static List<VarsetItem> Parse(string path)
    {
        using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return Parse(fs);
    }

    public static List<VarsetItem> Parse(Stream fs)
    {
        try
        {
            var reader = new StreamReader(fs);

            List<VarsetItem> varsetItems = [];

            var lineIndex = 1;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine()!;
                if (line.Length == 0)
                {
                    lineIndex++;
                    continue;
                }

                if (line.StartsWith("//") || line.Trim().StartsWith("//"))
                {
                    lineIndex++;
                    continue;
                }

                if (!line.StartsWith("VAR"))
                {
                    Console.WriteLine($"Error on line: {lineIndex}! Not starting with VAR");
                    lineIndex++;
                    continue;
                }

                var openParenthesisIndex = line.IndexOf("(");
                var closeParenthesisIndex = line.IndexOf(")");

                if (openParenthesisIndex == -1 || closeParenthesisIndex == -1 ||
                    closeParenthesisIndex <= openParenthesisIndex)
                {
                    Console.WriteLine($"Error on line: {lineIndex}! VAR() format invalid");
                    lineIndex++;
                    continue;
                }

                var arguments = line.Substring(openParenthesisIndex + 1,
                    closeParenthesisIndex - openParenthesisIndex - 1);

                var parts = arguments.Trim()
                    .Split(',');

                var type = parts[0]
                    .Trim();

                var name = parts[1]
                    .Trim();

                var value = parts[2]
                    .Trim();

                var item = new VarsetItem(type, name, value);
                varsetItems.Add(item);

                lineIndex++;
            }

            return varsetItems;
        }
        catch
        {
            return [];
        }
    }
}