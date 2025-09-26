using System.Collections.Generic;
using System.ComponentModel;
using Biosero.TeachPendant.Common;
using ModelContextProtocol.Server;

namespace KinematicsDemo.Services;

[McpServerToolType]
public static class RobotMcpTool
{
    /// <summary>
    /// Retrieves the current position of the robot's end effector.
    /// </summary>
    /// <returns>A <see cref="RobotCoordinate"/> representing the end effector's position in the robot's coordinate system.</returns>
    [McpServerTool]
    [Description("Returns the end effector position")]
    public static RobotCoordinate GetCoordinate()
    {
        return new RobotCoordinate(0, 0, 0, 0);
    }

    /// <summary>
    /// Moves the robot arm by the specified increments along each axis and returns the new position.
    /// </summary>
    /// <param name="railIncrease">The amount, in millimeters, to move the robot arm along the rail axis. Can be positive or negative to indicate
    /// direction.</param>
    /// <param name="xIncrease">The amount, in millimeters, to move the robot arm along the X axis. Can be positive or negative to indicate direction.</param>
    /// <param name="yIncrease">The amount, in millimeters, to move the robot arm along the Y axis. Can be positive or negative to indicate direction.</param>
    /// <param name="zIncrease">The amount, in millimeters, to move the robot arm along the Z axis. Can be positive or negative to indicate direction.</param>
    /// <returns>A RobotCoordinate representing the new position of the robot arm after the movement.</returns>
    [McpServerTool]
    [Description("Move the robot arm by some unit and returns the new position coordinates are in Millimeters")]
    public static RobotCoordinate MoveBy(double railIncrease = 0, double xIncrease = 0, double yIncrease = 0, double zIncrease = 0)
    {
        return new RobotCoordinate(0, 0, 0, 0);
    }

    /// <summary>
    /// Move the robot arm to a point in space and returns the new position
    /// </summary>
    /// <param name="railPosition">new position in rail</param>
    /// <param name="xPosition">New x position</param>
    /// <param name="yPosition">New y position</param>
    /// <param name="zPosition">New z position</param>
    /// <returns>new robot coordinates</returns>
    [McpServerTool]
    [Description("Move the robot arm to a point in space and returns the new position, coordinates are in Millimeters")]
    public static RobotCoordinate MoveTo(
        [Description("Position on the rail in millimeters")] double railPosition,
        [Description("X Position (east or west)  in millimeters")] double xPosition,
        [Description("Y Position (north and south)  in millimeters")] double yPosition,
        [Description("Height Position on the mast in millimeters")] double zPosition )
    {
        return new RobotCoordinate(0, 0, 0, 0);
    }

    [McpServerTool]
    [Description("Record the current Position")]
    public static List<RobotCoordinate> RecordCurrentPosition()
    {
        return new List<RobotCoordinate>();
    }

    [McpServerTool]
    [Description("Play the list of recorded positions")]
    public static void PlayRecordedPoints()
    {
        
    }

    [McpServerTool]
    [Description("Clears the list of recorded positions")]
    public static void ClearRecordedPoints()
    {

    }

    [McpServerTool]
    [Description("Home the robot to its inital position")]
    public static RobotCoordinate HomeRobotArm()
    {
        return new RobotCoordinate(0, 0, 0, 0);
    }
}
