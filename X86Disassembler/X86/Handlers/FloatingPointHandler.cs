namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for floating-point instructions (D8-DF opcodes)
/// </summary>
public class FloatingPointHandler : InstructionHandler
{
    // Floating-point instruction mnemonics based on opcode and ModR/M reg field
    private static readonly string[][] FpuMnemonics = new string[8][];
    
    // Two-byte floating-point instructions
    private static readonly Dictionary<ushort, string> TwoByteInstructions = new Dictionary<ushort, string>();
    
    /// <summary>
    /// Static constructor to initialize the FPU mnemonic tables
    /// </summary>
    static FloatingPointHandler()
    {
        InitializeFpuMnemonics();
        InitializeTwoByteInstructions();
    }
    
    /// <summary>
    /// Initializes the FPU mnemonic tables
    /// </summary>
    private static void InitializeFpuMnemonics()
    {
        // Initialize all tables
        for (int i = 0; i < 8; i++)
        {
            FpuMnemonics[i] = new string[8];
            for (int j = 0; j < 8; j++)
            {
                FpuMnemonics[i][j] = "??";
            }
        }
        
        // D8 opcode - operations on float32
        FpuMnemonics[0][0] = "fadd";
        FpuMnemonics[0][1] = "fmul";
        FpuMnemonics[0][2] = "fcom";
        FpuMnemonics[0][3] = "fcomp";
        FpuMnemonics[0][4] = "fsub";
        FpuMnemonics[0][5] = "fsubr";
        FpuMnemonics[0][6] = "fdiv";
        FpuMnemonics[0][7] = "fdivr";
        
        // D9 opcode - load, store, and control operations
        FpuMnemonics[1][0] = "fld";
        FpuMnemonics[1][2] = "fst";
        FpuMnemonics[1][3] = "fstp";
        FpuMnemonics[1][4] = "fldenv";
        FpuMnemonics[1][5] = "fldcw";
        FpuMnemonics[1][6] = "fnstenv";
        FpuMnemonics[1][7] = "fnstcw";
        
        // DA opcode - operations on int32
        FpuMnemonics[2][0] = "fiadd";
        FpuMnemonics[2][1] = "fimul";
        FpuMnemonics[2][2] = "ficom";
        FpuMnemonics[2][3] = "ficomp";
        FpuMnemonics[2][4] = "fisub";
        FpuMnemonics[2][5] = "fisubr";
        FpuMnemonics[2][6] = "fidiv";
        FpuMnemonics[2][7] = "fidivr";
        
        // DB opcode - load/store int32, misc
        FpuMnemonics[3][0] = "fild";
        FpuMnemonics[3][2] = "fist";
        FpuMnemonics[3][3] = "fistp";
        FpuMnemonics[3][5] = "fld";
        FpuMnemonics[3][7] = "fstp";
        
        // DC opcode - operations on float64
        FpuMnemonics[4][0] = "fadd";
        FpuMnemonics[4][1] = "fmul";
        FpuMnemonics[4][2] = "fcom";
        FpuMnemonics[4][3] = "fcomp";
        FpuMnemonics[4][4] = "fsub";
        FpuMnemonics[4][5] = "fsubr";
        FpuMnemonics[4][6] = "fdiv";
        FpuMnemonics[4][7] = "fdivr";
        
        // DD opcode - load/store float64
        FpuMnemonics[5][0] = "fld";
        FpuMnemonics[5][2] = "fst";
        FpuMnemonics[5][3] = "fstp";
        FpuMnemonics[5][4] = "frstor";
        FpuMnemonics[5][6] = "fnsave";
        FpuMnemonics[5][7] = "fnstsw";
        
        // DE opcode - operations on int16
        FpuMnemonics[6][0] = "fiadd";
        FpuMnemonics[6][1] = "fimul";
        FpuMnemonics[6][2] = "ficom";
        FpuMnemonics[6][3] = "ficomp";
        FpuMnemonics[6][4] = "fisub";
        FpuMnemonics[6][5] = "fisubr";
        FpuMnemonics[6][6] = "fidiv";
        FpuMnemonics[6][7] = "fidivr";
        
        // DF opcode - load/store int16, misc
        FpuMnemonics[7][0] = "fild";
        FpuMnemonics[7][2] = "fist";
        FpuMnemonics[7][3] = "fistp";
        FpuMnemonics[7][4] = "fbld";
        FpuMnemonics[7][5] = "fild";
        FpuMnemonics[7][6] = "fbstp";
        FpuMnemonics[7][7] = "fistp";
    }
    
    /// <summary>
    /// Initializes the two-byte floating-point instructions
    /// </summary>
    private static void InitializeTwoByteInstructions()
    {
        // DF E0 - FNSTSW AX (Store FPU status word to AX without checking for pending unmasked floating-point exceptions)
        TwoByteInstructions.Add(0xDFE0, "fnstsw");
        
        // Add other two-byte instructions as needed
    }
    
    /// <summary>
    /// Initializes a new instance of the FloatingPointHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public FloatingPointHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
        : base(codeBuffer, decoder, length)
    {
    }
    
    /// <summary>
    /// Checks if this handler can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        return opcode >= 0xD8 && opcode <= 0xDF;
    }
    
    /// <summary>
    /// Decodes a floating-point instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Check for two-byte instructions
        if (position < Length)
        {
            // Create a two-byte opcode by combining the primary opcode with the next byte
            ushort twoByteOpcode = (ushort)((opcode << 8) | CodeBuffer[position]);
            
            // Check if this is a known two-byte instruction
            if (TwoByteInstructions.TryGetValue(twoByteOpcode, out string? mnemonic) && mnemonic != null)
            {
                instruction.Mnemonic = mnemonic;
                
                // Special handling for specific instructions
                if (twoByteOpcode == 0xDFE0) // FNSTSW AX
                {
                    instruction.Operands = "ax";
                    Decoder.SetPosition(position + 1); // Skip the second byte
                    return true;
                }
            }
        }
        
        // The opcode index in our tables (0-7 for D8-DF)
        int opcodeIndex = opcode - 0xD8;
        
        // Read the ModR/M byte
        var (mod, reg, rm, operand) = ModRMDecoder.ReadModRM(opcodeIndex == 7); // DF uses 64-bit operands
        
        // Set the mnemonic based on the opcode and reg field
        instruction.Mnemonic = FpuMnemonics[opcodeIndex][reg];
        
        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            instruction.Operands = operand;
        }
        else // Register operand (ST(i))
        {
            // For register operands, we need to handle the stack registers
            // This is a simplified implementation and may need to be expanded
            instruction.Operands = $"st({rm})";
        }
        
        return true;
    }
}
