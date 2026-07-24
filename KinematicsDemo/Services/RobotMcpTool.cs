using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BioRobot.Kinematics.Common;
using KinematicsDemo.ViewModels;
using ModelContextProtocol.Server;

namespace KinematicsDemo.Services;

[McpServerToolType]
public static class RobotMcpTool
{
    public static RobotArmViewModel? Robot { get; set; }

    public static List<RobotCoordinate> Coordinates { get; } = new();

    /// <summary>
    /// Retrieves the current position of the robot's end effector.
    /// </summary>
    /// <returns>A <see cref="RobotCoordinate"/> representing the end effector's position in the robot's coordinate system.</returns>
    [McpServerTool]
    [Description("Returns the position of the Robot")]
    public static RobotCoordinate GetLastCoordinates()
    {
        return GetCoordinates();
    }

    /// <summary>
    /// Moves the robot arm by the specified increments along each axis and returns the new position.
    /// </summary>
    /// <param name="railChangeBy">The amount, in millimeters, to move the robot arm along the rail axis. Can be positive or negative to indicate
    /// direction.</param>
    /// <param name="armExtendOrRetractBy">The amount, in millimeters, to move the robot arm along the X axis. Can be positive or negative to indicate direction.</param>
    /// <param name="armLeftOrRightBy">The amount, in millimeters, to move the robot arm along the Y axis. Can be positive or negative to indicate direction.</param>
    /// <param name="armUpOrDownBy">The amount, in millimeters, to move the robot arm along the Z axis. Can be positive or negative to indicate direction.</param>
    /// <returns>A RobotCoordinate representing the new position of the robot arm after the movement.</returns>
    [McpServerTool]
    [Description("Moves the robot by relative distances in millimeters. Use MoveLeft or MoveRight for lateral motion. Positive values mean rail forward, arm extend, right, and up. Negative values mean rail backward, arm retract, left, and down. Each value is a delta from the current position; the returned coordinates are absolute.")]
    public static async Task<RobotCoordinate> MoveBy(
            [Description("Relative rail distance in millimeters. Positive moves forward and increases the rail coordinate; negative moves backward and decreases it.")]
            double railChangeBy = 0,
            [Description("Relative X distance in millimeters. Positive extends the arm and increases X; negative retracts the arm and decreases X.")]
            double armExtendOrRetractBy = 0,
            [Description("Legacy relative rendering-Y distance in millimeters. Negative moves left; positive moves right. Prefer MoveLeft or MoveRight.")]
            double armLeftOrRightBy = 0,
            [Description("Relative Z distance in millimeters. Positive moves up and increases Z; negative moves down and decreases Z.")]
            double armUpOrDownBy = 0)
    {
        if (Robot is not { } robot)
        {
            return new RobotCoordinate(0, 0, 0, 0);
        }

        RobotCoordinate newCoordinates = new RobotCoordinate(0, 0, 0, 0); ;

        // Sync to UI thread
        await Application.Current.Dispatcher.InvokeAsync(
            () =>
            {
                if (railChangeBy > 0)
                {
                    robot.GoForwardCommand.Execute(railChangeBy);
                }
                else if (railChangeBy < 0)
                {
                    robot.GoBackwardCommand.Execute(-railChangeBy);
                }

                if (armUpOrDownBy > 0)
                {
                    robot.GoUpCommand.Execute(armUpOrDownBy);
                }
                else if (armUpOrDownBy < 0)
                {
                    robot.GoDownCommand.Execute(-armUpOrDownBy);
                }

                // TODO: Needs to be optimized
                var m = robot.LastSurfacePoint;
                m.X += armExtendOrRetractBy;
                m.Y += armLeftOrRightBy;

                UpdateAndRefresh(m);
                newCoordinates = GetCoordinates();
            },
            DispatcherPriority.Send);

        return newCoordinates;
    }

    /// <summary>
    /// Moves the end effector to the robot's left.
    /// </summary>
    /// <param name="distanceMillimeters">Positive distance to move left.</param>
    /// <returns>The reached absolute robot coordinates.</returns>
    [McpServerTool]
    [Description("Moves the robot arm left by a positive distance in millimeters. Use this tool for every relative left movement.")]
    public static Task<RobotCoordinate> MoveLeft(
        [Description("Positive distance to move left in millimeters.")] double distanceMillimeters)
    {
        return MoveLaterally(-System.Math.Abs(distanceMillimeters));
    }

    /// <summary>
    /// Moves the end effector to the robot's right.
    /// </summary>
    /// <param name="distanceMillimeters">Positive distance to move right.</param>
    /// <returns>The reached absolute robot coordinates.</returns>
    [McpServerTool]
    [Description("Moves the robot arm right by a positive distance in millimeters. Use this tool for every relative right movement.")]
    public static Task<RobotCoordinate> MoveRight(
        [Description("Positive distance to move right in millimeters.")] double distanceMillimeters)
    {
        return MoveLaterally(System.Math.Abs(distanceMillimeters));
    }

    /// <summary>
    /// Move the robot arm to a point in space and returns the new position
    /// The new position will be the furthest towards the requested position 
    /// that is reachable by the robot effector
    /// </summary>
    /// <param name="railPosition">new position in rail</param>
    /// <param name="xPosition">New x position</param>
    /// <param name="yPosition">New y position</param>
    /// <param name="zPosition">New z position</param>
    /// <returns>new robot coordinates</returns>
    [McpServerTool]
    [Description("Moves the robot to absolute coordinates in millimeters and returns the reached absolute position. Increasing rail is forward, increasing X is extend, increasing Y is left, and increasing Z is up.")]
    public static async Task<RobotCoordinate> MoveTo(
        [Description("Absolute rail coordinate in millimeters; larger values are farther forward and smaller values are farther backward.")] double railPosition,
        [Description("Absolute X coordinate in millimeters; larger values are more extended and smaller values are more retracted.")] double xPosition,
        [Description("Absolute Y coordinate in millimeters; larger values are farther left and smaller values are farther right.")] double yPosition,
        [Description("Absolute Z coordinate in millimeters; larger values are higher and smaller values are lower.")] double zPosition)
    {
        // Get the current Z position and move down if needed based on the zPosition
        if (Robot is not { } robot)
        {
            return new RobotCoordinate(0, 0, 0, 0);
        }

        RobotCoordinate newCoordinates = new RobotCoordinate(0, 0, 0, 0); ;

        // Sync to the UI Thread
        await Application.Current.Dispatcher.InvokeAsync(
            () =>
            {
                var currentRail = robot.ArmRailPosition;
                var stepPrecision = railPosition - currentRail;
                if (stepPrecision > 0)
                {
                    robot.GoForwardCommand.Execute(stepPrecision);
                }
                else if (stepPrecision < 0)
                {
                    robot.GoBackwardCommand.Execute(-stepPrecision);
                }

                var currentZ = robot.ArmHeightPosition;
                stepPrecision = zPosition - currentZ;
                if (stepPrecision > 0)
                {
                    robot.GoUpCommand.Execute(stepPrecision);
                }
                else if (stepPrecision < 0)
                {
                    robot.GoDownCommand.Execute(-stepPrecision);
                }

                var currentPoint = robot.MousePoint;
                currentPoint.X = xPosition;
                currentPoint.Y = -yPosition;

                UpdateAndRefresh(currentPoint);
                newCoordinates = GetCoordinates();
            },
            DispatcherPriority.Send);

        return newCoordinates;
    }

    [McpServerTool]
    [Description("Record the current Position and returns the list with the added coordinates")]
    public static async Task<List<RobotCoordinate>> RecordCurrentPosition()
    {
        await Application.Current.Dispatcher.InvokeAsync(
        () =>
        {
            Robot?.AddPointCommand.Execute(null);
            Coordinates.Add(GetCoordinates());
        },
        DispatcherPriority.Send);
        return Coordinates;
    }

    [McpServerTool]
    [Description("Play the list of recorded positions")]
    public static async Task PlayRecordedPoints()
    {
        await Application.Current.Dispatcher.InvokeAsync(
        () =>
        {
            Robot?.PlayCommand.Execute(null);
        },
        DispatcherPriority.Send);
    }

    [McpServerTool]
    [Description("Stop playing the recorded positions")]
    public static async Task StopPlaying()
    {
        await Application.Current.Dispatcher.InvokeAsync(
        () =>
        {
            Robot?.StopPlayCommand.Execute(null);
        },
        DispatcherPriority.Send);
    }

    [McpServerTool]
    [Description("Clears the list of recorded positions")]
    public static async Task ClearRecordedPoints()
    {
        await Application.Current.Dispatcher.InvokeAsync(
        () =>
        {
            Robot?.RecordedMetaPoints.Points.Clear();
        },
        DispatcherPriority.Send);

        Coordinates.Clear();
    }

    [McpServerTool]
    [Description("Home the robot to its inital position")]
    public static async Task<RobotCoordinate> HomeRobotArm()
    {
        RobotCoordinate newCoordinates  = new RobotCoordinate(0, 0, 0, 0);
        await Application.Current.Dispatcher.InvokeAsync(
            () =>
            {
                Robot?.GoHomeCommand.Execute(null);
        
                newCoordinates = GetCoordinates();
            },
            DispatcherPriority.Send);

        return newCoordinates;
    }

    /// <summary>
    /// Retrieves the current coordinates of the robot, including mouse position and arm positions.
    /// </summary>
    /// <returns>A <see cref="RobotCoordinate"/> representing the robot's current mouse X and Y positions, arm height, and arm
    /// rail position. If the robot is not available, returns a coordinate with all values set to zero.</returns>
    private static RobotCoordinate GetCoordinates()
    {
        if (Robot != null)
        {
            var coordinate = new RobotCoordinate(
                Robot.MousePoint.X,
                -Robot.MousePoint.Y,
                Robot.ArmHeightPosition,
                Robot.ArmRailPosition);

            return coordinate;
        }

        return new RobotCoordinate(0, 0, 0, 0);
    }

    /// <summary>
    /// Moves laterally using the rendering coordinate system, where negative Y
    /// is left and positive Y is right.
    /// </summary>
    /// <param name="renderingYChange">Signed rendering Y delta.</param>
    /// <returns>The reached absolute robot coordinates.</returns>
    private static async Task<RobotCoordinate> MoveLaterally(double renderingYChange)
    {
        if (Robot is not { } robot)
        {
            return new RobotCoordinate(0, 0, 0, 0);
        }

        RobotCoordinate newCoordinates = new RobotCoordinate(0, 0, 0, 0);
        await Application.Current.Dispatcher.InvokeAsync(
            () =>
            {
                var target = robot.LastSurfacePoint;
                target.Y += renderingYChange;
                UpdateAndRefresh(target);
                newCoordinates = GetCoordinates();
            },
            DispatcherPriority.Send);

        return newCoordinates;
    }

    /// <summary>
    /// Updates the robot's mouse and surface positions to the specified point and refreshes its drawing state.
    /// === ONLY CALL FROM UI THREAD ===
    /// </summary>
    /// <param name="m">The point to set as the robot's current mouse and surface position.</param>
    private static void UpdateAndRefresh(Point m)
    {
        if( Robot == null)
        {
            return;
        }

        // Test if we are inside the UI thread 
        if (Application.Current.Dispatcher.CheckAccess())
        {
            // We are on the UI thread
            Robot.MousePoint = m;
            Robot.LastSurfacePoint = m;
            Robot.IsMousePointInRobotCoordinates = true;
            Robot.RefreshDrawing();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(
            () =>
            {
                if (Robot is { } robot)
                {
                    robot.MousePoint = m;
                    robot.LastSurfacePoint = m;
                    robot.IsMousePointInRobotCoordinates = true;
                    robot.RefreshDrawing();
                }
            },
            DispatcherPriority.Send);
        }
    }
}
