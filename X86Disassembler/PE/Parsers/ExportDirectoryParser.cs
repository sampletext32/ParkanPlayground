using System.Text;

namespace X86Disassembler.PE.Parsers;

/// <summary>
/// Parser for the Export Directory of a PE file
/// </summary>
public class ExportDirectoryParser
{
    private readonly PEUtility _utility;
    
    public ExportDirectoryParser(PEUtility utility)
    {
        _utility = utility;
    }
    
    /// <summary>
    /// Parse the Export Directory from the binary reader
    /// </summary>
    /// <param name="reader">The binary reader</param>
    /// <param name="rva">The RVA of the Export Directory</param>
    /// <returns>The parsed Export Directory</returns>
    public ExportDirectory Parse(BinaryReader reader, uint rva)
    {
        ExportDirectory directory = new ExportDirectory();
        
        reader.BaseStream.Seek(_utility.RvaToOffset(rva), SeekOrigin.Begin);
        
        directory.Characteristics = reader.ReadUInt32();
        directory.TimeDateStamp = reader.ReadUInt32();
        directory.MajorVersion = reader.ReadUInt16();
        directory.MinorVersion = reader.ReadUInt16();
        directory.DllNameRva = reader.ReadUInt32();
        directory.Base = reader.ReadUInt32();
        directory.NumberOfFunctions = reader.ReadUInt32();
        directory.NumberOfNames = reader.ReadUInt32();
        directory.AddressOfFunctions = reader.ReadUInt32();
        directory.AddressOfNames = reader.ReadUInt32();
        directory.AddressOfNameOrdinals = reader.ReadUInt32();
        
        // Read the DLL name
        try
        {
            uint dllNameRVA = directory.DllNameRva;
            uint dllNameOffset = _utility.RvaToOffset(dllNameRVA);
            reader.BaseStream.Seek(dllNameOffset, SeekOrigin.Begin);
            
            // Read the null-terminated ASCII string
            StringBuilder nameBuilder = new StringBuilder();
            byte b;
            
            while ((b = reader.ReadByte()) != 0)
            {
                nameBuilder.Append((char)b);
            }
            
            directory.DllName = nameBuilder.ToString();
        }
        catch (Exception)
        {
            directory.DllName = "Unknown";
        }
        
        return directory;
    }
    
    /// <summary>
    /// Parse the exported functions using the export directory information
    /// </summary>
    /// <param name="reader">The binary reader</param>
    /// <param name="directory">The Export Directory</param>
    /// <param name="exportDirRva">The RVA of the Export Directory</param>
    /// <param name="exportDirSize">The size of the Export Directory</param>
    /// <returns>List of exported functions</returns>
    public List<ExportedFunction> ParseExportedFunctions(BinaryReader reader, ExportDirectory directory, uint exportDirRva, uint exportDirSize)
    {
        List<ExportedFunction> exportedFunctions = new List<ExportedFunction>();
        
        if (directory == null)
        {
            return exportedFunctions;
        }
        
        // Read the array of function addresses (RVAs)
        uint[] functionRVAs = new uint[directory.NumberOfFunctions];
        reader.BaseStream.Seek(_utility.RvaToOffset(directory.AddressOfFunctions), SeekOrigin.Begin);
        for (int i = 0; i < directory.NumberOfFunctions; i++)
        {
            functionRVAs[i] = reader.ReadUInt32();
        }
        
        // Read the array of name RVAs
        uint[] nameRVAs = new uint[directory.NumberOfNames];
        reader.BaseStream.Seek(_utility.RvaToOffset(directory.AddressOfNames), SeekOrigin.Begin);
        for (int i = 0; i < directory.NumberOfNames; i++)
        {
            nameRVAs[i] = reader.ReadUInt32();
        }
        
        // Read the array of name ordinals
        ushort[] nameOrdinals = new ushort[directory.NumberOfNames];
        reader.BaseStream.Seek(_utility.RvaToOffset(directory.AddressOfNameOrdinals), SeekOrigin.Begin);
        for (int i = 0; i < directory.NumberOfNames; i++)
        {
            nameOrdinals[i] = reader.ReadUInt16();
        }
        
        // Create a dictionary to map ordinals to names
        Dictionary<ushort, string> ordinalToName = new Dictionary<ushort, string>();
        for (int i = 0; i < directory.NumberOfNames; i++)
        {
            // Read the function name
            reader.BaseStream.Seek(_utility.RvaToOffset(nameRVAs[i]), SeekOrigin.Begin);
            List<byte> nameBytes = new List<byte>();
            byte b;
            while ((b = reader.ReadByte()) != 0)
            {
                nameBytes.Add(b);
            }
            string name = Encoding.ASCII.GetString(nameBytes.ToArray());
            
            // Map the ordinal to the name
            ordinalToName[nameOrdinals[i]] = name;
        }
        
        // Create the exported functions
        for (ushort i = 0; i < directory.NumberOfFunctions; i++)
        {
            uint functionRVA = functionRVAs[i];
            if (functionRVA == 0)
            {
                continue; // Skip empty entries
            }
            
            ExportedFunction function = new ExportedFunction();
            function.Ordinal = (ushort)(i + directory.Base);
            function.AddressRva = functionRVA;
            
            // Check if this function has a name
            if (ordinalToName.TryGetValue(i, out string? name))
            {
                function.Name = name ?? $"Ordinal_{function.Ordinal}";
            }
            else
            {
                function.Name = $"Ordinal_{function.Ordinal}";
            }
            
            // Check if this is a forwarder
            uint exportDirEnd = exportDirRva + exportDirSize;
            
            if (functionRVA >= exportDirRva && functionRVA < exportDirEnd)
            {
                function.IsForwarder = true;
                
                // Read the forwarder string
                reader.BaseStream.Seek(_utility.RvaToOffset(functionRVA), SeekOrigin.Begin);
                List<byte> forwarderBytes = new List<byte>();
                byte b;
                while ((b = reader.ReadByte()) != 0)
                {
                    forwarderBytes.Add(b);
                }
                function.ForwarderName = Encoding.ASCII.GetString(forwarderBytes.ToArray());
            }
            
            exportedFunctions.Add(function);
        }
        
        return exportedFunctions;
    }
}
