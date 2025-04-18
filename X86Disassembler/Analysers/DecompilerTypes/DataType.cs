namespace X86Disassembler.Analysers.DecompilerTypes;

/// <summary>
/// Represents a data type in decompiled code
/// </summary>
public class DataType
{
    /// <summary>
    /// The category of the data type
    /// </summary>
    public enum TypeCategory
    {
        /// <summary>
        /// Unknown type
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Void type (no value)
        /// </summary>
        Void,
        
        /// <summary>
        /// Integer type
        /// </summary>
        Integer,
        
        /// <summary>
        /// Floating point type
        /// </summary>
        Float,
        
        /// <summary>
        /// Pointer type
        /// </summary>
        Pointer,
        
        /// <summary>
        /// Structure type
        /// </summary>
        Struct,
        
        /// <summary>
        /// Array type
        /// </summary>
        Array,
        
        /// <summary>
        /// Function type
        /// </summary>
        Function
    }
    
    /// <summary>
    /// The name of the type
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The category of the type
    /// </summary>
    public TypeCategory Category { get; set; }
    
    /// <summary>
    /// The size of the type in bytes
    /// </summary>
    public int Size { get; set; }
    
    /// <summary>
    /// Whether the type is signed (for integer types)
    /// </summary>
    public bool IsSigned { get; set; }
    
    /// <summary>
    /// The pointed-to type (for pointer types)
    /// </summary>
    public DataType? PointedType { get; set; }
    
    /// <summary>
    /// The element type (for array types)
    /// </summary>
    public DataType? ElementType { get; set; }
    
    /// <summary>
    /// The number of elements (for array types)
    /// </summary>
    public int? ElementCount { get; set; }
    
    /// <summary>
    /// The fields of the structure (for struct types)
    /// </summary>
    public List<StructField> Fields { get; set; } = [];
    
    /// <summary>
    /// Creates a new data type with the specified name and category
    /// </summary>
    /// <param name="name">The name of the type</param>
    /// <param name="category">The category of the type</param>
    /// <param name="size">The size of the type in bytes</param>
    public DataType(string name, TypeCategory category, int size)
    {
        Name = name;
        Category = category;
        Size = size;
    }
    
    /// <summary>
    /// Returns a string representation of the type
    /// </summary>
    public override string ToString()
    {
        return Name;
    }
    
    /// <summary>
    /// Creates a pointer type to the specified type
    /// </summary>
    /// <param name="pointedType">The type being pointed to</param>
    /// <returns>A new pointer type</returns>
    public static DataType CreatePointerType(DataType pointedType)
    {
        return new DataType($"{pointedType.Name}*", TypeCategory.Pointer, 4)
        {
            PointedType = pointedType
        };
    }
    
    /// <summary>
    /// Creates an array type of the specified element type and count
    /// </summary>
    /// <param name="elementType">The type of the array elements</param>
    /// <param name="count">The number of elements in the array</param>
    /// <returns>A new array type</returns>
    public static DataType CreateArrayType(DataType elementType, int count)
    {
        return new DataType($"{elementType.Name}[{count}]", TypeCategory.Array, elementType.Size * count)
        {
            ElementType = elementType,
            ElementCount = count
        };
    }
    
    /// <summary>
    /// Common predefined types
    /// </summary>
    public static readonly DataType Unknown = new DataType("unknown", TypeCategory.Unknown, 0);
    public static readonly DataType Void = new DataType("void", TypeCategory.Void, 0);
    public static readonly DataType Char = new DataType("char", TypeCategory.Integer, 1) { IsSigned = true };
    public static readonly DataType UChar = new DataType("unsigned char", TypeCategory.Integer, 1);
    public static readonly DataType Short = new DataType("short", TypeCategory.Integer, 2) { IsSigned = true };
    public static readonly DataType UShort = new DataType("unsigned short", TypeCategory.Integer, 2);
    public static readonly DataType Int = new DataType("int", TypeCategory.Integer, 4) { IsSigned = true };
    public static readonly DataType UInt = new DataType("unsigned int", TypeCategory.Integer, 4);
    public static readonly DataType Float = new DataType("float", TypeCategory.Float, 4);
    public static readonly DataType Double = new DataType("double", TypeCategory.Float, 8);
}

/// <summary>
/// Represents a field in a structure
/// </summary>
public class StructField
{
    /// <summary>
    /// The name of the field
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of the field
    /// </summary>
    public DataType Type { get; set; } = DataType.Unknown;
    
    /// <summary>
    /// The offset of the field within the structure
    /// </summary>
    public int Offset { get; set; }
    
    /// <summary>
    /// Creates a new structure field
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="type">The type of the field</param>
    /// <param name="offset">The offset of the field within the structure</param>
    public StructField(string name, DataType type, int offset)
    {
        Name = name;
        Type = type;
        Offset = offset;
    }
}
