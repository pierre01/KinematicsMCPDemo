using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using System;
using System.Windows;

namespace KinematicsDemo.Models;

/// <summary>
/// Represents a link in a kinematic multi link system in a 2D system 
/// (e.g. section of a Scara robot arm)
/// PointA is the Origin, PointB is the extremity
/// </summary>
public partial class Segment : ObservableObject
{

    private Point _pointA;

    public double MinAngle { get; }
    public double MaxAngle { get; }

    /// <summary>
    /// used if you want to lock the end of the link to a point
    /// </summary>
    public bool IsPointBLocked { get; set; } = false;

    private static SKPaint _jointLockedPaint = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        Color = SKColor.Parse("dc2626"),
    };

    /// <summary>
    /// Extremity A of the segment
    /// </summary>
    public Point PointA
    {
        get => _pointA;
        set
        {
            if (IsPointBLocked)
            {
                // Calculate the increment Angle
                var newA = value;
                double distAtoNewA = KUtils.GetDistanceBetweenTwoPoints(_pointA, newA);
                double angle = KUtils.CalculateAngle(Length, Length, distAtoNewA);
                double angleIncrement = KUtils.RadianToDegree(angle);
                bool isClockwise = KUtils.IsDirectionBetweenTwoPointsOnCircleClockwise(_pointA, newA, _pointB);
                double sign = isClockwise ? -1 : 1;
                RelativeAngle += sign * angleIncrement;
            }
            _pointA = value;
            CalculateB();
        }
    }

    /// <summary>
    /// Extremity B of the segment
    /// </summary>
    private Point _pointB;

    public Point PointB => _pointB;

    /// <summary>
    /// Gets length of the segment
    /// </summary>
    public double Length { get; }

    /// <summary>
    /// Angle of the segment in radiant
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RelativeAngle))]
    private double _angle;

    /// <summary>
    /// Actual angle in degree for the segment relative to its axis
    /// </summary>
    [ObservableProperty]
    public double _relativeAngle;

    /// <summary>
    /// Create lenghtOfBtoC new segment
    /// </summary>
    /// <param name="x">origin X</param>
    /// <param name="y">origin Y</param>
    /// <param name="length"></param>
    /// <param name="initialAngle">Initial angleIncrement in Radiant</param>
    /// <param name="minAngle">Min angleIncrement of freedom in degree</param>
    /// <param name="maxAngle">Max angleIncrement of freedom in degree</param>
    public Segment(double x, double y, double length, double initialAngle, double minAngle = -93, double maxAngle = 93)
    {
        MaxAngle = maxAngle;
        MinAngle = minAngle;
        _pointA = new Point(x, y);
        Length = length;
        Angle = initialAngle;// KUtils.DegreeToRadian(KUtils.GetClosestAngleBetweenTwoAngles( initialAngle,MinAngle,MaxAngle));
        RelativeAngle = KUtils.RadianToDegree(Angle);
        CalculateB();

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Segment"/> class.
    /// Copy constructor
    /// </summary>
    /// <param name="segment">segment to copy</param>
    public Segment(Segment segment)
    {
        MaxAngle = segment.MaxAngle;
        MinAngle = segment.MinAngle;
        _pointA = segment._pointA;
        _pointB = segment._pointB;
        Length = segment.Length;
        Angle = segment.Angle;
        RelativeAngle = segment.RelativeAngle;
    }

    public Segment(Point pointA, Point pointB, double minAngle = -93, double maxAngle = 93)
    {
        MaxAngle = maxAngle;
        MinAngle = minAngle;
        _pointA = pointA;
        _pointB = pointB;
        var pointC = new Point(pointB.X, pointA.Y);

        double distBtoC = KUtils.GetDistanceBetweenTwoPoints(pointB, pointC);
        double distAtoC = KUtils.GetDistanceBetweenTwoPoints(pointA, pointC);
        double distAtoB = KUtils.GetDistanceBetweenTwoPoints(pointA, pointB);

        double angle = KUtils.CalculateAngle(distAtoC, distAtoB, distBtoC);

        Length = distAtoB;
        Angle = angle;// KUtils.DegreeToRadian(KUtils.GetClosestAngleBetweenTwoAngles( initialAngle,MinAngle,MaxAngle));
        RelativeAngle = KUtils.RadianToDegree(Angle);

    }



    /// <summary>
    /// Initializes a new instance of the <see cref="Segment"/> class.
    /// Create a new segment attached to another segment
    /// </summary>
    /// <param name="parent">Point A of the segment is attached to Parent Point B</param>
    /// <param name="length">length of the segment in mm</param>
    /// <param name="initialAngle">Angle in Degree</param>
    /// <param name="minAngle">Min angle of freedom in degree</param>
    ///  <param name="maxAngle">Max angle of freedom in degree</param>
    public Segment(Segment? parent, double length, double initialAngle, double minAngle = -93, double maxAngle = 93)
    {
        MinAngle = minAngle;
        MaxAngle = maxAngle;
        if (parent != null)
        {
            _pointA = parent._pointB;
        }

        Length = length;
        _angle = KUtils.DegreeToRadian(initialAngle); // KUtils.DegreeToRadian(KUtils.GetClosestAngleBetweenTwoAngles( initialAngle,MinAngle,MaxAngle));
        CalculateB();

    }

    /// <summary>
    /// Follows segment by moving the end of the segment to origin of the child
    /// </summary>
    /// <param name="child">Segment to follow</param>
    public void Follow(Segment child)
    {
        Follow(child.PointA);
    }

    /// <summary>
    /// Follows a point by moving the end of the segment to the target
    /// </summary>
    /// <param name="target"></param>
    public void Follow(Point target)
    {
        //Angle = KUtils.GetClosestAngleBetweenTwoAngles(Angle,MinAngle,MaxAngle);
        Vector directionVector = new Vector(target.X - _pointA.X, target.Y - _pointA.Y);
        var angle = Math.Atan2(directionVector.Y, directionVector.X);
        Angle = KUtils.GetClosestAngleBetweenTwoAngles(angle, MinAngle, MaxAngle);
        // are different, we are locked to one of the max  
        directionVector.Normalize();
        directionVector *= Length;
        directionVector *= -1; // Flip it so it is pointing to the target
        _pointA = new Point(target.X + directionVector.X, target.Y + directionVector.Y);
    }

    /// <summary>
    /// Follows a point by moving the begining of the segment to the target
    /// </summary>
    /// <param name="target"></param>
    public void FollowWithB(Point target)
    {

        Vector directionVector = new Vector(target.X - _pointB.X, target.Y - _pointB.Y);
        var angle = Math.Atan2(directionVector.Y, directionVector.X);
        Angle = KUtils.GetClosestAngleBetweenTwoAngles(angle, MinAngle, MaxAngle);
        // are different, we are locked to one of the max  
        directionVector.Normalize();
        directionVector *= Length;
        directionVector *= -1; // Flip it so it is pointing to the target
        _pointB = new Point(target.X + directionVector.X, target.Y + directionVector.Y);
    }

    /// <summary>
    /// Calculate coordinate of point B Knowing Point A, length and angleIncrement
    /// </summary>
    private void CalculateB()
    {
        if (IsPointBLocked)
        {
            return;
        }
        double dx = Length * Math.Cos(Angle);
        double dy = Length * Math.Sin(Angle);
        _pointB = new Point(_pointA.X + dx, _pointA.Y + dy);
    }

    public void Update()
    {
        CalculateB();
    }

    /// <summary>
    /// Draw the segment
    /// </summary>
    /// <param name="canvas">Skia Canvas</param>
    /// <param name="segmentPaint"></param>
    /// <param name="jointPaint"></param>
    public virtual void Draw(SKCanvas? canvas, SKPaint segmentPaint, SKPaint jointPaint, bool isJointLocked = false, bool isExtremityLocked = false)
    {
        if (canvas == null) return;
        canvas.DrawLine((float)_pointA.X, (float)_pointA.Y, (float)_pointB.X, (float)_pointB.Y, segmentPaint);

        // If the joint is locked, draw a red circle to indicate it is locked
        canvas.DrawCircle((float)_pointA.X, (float)_pointA.Y, 8, isJointLocked ? _jointLockedPaint : jointPaint);

        // this is used to indicate the extremity is locked (only on the last segment)
        canvas.DrawCircle((float)_pointB.X, (float)_pointB.Y, 8, isExtremityLocked ? _jointLockedPaint : jointPaint);
    }

    /// <summary>
    /// Add angleIncrement to the current angleIncrement
    /// </summary>
    /// <param name="angleIncrement">angleIncrement to add in radians</param>
    public void Rotate(double angleIncrement, double relativeAngle = 0)
    {
        var oldAngle = Angle;
        Angle += angleIncrement;
        RelativeAngle += KUtils.RadianToDegree(relativeAngle); // TODO: adjust for angle change
        Update();
    }
}