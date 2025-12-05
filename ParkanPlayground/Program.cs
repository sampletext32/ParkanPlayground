using ParkanPlayground;

// ========== MSH CONVERTER TEST - AUTO-DETECTING MODEL VS LANDSCAPE ==========

var converter = new MshConverter();

// Test with landscape
Console.WriteLine("=".PadRight(60, '='));
converter.Convert(
    @"C:\Program Files (x86)\Nikita\Iron Strategy\DATA\MAPS\SC_1\Land.msh",
    "landscape_lod0.obj",
    lodLevel: 0
);

Console.WriteLine();

// Test with landscape LOD 1 (lower detail)
Console.WriteLine("=".PadRight(60, '='));
converter.Convert(
    @"C:\Program Files (x86)\Nikita\Iron Strategy\DATA\MAPS\SC_1\Land.msh",
    "landscape_lod1.obj",
    lodLevel: 1
);

Console.WriteLine();

// Test with model
Console.WriteLine("=".PadRight(60, '='));
converter.Convert(
    @"E:\ParkanUnpacked\fortif.rlb\133_fr_m_bunker.msh",
    "bunker_lod0.obj",
    lodLevel: 0
);