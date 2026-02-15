namespace ResTreeLib;

public record ResearchNodeData
{
    public int Index { get; init; }
    public Trf0Element Node { get; init; } = null!;
    public NodeState State { get; init; } // From TRF1
    public string ShortName { get; set; } = "";
    public string LongName { get; init; } = "";
    public string HelpText { get; set; } = "";
    public string Description { get; init; } = "";
    
    /// <summary>
    /// These IDs define what technologies a player must complete before the current node becomes available for research.
    /// These are derived from TRF2 (which stores the count of parents) and TRF3 (which contains the flat list of parent IDs).
    /// </summary>
    /// <example>
    /// If Flamethrower Mk II has PrerequisiteIds = [18], then the node with index 18 (Flamethrower Mk I) must be finished first.
    /// </example>
    public uint[] PrerequisiteIds { get; init; } = []; // From TRF2/3
    
    /// <summary>
    /// These IDs define the immediate "downstream" effects or technologies that are triggered when the current research is finished.
    /// These are derived from TRF4 (the count of unlocks) and TRF5 (the flat list of target IDs).
    /// </summary>
    public uint[] UnlockIds { get; init; } = [];       // From TRF4/5
}
