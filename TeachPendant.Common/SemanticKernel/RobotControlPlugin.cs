using Biosero.TeachPendant.Common.Communicators;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Drawing;

namespace Biosero.TeachPendant.Common.SemanticKernel;

/// <summary>
/// Controls the robot with simple instructions
/// </summary>
public class RobotControlPlugin
{
    private KinematicsDemoCommunicator _kinematicDemoCommunicator;

    //public ScaraPosition Position { get; set; }
    public RobotCoordinate RobotPosition { get; set; } = new RobotCoordinate();

    [KernelFunction, Description("Gets the position of the robot arm.")]
    public string GetRobotPosition()
    {
        _kinematicDemoCommunicator ??= new KinematicsDemoCommunicator("http://localhost:7276");
        RobotPosition = _kinematicDemoCommunicator.GetCoordinates();
        SemanticKernelRecognizer.Instance.PluginResponse += $"[Robot is at {RobotPosition} ] {Environment.NewLine}";
        return RobotPosition.ToString();
    }

    [KernelFunction(name: "GoEast"), Description("Moves the robot arm X position to the right or towards the east")]
    public string GoEast([Description("Distance in millimeter to move by")] int xDistanceIncrement = 10)
        => MoveToPoint(MoveDirection.East, xDistanceIncrement);

    [KernelFunction, Description("Moves the robot X position to the left or towards the west")]
    public string GoWest([Description("Distance in millimeter to move by")] int xDistanceIncrement = 10)
        => MoveToPoint(MoveDirection.West, xDistanceIncrement);

    [KernelFunction, Description("Moves the robot arm Y position up or towards the north")]
    public string GoNorth([Description("Distance in millimeter to move by")] int yDistanceIncrement = 10)
        => MoveToPoint(MoveDirection.North, yDistanceIncrement);

    [KernelFunction, Description("Moves the robot Y position down or towards the South")]
    public string GoSouth([Description("Distance in millimeter to move by")] int yDistanceIncrement = 10)
        => MoveToPoint(MoveDirection.South, yDistanceIncrement);

    [KernelFunction, Description("Moves the robot Z position higher or up on the mast")]
    public string GoHigher([Description("Distance in millimeter to move by")] int zDistanceIncrement = 10)
        => MoveToPoint(MoveDirection.Up, zDistanceIncrement);

    [KernelFunction, Description("Moves the robot Z position lower or down on the mast")]
    public string GoLower([Description("Distance in millimeter to move by")] int zDistanceIncrement = 10)
        => MoveToPoint(MoveDirection.Down, zDistanceIncrement);

    [KernelFunction, Description("Stop the robot")]
    public string Stop()
    {
        var response = $"[The robot is now stopped at {RobotPosition}]";
        AddKernelResponseAndWriteToConsole(response);
        return RobotPosition.ToString();
    }

    private string MoveToPoint(MoveDirection direction, int increment)
    {
        RobotPosition = direction switch
        {
            //MoveDirection.West => new RobotCoordinate(RobotPosition.X - increment, RobotPosition.Y, RobotPosition.Z, RobotPosition.Rail),
            //MoveDirection.East => new RobotCoordinate(RobotPosition.X + increment, RobotPosition.Y, RobotPosition.Z, RobotPosition.Rail),
            //MoveDirection.North => new RobotCoordinate(RobotPosition.X, RobotPosition.Y + increment, RobotPosition.Z, RobotPosition.Rail),
            //MoveDirection.South => new RobotCoordinate(RobotPosition.X, RobotPosition.Y - increment, RobotPosition.Z, RobotPosition.Rail),
            //MoveDirection.Up => new RobotCoordinate(RobotPosition.X, RobotPosition.Y, RobotPosition.Z + increment, RobotPosition.Rail),
            //MoveDirection.Down => new RobotCoordinate(RobotPosition.X, RobotPosition.Y, RobotPosition.Z - increment, RobotPosition.Rail),
            MoveDirection.West => new RobotCoordinate( - increment, 0,0,0),
            MoveDirection.East => new RobotCoordinate(increment, 0,0,0),
            MoveDirection.North => new RobotCoordinate(0, increment, 0,0),
            MoveDirection.South => new RobotCoordinate(0, - increment,0, 0),
            MoveDirection.Up => new RobotCoordinate(0,0, increment,0),
            MoveDirection.Down => new RobotCoordinate(0, 0, - increment, 0),
            _ => throw new Exception("Invalid direction"),
        };
        SendMoveRequest();
        var response = $"[Robot Moved {direction} by {increment}mm ]";
        AddKernelResponseAndWriteToConsole(response);
        Thread.Sleep(100);
        return RobotPosition.ToString();
    }

    private static void AddKernelResponseAndWriteToConsole(string message)
    {
        Console.WriteLine(message);
        SemanticKernelRecognizer.Instance.PluginResponse += $"{message} {Environment.NewLine}";
    }

    private void SendMoveRequest()
    {
        _kinematicDemoCommunicator ??= new KinematicsDemoCommunicator("http://localhost:7276");
        _kinematicDemoCommunicator.Move(new RobotCoordinate(RobotPosition.X, RobotPosition.Y, RobotPosition.Z, RobotPosition.Rail));
        Task.Delay(200).Wait();
    }
}
