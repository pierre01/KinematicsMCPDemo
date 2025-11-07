using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Biosero.Kinematics.Common;
using KinematicsDemo.ViewModels;
using ModelContextProtocol.Server;

namespace KinematicsDemo.Services;

[McpServerToolType]
public static class RobotMcpTool
{
    public static RobotArmViewModel Robot;
    public static List<RobotCoordinate> Coordinates = new List<RobotCoordinate>();

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
    /// <param name="armExtendBy">The amount, in millimeters, to move the robot arm along the X axis. Can be positive or negative to indicate direction.</param>
    /// <param name="armLeftRightBy">The amount, in millimeters, to move the robot arm along the Y axis. Can be positive or negative to indicate direction.</param>
    /// <param name="armUpDownBy">The amount, in millimeters, to move the robot arm along the Z axis. Can be positive or negative to indicate direction.</param>
    /// <returns>A RobotCoordinate representing the new position of the robot arm after the movement.</returns>
    [McpServerTool]
    [Description("Moves the robot arm by a specified distance (in millimeters) along each axis. Positive and negative values indicate direction.The robot moves relative to its current position. (values are not cummulative acreoss call)")]
    public static async Task<RobotCoordinate> MoveBy(
            [Description("Distance to move along the rail axis (usually the base linear track). Positive values move the robot forward on the rail (away from the home position). Negative values move it backward on the rail (toward the home position). instructions should include the word rail (e.g. rail forward, rail backward)")]
            double railChangeBy = 0,
            [Description("Distance to move along the X-axis in the robot's local coordinate system. Positive values move the arm to the reach / reach forward / extend (right when facing the robot from the front). Negative values retract the arm (left).")]
            double armExtendBy = 0,
            [Description("Distance to move along the Y-axis in the robot's local coordinate system. Positive values move the arm to the Left (away from the operator). Negative values move it to the right (toward the operator).")]
            double armLeftRightBy = 0,
            [Description("Distance to move along the Z-axis (vertical mast). Positive values move the end effector up / upward. Negative values move it down / downward.")]
            double armUpDownBy = 0)
    {

        if (Robot == null)
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
                    Robot?.GoForwardCommand.Execute(railChangeBy);
                }
                else if (railChangeBy < 0)
                {
                    Robot?.GoBackwardCommand.Execute(-railChangeBy);
                }

                if (armUpDownBy > 0)
                {
                    Robot?.GoUpCommand.Execute(armUpDownBy);
                }
                else if (armUpDownBy < 0)
                {
                    Robot?.GoDownCommand.Execute(-armUpDownBy);
                }

                // TODO: Needs to be optimized
                var m = Robot.LastSurfacePoint;
                if (armExtendBy > 0)
                {
                    m.X += armExtendBy;
                }
                else if (armExtendBy < 0)
                {
                    m.X += armExtendBy;
                }

                if (armLeftRightBy > 0)
                {
                    m.Y -= armLeftRightBy;
                }
                else if (armLeftRightBy < 0)
                {
                    m.Y -= armLeftRightBy;
                }

                UpdateAndRefresh(m);
                newCoordinates = GetCoordinates();
            },
            DispatcherPriority.Send);

        return newCoordinates;
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
    [Description("Move the robot arm to a point in space and returns the new position (Furthest possible Reach sor the robot) coordinates are in Millimeters")]
    public static async Task<RobotCoordinate> MoveTo(
        [Description("Position on the rail in millimeters")] double railPosition,
        [Description("X Position (east or west)  in millimeters")] double xPosition,
        [Description("Y Position (north and south)  in millimeters")] double yPosition,
        [Description("Height Position on the mast in millimeters")] double zPosition)
    {
        // Get the current Z position and move down if needed based on the zPosition
        if (Robot == null)
        {
            return new RobotCoordinate(0, 0, 0, 0);
        }

        RobotCoordinate newCoordinates = new RobotCoordinate(0, 0, 0, 0); ;

        // Sync to the UI Thread
        await Application.Current.Dispatcher.InvokeAsync(
            () =>
            {
                var currentRail = Robot.ArmRailPosition;
                var stepPrecision = currentRail - railPosition; // in millimeters
                if (stepPrecision > 0)
                {
                    Robot?.GoForwardCommand.Execute(stepPrecision);
                    currentRail += stepPrecision;
                }
                else if (stepPrecision < 0)
                {
                    Robot?.GoBackwardCommand.Execute(-stepPrecision);
                    currentRail += stepPrecision;
                }

                var currentZ = Robot.ArmHeightPosition;
                stepPrecision = currentZ - zPosition; // in millimeters
                if (stepPrecision > 0)
                {
                    Robot?.GoUpCommand.Execute(stepPrecision);
                    currentZ += stepPrecision;
                }
                else if (stepPrecision < 0)
                {
                    Robot?.GoDownCommand.Execute(-stepPrecision);
                    currentZ -= stepPrecision;
                }

                // TODO: Needs to be optimized
                var currentPoint = Robot.MousePoint;
                var xStepPrecision = currentPoint.X - xPosition;
                if (xStepPrecision > 0)
                {
                    var m = Robot.LastSurfacePoint;
                    m.X -= xStepPrecision;
                    currentPoint.X = m.X;
                }
                else if (xStepPrecision < 0)
                {
                    var m = Robot.LastSurfacePoint;
                    m.X += xStepPrecision;
                    currentPoint.X = m.X;
                }

                var yStepPrecision = currentPoint.Y - yPosition;
                if (yStepPrecision > 0)
                {
                    var m = Robot.LastSurfacePoint;
                    m.Y -= yStepPrecision;
                    currentPoint.Y = m.Y;
                }
                else if (yStepPrecision < 0)
                {
                    var m = Robot.LastSurfacePoint;
                    m.Y += yStepPrecision;
                    currentPoint.Y = m.Y;
                }

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
            Robot.AddPointCommand.Execute(null);
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
                Robot.MousePoint.Y,
                Robot.ArmHeightPosition,
                Robot.ArmRailPosition);

            return coordinate;
        }

        return new RobotCoordinate(0, 0, 0, 0);
    }

    /// <summary>
    /// Updates the robot's mouse and surface positions to the specified point and refreshes its drawing state.
    /// === ONLY CALL FROM UI THREAD ===
    /// </summary>
    /// <param name="m">The point to set as the robot's current mouse and surface position.</param>
    private static async Task UpdateAndRefresh(Point m)
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
            Robot.RefreshDrawing();
        }
        else
        {
           await Application.Current.Dispatcher.InvokeAsync(
           () =>
            {
                Robot.MousePoint = m;
                Robot.LastSurfacePoint = m;
                Robot.RefreshDrawing();
            },
           DispatcherPriority.Send);
        }
    }
}
