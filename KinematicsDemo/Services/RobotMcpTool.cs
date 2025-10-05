using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Biosero.Kinematics.Common;
using Biosero.TeachPendant.Common;
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
    [Description("Returns the end effector position")]
    public static RobotCoordinate GetLastCoordinates()
    {
        return GetCoordinates();
    }

    private static RobotCoordinate GetCoordinates()
    {
        if (Robot != null)
        {
            var coordinate = new RobotCoordinate(
                Robot.ArmRailPosition,
                Robot.MousePoint.X,
                Robot.MousePoint.Y,
                Robot.ArmHeightPosition);
            return coordinate;
        }

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
    [Description("Move the robot arm by some unit and returns the new adjusted position coordinates are in Millimeters")]
    public static async Task<RobotCoordinate> MoveByAsync(
        [Description("The amount, in millimeters, to move the robot arm along the Rail. Can be positive (Forward) or negative (Backward / Back)) to indicate direction.")] double railIncrease = 0,
        [Description("The amount, in millimeters, to move the robot arm along the East or West. Can be positive (West / Left) or negative (East / Right) to indicate direction.")] double xIncrease = 0,
        [Description("The amount, in millimeters, to move the robot arm along the North or South Can be positive (North) or negative (South) to indicate direction.")] double yIncrease = 0,
        [Description("The amount, in millimeters, to move the robot arm (Up or Down) along the Mast (or Z Axis). Can be positive (Up) or negative (Down) to indicate direction.")] double zIncrease = 0)
    {
        if (Robot == null)
        {
            return new RobotCoordinate(0, 0, 0, 0);
        }

        RobotCoordinate newCoordinates = new RobotCoordinate(0, 0, 0, 0);;

        await Application.Current.Dispatcher.InvokeAsync(
            () =>
            {
                if (railIncrease > 0)
                {
                    Robot?.GoForwardCommand.Execute(railIncrease);
                }
                else if (railIncrease < 0)
                {
                    Robot?.GoBackwardCommand.Execute(-railIncrease);
                }

                if (zIncrease > 0)
                {
                    Robot?.GoUpCommand.Execute(zIncrease);
                }
                else if (zIncrease < 0)
                {
                    Robot?.GoDownCommand.Execute(-zIncrease);
                }

                // TODO: Needs to be optimized
                var m = Robot.LastSurfacePoint;
                if (xIncrease > 0)
                {
                    m.X += xIncrease;
                }
                else if (xIncrease < 0)
                {
                    m.X += xIncrease;
                }

                if (yIncrease > 0)
                {
                    m.Y += yIncrease;
                }
                else if (yIncrease < 0)
                {
                    m.Y += yIncrease;
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
    [Description("Move the robot arm to a point in space and returns the new position, coordinates are in Millimeters")]
    public static async Task<RobotCoordinate> MoveToAsync(
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
    [Description("Record the current Position")]
    public static List<RobotCoordinate> RecordCurrentPosition()
    {
        Robot.AddPointCommand.Execute(null);
        Coordinates.Add(GetCoordinates());
        return Coordinates;
    }

    [McpServerTool]
    [Description("Play the list of recorded positions")]
    public static void PlayRecordedPoints()
    {
        Robot?.PlayCommand.Execute(null);
    }

    [McpServerTool]
    [Description("Clears the list of recorded positions")]
    public static void ClearRecordedPoints()
    {
        Robot?.RecordedMetaPoints.Points.Clear();
        Coordinates.Clear();
    }

    [McpServerTool]
    [Description("Home the robot to its inital position")]
    public static RobotCoordinate HomeRobotArm()
    {
        Robot?.GoHomeCommand.Execute(null);
        return GetCoordinates();
    }

    private static void UpdateAndRefresh(Point m)
    {
        Robot?.MousePoint = m;
        Robot?.LastSurfacePoint = m;
        Robot?.RefreshDrawing();
    }
}
