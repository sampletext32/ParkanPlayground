using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace X86Disassembler.PE.Parsers
{
    /// <summary>
    /// Parser for Import Descriptors in a PE file
    /// </summary>
    public class ImportDescriptorParser
    {
        private readonly PEUtility _utility;
        
        public ImportDescriptorParser(PEUtility utility)
        {
            _utility = utility;
        }
        
        /// <summary>
        /// Parse the Import Descriptors from the binary reader
        /// </summary>
        /// <param name="reader">The binary reader</param>
        /// <param name="rva">The RVA of the Import Directory</param>
        /// <returns>List of Import Descriptors</returns>
        public List<ImportDescriptor> Parse(BinaryReader reader, uint rva)
        {
            List<ImportDescriptor> descriptors = new List<ImportDescriptor>();
            
            try
            {
                uint importTableOffset = _utility.RvaToOffset(rva);
                reader.BaseStream.Seek(importTableOffset, SeekOrigin.Begin);
                
                int descriptorCount = 0;
                
                while (true)
                {
                    descriptorCount++;
                    
                    // Read the import descriptor
                    uint originalFirstThunk = reader.ReadUInt32();
                    uint timeDateStamp = reader.ReadUInt32();
                    uint forwarderChain = reader.ReadUInt32();
                    uint nameRva = reader.ReadUInt32();
                    uint firstThunk = reader.ReadUInt32();
                    
                    // Check if we've reached the end of the import descriptors
                    if (originalFirstThunk == 0 && nameRva == 0 && firstThunk == 0)
                    {
                        break;
                    }
                    
                    ImportDescriptor descriptor = new ImportDescriptor
                    {
                        OriginalFirstThunk = originalFirstThunk,
                        TimeDateStamp = timeDateStamp,
                        ForwarderChain = forwarderChain,
                        Name = nameRva,
                        FirstThunk = firstThunk,
                        DllName = "Unknown" // Default name in case we can't read it
                    };
                    
                    // Try to read the DLL name
                    try
                    {
                        if (nameRva != 0)
                        {
                            uint nameOffset = _utility.RvaToOffset(nameRva);
                            reader.BaseStream.Seek(nameOffset, SeekOrigin.Begin);
                            
                            // Read the null-terminated ASCII string
                            StringBuilder nameBuilder = new StringBuilder();
                            byte b;
                            
                            while ((b = reader.ReadByte()) != 0)
                            {
                                nameBuilder.Append((char)b);
                            }
                            
                            descriptor.DllName = nameBuilder.ToString();
                        }
                    }
                    catch (Exception)
                    {
                        // If we can't read the name, keep the default "Unknown"
                    }
                    
                    // Parse the imported functions
                    ParseImportedFunctions(reader, descriptor);
                    
                    descriptors.Add(descriptor);
                    
                    // Return to the import table to read the next descriptor
                    reader.BaseStream.Seek(importTableOffset + (descriptorCount * 20), SeekOrigin.Begin);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing import descriptors: {ex.Message}");
                // Return whatever descriptors we've managed to parse
            }
            
            return descriptors;
        }
        
        /// <summary>
        /// Parse the imported functions for a given import descriptor
        /// </summary>
        /// <param name="reader">The binary reader</param>
        /// <param name="descriptor">The Import Descriptor</param>
        private void ParseImportedFunctions(BinaryReader reader, ImportDescriptor descriptor)
        {
            try
            {
                // Use OriginalFirstThunk if available, otherwise use FirstThunk
                uint thunkRva = descriptor.OriginalFirstThunk != 0 ? descriptor.OriginalFirstThunk : descriptor.FirstThunk;
                
                if (thunkRva == 0)
                {
                    return; // No functions to parse
                }
                
                uint thunkOffset = _utility.RvaToOffset(thunkRva);
                int functionCount = 0;
                
                while (true)
                {
                    reader.BaseStream.Seek(thunkOffset + (functionCount * 4), SeekOrigin.Begin);
                    uint thunkData = reader.ReadUInt32();
                    
                    if (thunkData == 0)
                    {
                        break; // End of the function list
                    }
                    
                    ImportedFunction function = new ImportedFunction
                    {
                        ThunkRVA = thunkRva + (uint)(functionCount * 4)
                    };
                    
                    // Check if imported by ordinal (high bit set)
                    if ((thunkData & 0x80000000) != 0)
                    {
                        function.IsOrdinal = true;
                        function.Ordinal = (ushort)(thunkData & 0xFFFF);
                        function.Name = $"Ordinal {function.Ordinal}";
                    }
                    else
                    {
                        // Imported by name - the thunkData is an RVA to a hint/name structure
                        try
                        {
                            uint hintNameOffset = _utility.RvaToOffset(thunkData);
                            reader.BaseStream.Seek(hintNameOffset, SeekOrigin.Begin);
                            
                            // Read the hint (2 bytes)
                            function.Hint = reader.ReadUInt16();
                            
                            // Read the function name (null-terminated ASCII string)
                            StringBuilder nameBuilder = new StringBuilder();
                            byte b;
                            
                            while ((b = reader.ReadByte()) != 0)
                            {
                                nameBuilder.Append((char)b);
                            }
                            
                            function.Name = nameBuilder.ToString();
                            
                            if (string.IsNullOrEmpty(function.Name))
                            {
                                function.Name = $"Function_at_{thunkData:X8}";
                            }
                        }
                        catch (Exception)
                        {
                            function.Name = $"Function_at_{thunkData:X8}";
                        }
                    }
                    
                    descriptor.Functions.Add(function);
                    functionCount++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing imported functions for {descriptor.DllName}: {ex.Message}");
            }
        }
    }
}
