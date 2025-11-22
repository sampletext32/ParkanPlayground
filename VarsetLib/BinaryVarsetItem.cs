using Common;

namespace VarsetLib;

public record BinaryVarsetItem(
    int Magic1, // length of something
    IntFloatValue Magic2,
    IntFloatValue Magic3,
    string Name,
    string String2,
    IntFloatValue Magic4,
    IntFloatValue Magic5,
    IntFloatValue Magic6,
    IntFloatValue Magic7
);
