namespace Biosero.TeachPendant.Common;

public struct RobotCoordinate
{
    public double Rail { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public RobotCoordinate(double x, double y, double z, double rail)
    {
        X = x;
        Y = y;
        Z = z;
        Rail = rail;
    }

    public override string ToString()
    {
        return $"X: {X}, Y: {Y}, Z: {Z}, Rail: {Rail}";
    }

    public static RobotCoordinate operator +(RobotCoordinate a, RobotCoordinate b)
        => new()
        {
            X = a.X + b.X,
            Y = a.Y + b.Y,
            Z = a.Z + b.Z,
            Rail = a.Rail + b.Rail
        };

    public static RobotCoordinate operator *(RobotCoordinate coordinate, double scale)
        => new()
        {
            X = coordinate.X * scale,
            Y = coordinate.Y * scale,
            Z = coordinate.Z * scale,
            Rail = coordinate.Rail * scale
        };
}
