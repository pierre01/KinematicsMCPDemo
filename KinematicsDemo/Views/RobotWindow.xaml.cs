using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KinematicsDemo.Models;
using KinematicsDemo.Styles;
using KinematicsDemo.ViewModels;
using SkiaSharp;

namespace KinematicsDemo.Views;

/// <summary>
/// Interaction logic for RobotWindow.xaml
/// </summary>
public partial class RobotWindow : Window
{
    private RobotArmViewModel _robotViewModel;
    private Point _mousePointOld = RobotArmViewModel.DefaultRandomPoint;
    private MetaPoint? _activePoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="RobotWindow"/> class.
    /// </summary>
    /// <param name="robotArmViewModel">the view model</param>
    public RobotWindow(RobotArmViewModel robotArmViewModel)
    {
        _robotViewModel = robotArmViewModel;
        _robotViewModel.Refresh += ArmView_Refresh;
        DataContext = _robotViewModel;
        InitializeComponent();
    }

    /// <summary>
    /// Called by the view model to notify that we need to refresh the view
    /// </summary>
    private void ArmView_Refresh(object? sender, RefreshDrawingEventArgs args)
    {
        if (args.Point != null)
        {
            _activePoint = args.Point;
        }

        SKSurface.InvalidateVisual();
    }

    private void SKSurface_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _robotViewModel.MousePoint = e.GetPosition(SKSurface);
        _robotViewModel.LastSurfacePoint = e.GetPosition(SKSurface);
        SKSurface.InvalidateVisual();
    }

    private void DrawSegments(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
    {
        var precision =_robotViewModel.Precision; // in mm

        if (_robotViewModel == null)
        {
            return;
        }

        SKImageInfo info = e.Info;
        SKSurface surface = e.Surface;
        SKCanvas canvas = surface.Canvas;
        Rect rc = new Rect(0, 0, info.Width, info.Height);

        float xRatio = 4f;
        float yRatio = 2f;
        canvas.Clear();
        canvas.Translate((float)info.Width / xRatio, info.Height / yRatio);
        TranslateTextBlock.Text = $"trans: {(float)info.Width / xRatio:F}, {info.Height / yRatio:F} - {rc}";

        // if we play, draw the active point in the path instead of following the mouse
        if (_robotViewModel.IsPlaying)
        {
            DrawActivePoint(canvas);
            DrawGraphicDetails(canvas, rc);
            return;
        }

        // We clicked on Add Point button to record the end of the effector
        if (_robotViewModel.IsPointManuallyAdded == true)
        {
            //At this point ViewModel.MousePoint is the effector end - now just scale it 
            _robotViewModel.MousePoint = new Point(_robotViewModel.MousePoint.X + (info.Width / xRatio), _robotViewModel.MousePoint.Y + (info.Height / yRatio));
            _robotViewModel.IsPointManuallyAdded = false;
        }

        // follow the mouse if it changed position
        if (_mousePointOld != _robotViewModel.MousePoint)
        {
            if (!_robotViewModel.IsPlaying)
            {
                _robotViewModel.MousePoint = new Point(_robotViewModel.MousePoint.X - (info.Width / xRatio), _robotViewModel.MousePoint.Y - (info.Height / yRatio));
                _mousePointOld = _robotViewModel.MousePoint;
            }

            _robotViewModel.RunInverseKinematics(precision, canvas);

            // TODO: If on rail see if we can get closer to the mouse
            // var xDistanceToMouse = _mousePoint.X - _effectorSegment.PointB.X ;
            if (_robotViewModel.IsRecording && !_robotViewModel.IsPlaying)
            {
                //RecordMetaPoint(new Point(_mousePoint.X, _mousePoint.Y));
                // angleLock combines _isElbowLocked and _isWristLocked and _isShoulderLocked
                var angleLock = (JointsLocks)(_robotViewModel.IsShoulderLocked ? 1 : 0) + (_robotViewModel.IsElbowLocked ? 2 : 0) + (_robotViewModel.IsWristLocked ? 4 : 0);

                //_recordedMetaPoints.Add(new MetaPoint( new Point(_mousePoint.X, _mousePoint.Y),1,angleLock));
                _robotViewModel.RecordPoint(new MetaPoint(
                    new Point(_robotViewModel.MousePoint.X, _robotViewModel.MousePoint.Y), 
                    1, // speed (not used)
                    angleLock, 
                    _robotViewModel.UpperArmSegment.Angle, 
                    _robotViewModel.ForearmSegment.Angle, 
                    _robotViewModel.EffectorSegment.Angle, 
                    _robotViewModel.ArmHeightPosition, 
                    _robotViewModel.ArmRailPosition, 
                    _robotViewModel.EffectorSegment.PointB));
            }
        }

        // Draws a red circle at the mouse position
        canvas.DrawCircle((float)_robotViewModel.MousePoint.X, (float)_robotViewModel.MousePoint.Y, 6, SkiaColors.MousePaint);

        DrawGraphicDetails(canvas, rc);
    }

    /// <summary>
    /// Draw the active point in the recorded list of arm positions 
    /// This is used when playing the recorded arm movement (joint angles)  
    /// </summary>
    /// <param name="canvas">Canvas to draw on</param>
    private void DrawActivePoint(SKCanvas canvas)
    {
        if (_activePoint is not { } activePoint)
        {
            return;
        }

        _robotViewModel.UpperArmSegment.Angle = KUtils.DegreeToRadian(activePoint.ShoulderAngle);
        _robotViewModel.ForearmSegment.Angle = KUtils.DegreeToRadian(activePoint.ShoulderAngle + activePoint.ElbowAngle);
        _robotViewModel.EffectorSegment.Angle = KUtils.DegreeToRadian(activePoint.ShoulderAngle + activePoint.ElbowAngle + activePoint.WristAngle);
        _robotViewModel.UpperArmSegment.Update();
        _robotViewModel.ForearmSegment.Update();
        _robotViewModel.EffectorSegment.Update();
        _robotViewModel.UpperArmSegment.RelativeAngle = activePoint.ShoulderAngle;
        _robotViewModel.ForearmSegment.RelativeAngle = activePoint.ElbowAngle;
        _robotViewModel.EffectorSegment.RelativeAngle = activePoint.WristAngle;

        // Stick the arm to the base and put the arm back together
        _robotViewModel.UpperArmSegment.PointA = _robotViewModel.RobotArmOriginPosition;
        _robotViewModel.ForearmSegment.PointA = _robotViewModel.UpperArmSegment.PointB;
        _robotViewModel.EffectorSegment.PointA = _robotViewModel.ForearmSegment.PointB;

        if (_robotViewModel.IsShowingDetails)
        {
            _robotViewModel.UpperArmSegment.Draw(canvas, SkiaColors.UpperArmPaint1, SkiaColors.JointPaint1, activePoint.JointsLocks.HasFlag(JointsLocks.Shoulder));
            _robotViewModel.ForearmSegment.Draw(canvas,  SkiaColors.ForearmPaint1,  SkiaColors.JointPaint1, activePoint.JointsLocks.HasFlag(JointsLocks.Elbow));
            _robotViewModel.EffectorSegment.Draw(canvas, SkiaColors.EffectorPaint1, SkiaColors.JointPaint1, activePoint.JointsLocks.HasFlag(JointsLocks.Wrist), activePoint.JointsLocks.HasFlag(JointsLocks.EffectorGrip));
        }
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        SKSurface.InvalidateVisual();

        //if (_robotArmCanvasOrigin == new Point(0, 0))
        //{
        //    var top = Canvas.GetTop(RobotArmCanvas);
        //    var left = Canvas.GetLeft(RobotArmCanvas);
        //    _robotArmCanvasOrigin = new Point(left, top);
        //}
    }

    /// <summary>
    /// Draws the recorded points and the robot arm locked joints
    /// </summary>
    /// <param name="canvas">Skia Canvas to draw on</param>
    /// <param name="rc">the rectangle surface of the Skia Canvas</param>
    private void DrawGraphicDetails(SKCanvas canvas, Rect rc)
    {
        Point prevPoint = new Point(0, 0);
        foreach (MetaPoint p in _robotViewModel.RecordedMetaPoints.Points)
        {
            canvas.DrawCircle((float)p.MousePoint.X, (float)p.MousePoint.Y, 4, SkiaColors.MouseRecordingPaint);
            if (prevPoint != new Point(0, 0))
            {
                canvas.DrawLine((float)prevPoint.X, (float)prevPoint.Y, (float)p.MousePoint.X, (float)p.MousePoint.Y, SkiaColors.PathPaint);
            }

            prevPoint = p.MousePoint;
        }

        if (_robotViewModel.IsShowingDetails)
        {
            // TODO: draw the locked joints in a different color
            _robotViewModel.UpperArmSegment.Draw(canvas, SkiaColors.UpperArmPaint, SkiaColors.JointPaint, _robotViewModel.IsShoulderLocked);
            _robotViewModel.ForearmSegment.Draw(canvas,  SkiaColors.ForearmPaint,  SkiaColors.JointPaint, _robotViewModel.IsElbowLocked);
            _robotViewModel.EffectorSegment.Draw(canvas, SkiaColors.EffectorPaint, SkiaColors.JointPaint, _robotViewModel.IsWristLocked, _robotViewModel.IsEffectorLocked);
        }

        MousePosTextBlock.Text = $"Mouse : X = {_mousePointOld.X:.00},   Y = {_mousePointOld.Y:.00}";
        GripPosTextBlock.Text = $"Grip : X = {_robotViewModel.EffectorSegment.PointB.X:.00},   Y = {_robotViewModel.EffectorSegment.PointB.Y:.00}";
        DrawScale(canvas, rc);
    }

    /// <summary>
    ///  Draw 10 cm scale in the lower right corner knowing that 1 px = 1mm
    /// </summary>
    /// <param name="canvas">canvas to draw in</param>
    /// <param name="rc">rect of the canvas</param>
    private void DrawScale(SKCanvas canvas, Rect rc)
    {
        // Draw a 10 cm scale in the lower right corner knowing that 1 px = 1mm
        var scale = 10; // 10 cm
        var scaleLength = scale * 10; // 100 mm
        var scaleHeight = 10; // 10 mm
        var scaleStart = new Point((rc.Width / 4) - scaleLength, (rc.Height / 2) - scaleHeight - 100);
        var scaleEnd = new Point(rc.Width / 4, (rc.Height / 2) - 100);
        canvas.DrawLine((float)scaleStart.X, (float)scaleEnd.Y - 5, (float)scaleEnd.X, (float)scaleEnd.Y - 5, SkiaColors.EffectorPaint);

        // Draw ticks on the scale every 1 cm
        for (int i = 0; i <= scale; i++)
        {
            var markStart = new Point(scaleStart.X + (i * 10), scaleStart.Y);
            var markEnd = new Point(markStart.X, markStart.Y + scaleHeight);
            canvas.DrawLine((float)markStart.X, (float)markStart.Y, (float)markEnd.X, (float)markEnd.Y, SkiaColors.EffectorPaint);
        }

        // Use the new overload: DrawText(string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint)
        // We'll use SKTextAlign.Left and a default font from the paint
        var font = new SKFont();
        canvas.DrawText($"{scale} cm", (float)scaleStart.X + 25, (float)scaleStart.Y + 26, SKTextAlign.Left, font, SkiaColors.PathPaint);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Close the web server
        //_host.Dispose();
        base.OnClosing(e);
    }
}
