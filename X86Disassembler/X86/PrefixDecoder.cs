namespace X86Disassembler.X86;

/// <summary>
/// Handles decoding of instruction prefixes
/// </summary>
public class PrefixDecoder
{
    // Prefix flags
    private bool _operandSizePrefix;
    private bool _addressSizePrefix;
    private bool _segmentOverridePrefix;
    private bool _lockPrefix;
    private bool _repPrefix;
    private string _segmentOverride = string.Empty;
    
    /// <summary>
    /// Initializes a new instance of the PrefixDecoder class
    /// </summary>
    public PrefixDecoder()
    {
        Reset();
    }
    
    /// <summary>
    /// Resets all prefix flags
    /// </summary>
    public void Reset()
    {
        _operandSizePrefix = false;
        _addressSizePrefix = false;
        _segmentOverridePrefix = false;
        _lockPrefix = false;
        _repPrefix = false;
        _segmentOverride = string.Empty;
    }
    
    /// <summary>
    /// Decodes a prefix byte
    /// </summary>
    /// <param name="prefix">The prefix byte</param>
    /// <returns>True if the byte was a prefix, false otherwise</returns>
    public bool DecodePrefix(byte prefix)
    {
        if (prefix == 0x66) // Operand size prefix
        {
            _operandSizePrefix = true;
            return true;
        }
        else if (prefix == 0x67) // Address size prefix
        {
            _addressSizePrefix = true;
            return true;
        }
        else if ((prefix >= 0x26 && prefix <= 0x3E && (prefix & 0x7) == 0x6) || prefix == 0x64 || prefix == 0x65) // Segment override prefix
        {
            _segmentOverridePrefix = true;
            switch (prefix)
            {
                case 0x26: _segmentOverride = "es"; break;
                case 0x2E: _segmentOverride = "cs"; break;
                case 0x36: _segmentOverride = "ss"; break;
                case 0x3E: _segmentOverride = "ds"; break;
                case 0x64: _segmentOverride = "fs"; break;
                case 0x65: _segmentOverride = "gs"; break;
            }
            return true;
        }
        else if (prefix == 0xF0) // LOCK prefix
        {
            _lockPrefix = true;
            return true;
        }
        else if (prefix == 0xF2 || prefix == 0xF3) // REP/REPNE prefix
        {
            _repPrefix = true;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if the operand size prefix is present
    /// </summary>
    /// <returns>True if the operand size prefix is present</returns>
    public bool HasOperandSizePrefix()
    {
        return _operandSizePrefix;
    }
    
    /// <summary>
    /// Checks if the address size prefix is present
    /// </summary>
    /// <returns>True if the address size prefix is present</returns>
    public bool HasAddressSizePrefix()
    {
        return _addressSizePrefix;
    }
    
    /// <summary>
    /// Checks if a segment override prefix is present
    /// </summary>
    /// <returns>True if a segment override prefix is present</returns>
    public bool HasSegmentOverridePrefix()
    {
        return _segmentOverridePrefix;
    }
    
    /// <summary>
    /// Gets the segment override prefix
    /// </summary>
    /// <returns>The segment override prefix, or an empty string if none is present</returns>
    public string GetSegmentOverride()
    {
        return _segmentOverride;
    }
    
    /// <summary>
    /// Checks if the LOCK prefix is present
    /// </summary>
    /// <returns>True if the LOCK prefix is present</returns>
    public bool HasLockPrefix()
    {
        return _lockPrefix;
    }
    
    /// <summary>
    /// Checks if the REP/REPNE prefix is present
    /// </summary>
    /// <returns>True if the REP/REPNE prefix is present</returns>
    public bool HasRepPrefix()
    {
        return _repPrefix;
    }
    
    /// <summary>
    /// Applies the segment override prefix to the operands string if applicable
    /// </summary>
    /// <param name="operands">The operands string</param>
    /// <returns>The operands string with segment override applied</returns>
    public string ApplySegmentOverride(string operands)
    {
        if (_segmentOverridePrefix && !string.IsNullOrEmpty(operands))
        {
            // If the instruction has memory operands, add the segment override
            if (operands.Contains("["))
            {
                // Replace the first '[' with the segment override
                return operands.Replace("[", $"{_segmentOverride}:[" );
            }
        }
        
        return operands;
    }
    
    /// <summary>
    /// Applies the REP prefix to the mnemonic if applicable
    /// </summary>
    /// <param name="mnemonic">The mnemonic</param>
    /// <returns>The mnemonic with REP prefix applied</returns>
    public string ApplyRepPrefix(string mnemonic)
    {
        if (_repPrefix && !mnemonic.StartsWith("rep"))
        {
            return $"rep {mnemonic}";
        }
        
        return mnemonic;
    }
}
