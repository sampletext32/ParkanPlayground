using Common;

namespace ParkanPlayground;

public record BasContours(InnerContour[] Inner, OuterContour[] Outer);
public record InnerContour(Vector3[] Points, uint[] Nums1, uint[] Nums2);
public record OuterContour(Vector3[] Points);

public static class BasParser
{
    public static BasContours ReadFile(string path)
    {
        using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return ReadFile(fs);
    }

    public static BasContours ReadFile(Stream fs)
    {
        var innerContourCount = fs.ReadUInt32LittleEndian();

        InnerContour[] innerContours = new InnerContour[innerContourCount];

        for (var i = 0; i < innerContourCount; i++)
        {
            var pointCount = fs.ReadUInt32LittleEndian();

            Vector3[] points = new Vector3[pointCount + 1];
    
            for (var j = 0; j < pointCount + 1; j++)
            {
                var x = fs.ReadFloatLittleEndian();
                var y = fs.ReadFloatLittleEndian();
                var z = fs.ReadFloatLittleEndian();

                points[j] = new Vector3(x, y, z);
            }

            uint[] nums1 = new uint[pointCount];

            // batch 1
            for (var j = 0; j < pointCount; j++)
            {
                var num = fs.ReadUInt32LittleEndian();
                nums1[j] = num;
            }

            uint[] nums2 = new uint[pointCount];

            // batch 2
            for (var j = 0; j < pointCount; j++)
            {
                var num = fs.ReadUInt32LittleEndian();
                nums2[j] = num;
            }
    
            innerContours[i] = new InnerContour(points, nums1, nums2);
        }


        var outerContourCount = fs.ReadUInt32LittleEndian();

        OuterContour[] outerContours = new OuterContour[outerContourCount];

        for (var i = 0; i < outerContourCount; i++)
        {
            var pointCount = fs.ReadUInt32LittleEndian();

            Vector3[] points = new Vector3[pointCount + 1];
            for (var j = 0; j < pointCount + 1; j++)
            {
                var x = fs.ReadFloatLittleEndian();
                var y = fs.ReadFloatLittleEndian();
                var z = fs.ReadFloatLittleEndian();
        
                points[j] = new Vector3(x, y, z);
            }

            outerContours[i] = new OuterContour(points);
        }

        BasContours contours = new BasContours(innerContours, outerContours);

        return contours;
    }
}
