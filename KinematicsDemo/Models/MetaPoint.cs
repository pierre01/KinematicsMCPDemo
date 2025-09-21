using System;
using System.Windows;

namespace KinematicsDemo.Models;

/// <summary>
/// The position of the robot Arm in 3D space not only including the X and Y position of the end effector
/// but also the Z position, the rail position, and the angles of each joints
/// including the states for each joints  (locked,not locked) and the speed of the movement
/// </summary>
[Serializable]
public class MetaPoint
{
    private Point _mouseRecordedPoint;
    private double _speed; //  Not used yet
    private JointsLocks _jointsLocks; //0  no locks 1 shoulder lock 2 elbow lock  3 shoulder and elbow 4
    private double _shoulderAngle;
    private double _elbowAngle;
    private double _wristAngle;
    private double _zPosition;
    private double _railPosition;
    private Point _effectorGripPoint;

    public MetaPoint()
    {

    }

    // TODO: add angles, Z, and Rail positions
    // TODO: Add possibility to play by angle or play by inverse Kinematics
    /// <summary>
    /// Initializes a new instance of the <see cref="MetaPoint"/> class.
    /// 
    /// </summary>
    /// <param name="point">The point we are trying to follow (usually the mouse)</param>
    /// <param name="speed"></param>
    /// <param name="jointsLocks">the state of the joints locking</param>
    public MetaPoint(Point point, double speed, JointsLocks jointsLocks)//,double shoulderAngle, double elbowAngle, double wristAngle, double zPosition,double railPosition )
    {
        _mouseRecordedPoint = point;
        _speed = speed;
        _jointsLocks = jointsLocks;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetaPoint"/> class.
    /// </summary>
    /// <param name="mouseRecordedPoint">The point we are trying to follow (usually the mouse)</param>
    /// <param name="speed">speed</param>
    /// <param name="jointsLocks">the state of the joints locking</param>
    /// <param name="shoulderAngle">Angle of the upper arm</param>
    /// <param name="elbowAngle">angle of the forearm</param>
    /// <param name="wristAngle">angle of the wrist</param>
    /// <param name="zPosition">vertical position</param>
    /// <param name="railPosition">pos of the rail</param>
    /// <param name="effectorGripPoint">the end of the effector </param>
    public MetaPoint(Point mouseRecordedPoint, double speed, JointsLocks jointsLocks, double shoulderAngle, double elbowAngle, double wristAngle, double zPosition, double railPosition, Point effectorGripPoint)
    {
        _mouseRecordedPoint = mouseRecordedPoint;
        _speed = speed;
        _jointsLocks = jointsLocks;
        _shoulderAngle = shoulderAngle;
        _elbowAngle = elbowAngle;
        _wristAngle = wristAngle;
        _zPosition = zPosition;
        _railPosition = railPosition;
        _effectorGripPoint = effectorGripPoint;

    }

    public double ShoulderAngle
    {
        get => _shoulderAngle;
        set => _shoulderAngle = value;
    }

    public double ElbowAngle
    {
        get => _elbowAngle;
        set => _elbowAngle = value;
    }

    public double WristAngle
    {
        get => _wristAngle;
        set => _wristAngle = value;
    }

    public double ZPosition
    {

        get => _zPosition;
        set => _zPosition = value;
    }

    public double RailPosition
    {
        get => _railPosition;
        set => _railPosition = value;
    }

    public Point EffectorGripPoint
    {
        get => _effectorGripPoint;
        set => _effectorGripPoint = value;
    }


    public Point MousePoint
    {
        get => _mouseRecordedPoint;
        set => _mouseRecordedPoint = value;
    }

    public double Speed
    {
        get => _speed;
        set => _speed = value;
    }

    public JointsLocks JointsLocks
    {
        get => _jointsLocks;
        set => _jointsLocks = value;
    }
}
