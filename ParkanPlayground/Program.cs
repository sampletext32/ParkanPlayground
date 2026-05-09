using ResTreeLib;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
var trfPath = Directory.EnumerateFiles(@"C:\Program Files (x86)\Nikita\Iron Strategy\MISSIONS\SCRIPTS", "*.trf").First();
var nodes = ResTreeParser.Parse(trfPath);

var active = nodes.Where(n => !string.IsNullOrWhiteSpace(n.ShortName) && (n.Node.ResearchCost > 0 || n.Node.ResearchTime > 0)).ToList();
Console.WriteLine($"active={active.Count}");

Analyze("X from Cost", active.Select(n => (A:n.Node.ResearchCost, B:n.Node.ViewPosX, Name:n.ShortName, Idx:n.Index)).ToList());
Analyze("Y from Time", active.Select(n => (A:n.Node.ResearchTime, B:n.Node.ViewPosY, Name:n.ShortName, Idx:n.Index)).ToList());

Console.WriteLine();
Console.WriteLine("Largest |Y-Time| (active):");
foreach (var n in active.OrderByDescending(n => Math.Abs(n.Node.ViewPosY - n.Node.ResearchTime)).Take(20))
{
    Console.WriteLine($"idx={n.Index,3} {n.ShortName,-12} T={n.Node.ResearchTime,6:0.##} Y={n.Node.ViewPosY,6:0.##} d={n.Node.ViewPosY-n.Node.ResearchTime,6:0.##}");
}

Console.WriteLine();
Console.WriteLine("Largest |X-Cost| (active):");
foreach (var n in active.OrderByDescending(n => Math.Abs(n.Node.ViewPosX - n.Node.ResearchCost)).Take(20))
{
    Console.WriteLine($"idx={n.Index,3} {n.ShortName,-12} C={n.Node.ResearchCost,6:0.##} X={n.Node.ViewPosX,6:0.##} d={n.Node.ViewPosX-n.Node.ResearchCost,6:0.##}");
}

static void Analyze(string title, List<(float A, float B, string Name, int Idx)> data)
{
    float meanA = data.Average(x => x.A);
    float meanB = data.Average(x => x.B);
    float cov=0, varA=0;
    foreach (var d in data)
    {
        var da = d.A - meanA;
        var db = d.B - meanB;
        cov += da*db;
        varA += da*da;
    }
    float slope = varA == 0 ? 0 : cov / varA;
    float intercept = meanB - slope*meanA;

    float mae = data.Average(d => Math.Abs((slope*d.A + intercept) - d.B));
    float rmse = MathF.Sqrt(data.Average(d => MathF.Pow((slope*d.A + intercept) - d.B,2)));

    float corr = Corr(data.Select(x=>x.A).ToArray(), data.Select(x=>x.B).ToArray());

    Console.WriteLine($"{title}: corr={corr:0.###}, slope={slope:0.###}, intercept={intercept:0.###}, MAE={mae:0.###}, RMSE={rmse:0.###}");
}

static float Corr(float[] a, float[] b)
{
    if (a.Length != b.Length || a.Length == 0) return 0;
    var ma = a.Average();
    var mb = b.Average();
    float top=0, aa=0, bb=0;
    for(int i=0;i<a.Length;i++)
    {
        var da=a[i]-ma; var db=b[i]-mb;
        top += da*db; aa += da*da; bb += db*db;
    }
    if (aa<=0||bb<=0) return 0;
    return top / MathF.Sqrt(aa*bb);
}
