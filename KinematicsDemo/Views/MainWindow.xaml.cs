using System;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Microsoft.Xaml.Behaviors.Layout;

namespace KinematicsDemo.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public double UpperArmLength { get; } = 112;

    public double ForeArmLength { get; } = 80;

    private Point _shoulderAxis = new Point(213, 196); // Canvas x + w/2,  Canvas y + h/2
    private Point _elbowAxis = new Point(99, 84); // Canvas x + w/2,  Canvas y + h/2
    private double _jointOffset;
    private double _currentShoulderAngle;
    private double _currentElbowAngle;

    public MainWindow()
    {
        InitializeComponent();
        CalculateIntitialSetup();
    }

    // Current Angle
    // Current Distance
    private void CalculateIntitialSetup()
    {
        // Calculate the distance
        _jointOffset = ShoulderJoint.Width / 2;
    }

    private void ElbowJoint_Dragging(object sender, MouseEventArgs e)
    {
        var dragBehavior = (MouseDragElementBehavior)sender;

        //  e.GetPosition
        var angle = CalculateAngle(_shoulderAxis, dragBehavior.X, dragBehavior.Y);
        AngleTextBox.Text = $"Angle: {angle}";
        _currentShoulderAngle = -angle;
        var t = FullArm.RenderTransform as TransformGroup; //..Children[2];
        var r = t?.Children[0] as RotateTransform;
        if(r!=null)
        {
            r.Angle=_currentShoulderAngle;
        }

        XDragTextBox.Text= $"x drag: {dragBehavior.X}";
        YDragTextBox.Text= $"Y drag: {dragBehavior.Y}";
    }

    private void ElbowJoint_DragFinished(object sender, MouseEventArgs e)
    {
        var dragBehavior = (MouseDragElementBehavior)sender;

        // TODO: Place back the cursor to the end of the axis
        var centerX = _shoulderAxis.X; // centre x of circle
        var centerY = _shoulderAxis.Y; // centre y of circle
        var radius = UpperArmLength; // radius
        var angle = _currentShoulderAngle* (Math.PI/180); // degree in angle from top
        double newX = centerX + (radius * Math.Cos(angle));
        double newY = centerY + (radius * Math.Sin(angle));
        Point p = new Point(newX-16, newY-16);

        // Remove the transforms created by the drag behavior
        ElbowJointDragPoint.RenderTransform = null;

        Canvas.SetTop(ElbowJointDragPoint, p.Y);
        Canvas.SetLeft(ElbowJointDragPoint, p.X);
        _elbowAxis = p;
        XDragTextBoxEnd.Text= $"xe drag: {p.X}";
        YDragTextBoxEnd.Text= $"Ye drag: {p.Y}";
    }

    private void WristJoint_Dragging(object sender, MouseEventArgs e)
    {
        var dragBehavior = (MouseDragElementBehavior)sender;

        //  e.GetPosition
        var angle = CalculateAngle(_elbowAxis, dragBehavior.X, dragBehavior.Y);
        AngleTextBox.Text = $"Angle: {angle}";
        _currentElbowAngle = -angle;
        var t = ForeArm.RenderTransform as TransformGroup; //..Children[2];
        var r = t?.Children[0] as RotateTransform;
        if(r!=null)
        {
            r.Angle=_currentElbowAngle;
        }

        XDragTextBox.Text= $"x drag: {dragBehavior.X}";
        YDragTextBox.Text= $"Y drag: {dragBehavior.Y}";
    }

    private void WristJoint_DragFinished(object sender, MouseEventArgs e)
    {
        var dragBehavior = (MouseDragElementBehavior)sender;

        // TODO: Place back the cursor to the end of the axis
        var centerX = _elbowAxis.X; // centre x of circle
        var centerY = _elbowAxis.Y; // centre y of circle
        var radius = ForeArmLength; // radius
        var angle = _currentElbowAngle* (Math.PI/180); // degree in angle from top
        double newX = centerX + (radius * Math.Cos(angle));
        double newY = centerY + (radius * Math.Sin(angle));
        Point p = new Point(newX-16, newY-16);

        // Remove the transforms created by the drag behavior
        WristJointDragPoint.RenderTransform = null;

        Canvas.SetTop(WristJointDragPoint, p.Y);
        Canvas.SetLeft(WristJointDragPoint, p.X);

        //_wri = p;
        XDragTextBoxEnd.Text= $"xe drag: {p.X}";
        YDragTextBoxEnd.Text= $"Ye drag: {p.Y}";
    }

    /// <summary>
    /// Calculate the angle based on one angle (90°) and two sides
    /// </summary>
    /// <param name="dragX">x pos of the new point</param>
    /// <param name="dragY">y  pos of the new pos</param>
    /// <returns></returns>
    private double CalculateAngle(Point jointAxis, double dragX, double dragY)
    {
        // one side is the Arm length the other side is the y difference and the angle is 90°
        double aSide = jointAxis.Y-(dragY+_jointOffset); 
        double bSide = jointAxis.X-(dragX+_jointOffset); 
        
        // Remove possibility to zero divisions
        if (aSide == 0 )
        {
            aSide = 0.0001;
        }

        if (bSide == 0)
        {
            bSide=  0.0001;
        }

        double cSide = Math.Sqrt((aSide*aSide) + (bSide*bSide));
        double aAngle =  Math.Acos( ( (bSide * bSide) + (cSide * cSide) - (aSide * aSide)) / (2 * bSide * cSide)); // Math.Acos( ( dx * dx + c * c - dy * dy) / (2 * dx * c));
        aAngle = aAngle * 180 / Math.PI; // in degrees

        return 360-(aSide>=0?180+aAngle:-aAngle+180);
    }
}