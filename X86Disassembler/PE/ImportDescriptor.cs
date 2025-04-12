using System.Collections.Generic;

namespace X86Disassembler.PE
{
    /// <summary>
    /// Represents an Import Descriptor in a PE file
    /// </summary>
    public class ImportDescriptor
    {
        public uint OriginalFirstThunk;  // RVA to original first thunk
        public uint TimeDateStamp;       // Time and date stamp
        public uint ForwarderChain;      // Forwarder chain
        public uint Name;                // RVA to the name of the DLL
        public string DllName;          // The actual name of the DLL
        public uint FirstThunk;          // RVA to first thunk
        
        public List<ImportedFunction> Functions { get; } = new List<ImportedFunction>();
        
        /// <summary>
        /// Initializes a new instance of the ImportDescriptor class
        /// </summary>
        public ImportDescriptor()
        {
            // Initialize string field to avoid nullability warning
            DllName = string.Empty;
        }
    }
}
