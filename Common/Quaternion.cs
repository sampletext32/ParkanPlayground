namespace Common;

public record Quaternion(float X, float Y, float Z, float W);

public record UShortQuaternion(ushort X, ushort Y, ushort Z, ushort W)
{
    public Quaternion ToRegular()
    {
        return new Quaternion(
            X / 32767f,
            Y / 32767f,
            Z / 32767f,
            W / 32767f
        );
    }
};