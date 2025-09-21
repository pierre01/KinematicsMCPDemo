namespace Biosero.TeachPendant.Common;

public class MoveIncrements
{
    public static readonly RobotCoordinate North
        = new() { X = 0, Y = 1, Z = 0, Rail = 0 };

    public static readonly RobotCoordinate South
        = new() { X = 0, Y = -1, Z = 0, Rail = 0 };

    public static readonly RobotCoordinate West
        = new() { X = -1, Y = 0, Z = 0, Rail = 0 };

    public static readonly RobotCoordinate East
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
            MoveDirection.North => North,
            MoveDirection.South => South,
            MoveDirection.West => West,
            MoveDirection.East => East,
            MoveDirection.Up => Up,
            MoveDirection.Down => Down,
            MoveDirection.Forward => Forward,
            MoveDirection.Backward => Backward,
            _ => new RobotCoordinate()
        };
}
