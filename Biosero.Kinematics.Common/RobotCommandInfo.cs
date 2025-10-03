using Biosero.Kinematics.Common;

namespace Biosero.TeachPendant.Common;

public class RobotCommandInfo
{
    public string Command { get; set; }
    public double StepPrecision { get; set; }
    public RobotCoordinate Coordinate { get; set; }
}
