using System.ComponentModel;

namespace Biosero.Kinematics.Common;

public class RobotCoordinate(double x, double y, double z, double rail)
{
    [Description("Position on the rail in millimeters")]
    public double Rail { get; set; } = rail;

    [Description("X Position (east or west)  in millimeters")]
    public double X { get; set; } =x;

    [Description("Y Position (north and south)  in millimeters")]
    public double Y { get; set; }= y;

    [Description("Height Position on the mast in millimeters")]
    public double Z { get; set; }= z;


    public RobotCoordinate():this(0,0,0,0)
    {

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
