namespace X86Disassembler.PE
{
    /// <summary>
    /// Represents the Export Directory of a PE file
    /// </summary>
    public class ExportDirectory
    {
        public uint Characteristics;       // Reserved, must be 0
        public uint TimeDateStamp;         // Time and date stamp
        public ushort MajorVersion;        // Major version
        public ushort MinorVersion;        // Minor version
        public uint Name;                  // RVA of the name of the DLL
        public string DllName;            // The actual name of the DLL
        public uint Base;                  // Ordinal base
        public uint NumberOfFunctions;     // Number of functions
        public uint NumberOfNames;         // Number of names
        public uint AddressOfFunctions;    // RVA of the export address table
        public uint AddressOfNames;        // RVA of the export names table
        public uint AddressOfNameOrdinals; // RVA of the ordinal table
        
        /// <summary>
        /// Initializes a new instance of the ExportDirectory class
        /// </summary>
        public ExportDirectory()
        {
            // Initialize string field to avoid nullability warning
            DllName = string.Empty;
        }
    }
    
    /// <summary>
    /// Represents an exported function in a PE file
    /// </summary>
    public class ExportedFunction
    {
        public string Name;           // Function name
        public ushort Ordinal;        // Function ordinal
        public uint Address;          // Function RVA
        public bool IsForwarder;      // True if this is a forwarder
        public string ForwarderName;  // Name of the forwarded function
        
        /// <summary>
        /// Initializes a new instance of the ExportedFunction class
        /// </summary>
        public ExportedFunction()
        {
            // Initialize string fields to avoid nullability warnings
            Name = string.Empty;
            ForwarderName = string.Empty;
        }
    }
}
