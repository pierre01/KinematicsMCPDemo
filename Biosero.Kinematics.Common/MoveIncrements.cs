namespace Biosero.Kinematics.Common;

public class MoveIncrements
{
    public static readonly RobotCoordinate Left
        = new() { X = 0, Y = 1, Z = 0, Rail = 0 };

    public static readonly RobotCoordinate Right
        = new() { X = 0, Y = -1, Z = 0, Rail = 0 };

    public static readonly RobotCoordinate Retract
        = new() { X = -1, Y = 0, Z = 0, Rail = 0 };

    public static readonly RobotCoordinate Extend
        = new() { X = 1, Y = 0, Z = 0, Rail = 0 };

    public static readonly RobotCoordinate Up
        = new() { X = 0, Y = 0, Z = 1, Rail = 0 };

    public static readonly RobotCoordinate Down
        = new() { X = 0, Y = 0, Z = -1, Rail = 0 };

    public static readonly RobotCoordinate Forward
        = new() { X = 0, Y = 0, Z = 0, Rail = 1 };

    public static readonly RobotCoordinate Backward
        = new() { X = 0, Y = 0, Z = 0, Rail = -1 };

    public static RobotCoordinate GetIncrement(MoveDirection direction)
        => direction switch
        {
            MoveDirection.Left => Left,
            MoveDirection.Right => Right,
            MoveDirection.Retract => Retract,
            MoveDirection.Extend => Extend,
            MoveDirection.Up => Up,
            MoveDirection.Down => Down,
            MoveDirection.Forward => Forward,
            MoveDirection.Backward => Backward,
            _ => new RobotCoordinate()
        };
}
