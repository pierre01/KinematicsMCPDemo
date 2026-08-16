using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KinematicsDemo.Models;
using KinematicsDemo.Services;
using KinematicsDemo.Services.MessageBoxService;
using KinematicsDemo.Services.ToastService;
using KinematicsDemo.Styles;
using SkiaSharp;

namespace KinematicsDemo.ViewModels;

/// <summary>
/// Scara RobotArm View Model (Scara = Selective Compliance Articulated Robot Arm.)  
/// (e.g. PF400 Robot Arm)
/// (the arm moves a point on a 2D surface with the constraints of its segments length 
///  and the degrees of freedom of each joint) 
///  
/// With the option to move up and down on a mast (Z axis: HeightPosition) 
/// and moving on a linear rail (T axis: RailPosition) 
/// The T axis can be traveling on Y axis (Y axis: RailPosition) 
///     or on X axis (X axis: RailPosition) depending on how the robot is mounted
/// </summary>
public partial class RobotArmViewModel : ObservableObject
{
    /// <summary>
    /// Minimum number of recorded points 
    /// before confirming the deletion of the recording
    /// </summary>
    private const int MinPointsBeforeConfirm = 5;
    public static Point DefaultRandomPoint = new Point(920, 395);
    CancellationTokenSource? _tokenSource;

    /// <summary>
    /// How many times the Inverse kinematics will try to adjust the robot position towards the mouse
    /// </summary>
    private const int AdjustIterations = 8;

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RobotArmViewModel"/> class.
    /// Default constructor for the Robot Arm View Model (only for design time)
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public RobotArmViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
        {
            throw new InvalidOperationException("This constructor is only for design time");
        }

        RobotArmOriginPosition = new Point(ArmRailPosition, ArmHeightPosition);

        _upperArmSegment = new Segment(0, 0, 225, 0, -90, 90);
        _forearmSegment = new Segment(_upperArmSegment, 210, 0, -167, 167);
        _effectorSegment = new Segment(_forearmSegment, 144, 0, -970, 970);
        MastPositionRange = new KRange(-200, 200); // 40 cm  mast
        RailPositionRange = new KRange(-500, 500); // 1 meter rail - if no rail , then 0
        InitializeEffectorTarget();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RobotArmViewModel"/> class.
    /// Create a Robot Arm with the option to move on a rail and a mast
    /// </summary>
    /// <param name="mastHeightPosition">initial position on the mast.</param>
    /// <param name="heightRange">Height range in mm.</param>
    /// <param name="railPosition">initial position on rail.</param>
    /// <param name="railRange">Travel range in mm.</param>
    /// <param name="upperArmSegment">Inner Link.</param>
    /// <param name="forearmSegment">Outer link.</param>
    /// <param name="effectorSegment">Gripper.</param>
    /// <param name="toastService">Toast Service</param>
    /// <param name="fileDialog">File Dialog Service</param>
    /// <param name="messageBoxService">Message Box Service</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if positions are outside the range passed.</exception>
    public RobotArmViewModel(
        double mastHeightPosition,
        KRange heightRange,
        double railPosition,
        KRange railRange,
        Segment upperArmSegment,
        Segment forearmSegment,
        Segment effectorSegment,
        IMessageBoxService messageBoxService,
        IFileDialogService fileDialog,
        IToastService toastService,
        IToolWindowService toolWindowService)
    {
        if (!heightRange.IsInZeroRange(mastHeightPosition))
        {
            throw new ArgumentOutOfRangeException(nameof(mastHeightPosition), $"{mastHeightPosition} is out of range");
        }

        if (!railRange.IsInZeroRange(railPosition))
        {
            throw new ArgumentOutOfRangeException(nameof(railPosition), $"{railPosition} is out of range");
        }

        MastPositionRange = heightRange;
        ArmMaxHeightPosition = MastPositionRange.Max;
        RailPositionRange = railRange;
        ArmHeightPosition = mastHeightPosition;
        ArmRailPosition = railPosition;
        _toolWindowService = toolWindowService;

        RobotArmOriginPosition = new Point(ArmRailPosition, ArmHeightPosition);
        _upperArmSegment = upperArmSegment;
        _forearmSegment = forearmSegment;
        _effectorSegment = effectorSegment;
        IsShowingDetails = true;
        _messageBox = messageBoxService;
        _fileDialog = fileDialog;
        _toastService = toastService;
        FullyExtendedLenght = upperArmSegment.Length + forearmSegment.Length + effectorSegment.Length;

        InitializeEffectorTarget();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RobotArmViewModel"/> class.
    /// Create a robot arm with the option to move on a mast but NO Rail
    /// </summary>
    /// <param name="mastHeightPosition">initial height from 0 to mast length.</param>
    /// <param name="heightRange">Height range in mm from bottom to top of arm reach.</param>
    /// <param name="upperArmSegment">Inner Link.</param>
    /// <param name="forearmSegment">Outer link.</param>
    /// <param name="effectorSegment">Gripper.</param>
    /// <exception cref="ArgumentOutOfRangeException">range</exception>
    public RobotArmViewModel(
        double mastHeightPosition,
                             KRange heightRange,
                             Segment upperArmSegment,
                             Segment forearmSegment,
                             Segment effectorSegment,
                             IMessageBoxService messageBoxService,
                             IFileDialogService fileDialog,
                             IToastService toastService,
                             IToolWindowService toolWindowService)
    {
        if (!heightRange.IsInRange(mastHeightPosition))
        {
            throw new ArgumentOutOfRangeException(nameof(mastHeightPosition));
        }

        RailPositionRange = new KRange(0, 1000); // 1 meter
        MastPositionRange = heightRange;
        ArmMaxHeightPosition = MastPositionRange.Max;
        ArmHeightPosition = mastHeightPosition;
        _toolWindowService = toolWindowService;

        RobotArmOriginPosition = new Point(0, 0);
        _upperArmSegment = upperArmSegment;
        _forearmSegment = forearmSegment;
        _effectorSegment = effectorSegment;
        _messageBox = messageBoxService;
        _fileDialog = fileDialog;
        _toastService = toastService;

        InitializeEffectorTarget();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RobotArmViewModel"/> class.
    /// Create a robot arm with the option to move on a mast
    /// </summary>
    /// <param name="upperArmSegment">Inner Link</param>
    /// <param name="forearmSegment">Outer link</param>
    /// <param name="effectorSegment">Gripper</param>
    public RobotArmViewModel(
        Segment upperArmSegment, 
                             Segment forearmSegment, 
                             Segment effectorSegment,
                             IMessageBoxService messageBoxService, 
                             IFileDialogService fileDialog, 
                             IToastService toastService, 
                             IToolWindowService toolWindowService)
    {
        RobotArmOriginPosition = new Point(0, 0);
        _upperArmSegment = upperArmSegment;
        _forearmSegment = forearmSegment;
        _effectorSegment = effectorSegment;
        _messageBox = messageBoxService;
        _fileDialog = fileDialog;
        _toastService = toastService;
        _toolWindowService = toolWindowService;
        MastPositionRange = new KRange(0, 400); // 40 cm  mast
        _toastService = toastService;
        InitializeEffectorTarget();
        _toastService = toastService;
    }
    #endregion

    /// <summary>
    /// Initializes command targets from the real end-effector position so the
    /// first command does not depend on the view completing a render pass.
    /// </summary>
    private void InitializeEffectorTarget()
    {
        MousePoint = EffectorSegment.PointB;
        LastSurfacePoint = EffectorSegment.PointB;
        IsMousePointInRobotCoordinates = true;
    }
    
    /// <summary>
    /// If true it will Display Kinematics Skia Graphics on top of robot arm
    /// </summary>
    [ObservableProperty]
    public partial bool IsShowingDetails{ get; set; } = true;

    /// <summary>
    /// Mouse point on the view 
    /// IMPORTANT: This point is modified and transformed during rendering
    /// Do not take it as the real mouse point
    /// Use LastSurfacePoint for the real mouse pressed coordinates 
    /// relative to the Skia Surface.
    /// <see cref="LastSurfacePoint"/>
    /// </summary>
    [ObservableProperty]
    public partial Point MousePoint { get; set; }

    /// <summary>
    /// True when <see cref="MousePoint"/> is already relative to the robot origin.
    /// The view uses this to avoid applying its screen-to-robot translation to
    /// points supplied by commands and MCP tools.
    /// </summary>
    public bool IsMousePointInRobotCoordinates { get; set; }

    [ObservableProperty]
    public partial Point MouseCoordinates{ get; set; }

    /// <summary>
    /// Origin of the arm (Shoulder Joint)
    /// </summary>
    //private Point _base;

    /// <summary>
    /// Points Recordings
    /// </summary>
    [ObservableProperty]
    public partial RobotActionRecording RecordedMetaPoints {get; set; } = new RobotActionRecording();

    /// <summary>
    /// Event trigered by the viewModel when the view needs to refresh itself
    /// </summary>
    public event EventHandler<RefreshDrawingEventArgs>? Refresh;

    #region Arm Segments

    private Segment _upperArmSegment;
    private Segment _forearmSegment;
    private Segment _effectorSegment;

    /// <summary>
    /// The Upper Arm Segment (inner link)
    /// </summary>
    public Segment UpperArmSegment => _upperArmSegment;

    /// <summary>
    /// The Forearm Segment (outer link)
    /// </summary>
    public Segment ForearmSegment => _forearmSegment;

    /// <summary>
    /// the Effector Segment (End Effector, Gripper)
    /// </summary>
    public Segment EffectorSegment => _effectorSegment;

    /// <summary>
    /// We clicked on Add Point button to record the end of the effector
    /// </summary>
    public bool IsPointManuallyAdded { get; set; }

    /// <summary>
    /// Gets full length of the arm when fully extended (defined during construction)
    /// </summary>
    public double FullyExtendedLenght { get; private set; }

    /// <summary>
    /// Last recorded position on the SKIA surface.
    /// </summary>
    public Point LastSurfacePoint { get; internal set; }

    /// <summary>
    /// Gets how close the inverse kinematics calculation should be to the target point in mm
    /// </summary>
    public double Precision { get; internal set; } = 0.7;

    #endregion

    #region Joint Locking properties

    /// <summary>
    /// The Shoulder is the joint between the base and the upper arm is locked
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShoulderRotateCommand))]
    public partial bool IsShoulderLocked { get; set; }

    /// <summary>
    /// If true the arm is moving on the joints angles delta between two points
    /// If false the arm is moving along the straight line between two points
    /// </summary>
    [ObservableProperty]
    public partial bool IsMovingOnJointsDelta { get; set; } = false;

    /// <summary>
    /// Refresh when the shoulder is locked
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsShoulderLockedChanged(bool value)
    {
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }

    /// <summary>
    /// The Elbow is the joint between the upper arm and the forearm is locked
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ElbowRotateCommand))]
    public partial bool IsElbowLocked{ get; set; }

    /// <summary>
    /// Refresh when the elbow is locked
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsElbowLockedChanged(bool value)
    {
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }

    /// <summary>
    /// The Wrist is the joint between the forearm and the effectoris locked
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(WristRotateCommand))]
    public partial bool IsWristLocked{ get; set; }

    /// <summary>
    /// Refresh when the Wrist is locked
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsWristLockedChanged(bool value)
    {
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }

    /// <summary>
    /// Refresh the view when the arm position is changed
    /// </summary>
    public void RefreshDrawing()
    {
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }

    /// <summary>
    /// Locking the effector point to the end of the arm
    /// Meaning the Point B of the effector stays at the same position
    /// Could be used to twist around a point (capper/Decapper)
    /// Rule: If the effector is locked the other joints are unlocked 
    /// </summary>
    [ObservableProperty]
    public partial bool IsEffectorLocked{ get; set; }

    /// <summary>
    /// Refresh when the end effector point is locked   
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsEffectorLockedChanged(bool value)
    {
        // Create a circle where the effector is locked and wrist can rotate around
        _effectorLockCenter = EffectorSegment.PointB;
        MousePoint = _effectorLockCenter;
        _effectorSegment.IsPointBLocked = value;

        //Radius is the distance between the effector and the wrist (_effectorSegment.Length)
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }

    private Point _effectorLockCenter = new Point(0, 0);

    /// <summary>
    /// True if the effector is gripped
    /// </summary>
    [ObservableProperty]
    public partial bool IsEffectorGripped{ get; set; }

    /// <summary>
    /// Refresh when the end effector point is locked   
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsEffectorGrippedChanged(bool value)
    {
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }
    #endregion

    /// <summary>
    /// Refresh when the end effector point is locked   
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsShowingDetailsChanged(bool value)
    {
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }

    /// <summary>
    /// X coordinate on the rail (Zero Based)
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RobotArmOriginPosition))]
    public partial double ArmRailPosition{ get; set; }

    /// <summary>
    /// Redraw the perspective view as soon as the rail carriage moves.
    /// </summary>
    /// <param name="value">The new rail position.</param>
    partial void OnArmRailPositionChanged(double value)
    {
        GoForwardCommand.NotifyCanExecuteChanged();
        GoBackwardCommand.NotifyCanExecuteChanged();
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }

    /// <summary>
    /// Y coordinate on the Mast (zero based)
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RobotArmOriginPosition))]
    public partial double ArmHeightPosition{ get; set; }

    /// <summary>
    /// Redraw the perspective view as soon as the mast carriage moves.
    /// </summary>
    /// <param name="value">The new mast height.</param>
    partial void OnArmHeightPositionChanged(double value)
    {
        GoUpCommand.NotifyCanExecuteChanged();
        GoDownCommand.NotifyCanExecuteChanged();
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }

    /// <summary>
    /// Gets or sets the maximum vertical position, in units, that the arm can reach.
    /// </summary>
    /// <remarks>The value represents the upper limit for the arm's height. Setting this property to a value
    /// lower than the current arm position may restrict movement. Ensure that the value is within the supported range
    /// of the hardware.</remarks>
    public double ArmMaxHeightPosition{ get; set; } = 400;

    /// <summary>
    /// Increment of the angle in radiant
    /// </summary>
    private double _angleIncrement = KUtils.DegreeToRadian(1); // Increment by one degree

    /// <summary>
    /// Keeps track of the current playback index
    /// </summary>
    private int _playbackIndex;

    #region State Management

    [ObservableProperty]
    public partial string LoadedRobotActionName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LoadedRobotActionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Recorded points array are being recorded
    /// </summary>
    [ObservableProperty]
    public partial bool IsRecording{ get; set; }

    /// <summary>
    /// Recorded points array are being played back
    /// </summary>
    [ObservableProperty]
    public partial bool IsPlaying{ get; set; }

    #endregion

    #region Robot Arm Position properties

    /// <summary>
    /// Origin of the shoulder joint on the plane made by the rail and the mast
    /// X=position on the rail (zero based)
    /// Y=position on the mast (zero based)
    /// </summary>
    [ObservableProperty]
    public partial Point RobotArmOriginPosition { get; set; } = new Point(0, 0);

    /// <summary>
    /// Range of the rail position in mm (min,max) 
    /// </summary>
    [ObservableProperty]
    public partial KRange? RailPositionRange{ get; set; }

    /// <summary>
    /// Range of the height position in mm (min,max) From the floor
    /// </summary>
    [ObservableProperty]
    public partial KRange MastPositionRange{ get; set; }

    private IToastService _toastService;
    private IMessageBoxService _messageBox;
    private IFileDialogService _fileDialog;
    private IToolWindowService _toolWindowService;
    private MetaPoint? _activePoint;

    #endregion

    #region Commands

    /// <summary>
    /// Home the robot arm
    /// </summary>
    [RelayCommand]
    private void GoHome()
    {
        ArmRailPosition = RailPositionRange?.GetClosestValueInRange(0) ?? 0;
        ArmHeightPosition = MastPositionRange.GetClosestValueInRange(0);
        RobotArmOriginPosition = new Point(ArmRailPosition, ArmHeightPosition);

        UpperArmSegment.Angle = 0;
        ForearmSegment.Angle = 0;
        EffectorSegment.Angle = 0;
        UpperArmSegment.RelativeAngle = 0;
        ForearmSegment.RelativeAngle = 0;
        EffectorSegment.RelativeAngle = 0;

        UpperArmSegment.PointA = RobotArmOriginPosition;
        ForearmSegment.PointA = UpperArmSegment.PointB;
        EffectorSegment.PointA = ForearmSegment.PointB;

        MousePoint = EffectorSegment.PointB;
        LastSurfacePoint = EffectorSegment.PointB;
        IsMousePointInRobotCoordinates = true;
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);

        //_messageBox.Show("Robot Arm Homed...","Robot",MessageBoxServiceButton.Ok);   
    }

    /// <summary>
    /// Clear the recorded points
    /// Pops a confirmation if the number of points is greater than 5
    /// </summary>
    [RelayCommand]
    private void ClearRecording()
    {
        // Only confirm deletion if there are more than 5 points
        if (RecordedMetaPoints.Points.Count > MinPointsBeforeConfirm)
        {
            var res = _messageBox?.Show("Are you sure you want to clear the points recording?", "Erase Records", MessageBoxServiceButton.OkCancel);
            if (res == MessageBoxServiceResult.OK)
            {
                RecordedMetaPoints.Points.Clear();
                Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
            }
        }
        else
        {
            RecordedMetaPoints.Points.Clear();
            Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
        }

        LoadedRobotActionName = string.Empty;
        LoadedRobotActionDescription = string.Empty;
    }

    /// <summary>
    /// Save recording to file
    /// </summary>
    [RelayCommand]
    private void SaveRecording()
    {
        // Save _recordedPoints array to a local file
        _fileDialog.Filter = "Robot Pendant Files|*.pendant";
        _fileDialog.Title = "Save pendant Points to File";
        _fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        RecordedMetaPoints.Name = LoadedRobotActionName;
        RecordedMetaPoints.Description = LoadedRobotActionDescription;
        if (!_fileDialog.SaveMetaPointsToFile(RecordedMetaPoints))
        {
            _messageBox.Show("Error saving file", "Error", MessageBoxServiceButton.Ok);
        }
    }

    /// <summary>
    /// Load recording from file
    /// </summary>
    [RelayCommand]
    private void LoadRecording()
    {
        // Load _recordedPoints array from a local file
        _fileDialog.Filter = "Robot Pendant Files|*.pendant";
        _fileDialog.Title = "Load pendant Points from File";
        _fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        RecordedMetaPoints = _fileDialog.LoadMetaPointsFromFile();
        LoadedRobotActionName = RecordedMetaPoints.Name ?? string.Empty;
        LoadedRobotActionDescription = RecordedMetaPoints.Description ?? string.Empty;
    }

    /// <summary>
    /// When the Add button is clicked it adds the effector B point 
    /// to the list 
    /// The B point is the end effector position
    /// </summary>
    [RelayCommand]
    private void AddPoint()
    {
        // Find way to differentiate when the teach pendant called it and when the user clicked it so we can refresh the teach pendant view
        MousePoint = EffectorSegment.PointB;
        var angleLock = (JointsLocks)(IsShoulderLocked ? 1 : 0) + (IsElbowLocked ? 2 : 0) + (IsWristLocked ? 4 : 0);

        //_recordedMetaPoints.Add(new MetaPoint( new Point(_mousePoint.X, _mousePoint.Y),1,angleLock));
        IsMousePointInRobotCoordinates = true;
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
        RecordedMetaPoints.Add(new MetaPoint(
            EffectorSegment.PointB, 
            1, 
            angleLock, 
            KUtils.NormalizeAngle(UpperArmSegment.RelativeAngle), 
            KUtils.NormalizeAngle(ForearmSegment.RelativeAngle), 
            KUtils.NormalizeAngle(EffectorSegment.RelativeAngle),
            ArmHeightPosition,
            ArmRailPosition,
            EffectorSegment.PointB));

        //MousePoint = EffectorSegment.PointB;
        //_messageBox.Show("Adding point to record","Recording",MessageBoxServiceButton.Ok);   
        _toastService.ShowToast("Point Added to Record", ToastLocation.ApplicationTopCenter, BadgeTypeEnum.Success, 3);
    }

    [RelayCommand]
    private void StopPlay()
    {
        IsPlaying = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Async</returns>
    [RelayCommand]
    private async Task Play()
    {
        // If no joint is locked, try to follow the straight line between two recorded points.
        if (IsPlaying)
        {
            return;
        }

        IsPlaying = true;
        _playbackIndex = 0;
        List<MetaPoint> playbackPoints = new List<MetaPoint>();

        // for each successive pair of points in the list, calculate the distance between them and the number of steps required to get there
        // then create a list of points in between the two points
        // then add the list of points to the playback list
        for (int i = 0; i < RecordedMetaPoints.Points.Count - 1; i++)
        {
            // Check if the arm is following a straight line (no joint is locked at anytime)
            bool isFollowingEffectorLine = false;

            //  Debug.WriteLine($"Origin: Shoulder {RecordedMetaPoints.Points[i].ShoulderAngle} elbow: {RecordedMetaPoints.Points[i].ElbowAngle} wrist: {RecordedMetaPoints.Points[i].WristAngle}");
            // remove abs because it gives the direction of the angle

            // TODO: instead of moving the joints, but this may cause unwanted movements
            // let's move the end effector as close as possible between the two points
            // then calculate the angles to get there
            // THE MAIN CHALLENGE HERE IS WHEN THE EFFECTOR HAS TO MOVE IN A CIRCLE around a point
            if (RecordedMetaPoints.Points[i].JointsLocks.HasFlag(JointsLocks.None))
            {
                // no joint is locked, we will try to follow the straight line between 2 recorded points by calculating
                // the closest kinematic point to the point on the line 
                isFollowingEffectorLine = true;
            }

            if (!isFollowingEffectorLine || IsMovingOnJointsDelta) // isFollowingEffectorLine == false or articulating the joints
            {
                var shoulderAngleDistance = KUtils.GetAngleInBetweenAandB(RecordedMetaPoints.Points[i].ShoulderAngle, RecordedMetaPoints.Points[i + 1].ShoulderAngle);
                var elbowAngleDistance = KUtils.GetAngleInBetweenAandB(RecordedMetaPoints.Points[i].ElbowAngle, RecordedMetaPoints.Points[i + 1].ElbowAngle);
                var wristAngleDistance = KUtils.GetAngleInBetweenAandB(RecordedMetaPoints.Points[i].WristAngle, RecordedMetaPoints.Points[i + 1].WristAngle);
                var shoulderSteps = (int)(shoulderAngleDistance * 2);
                var elbowSteps = (int)(elbowAngleDistance * 2);
                var wristSteps = (int)(wristAngleDistance * 2);
                int maxSteps = (int)KUtils.GetDistanceBetweenTwoPoints(RecordedMetaPoints.Points[i].MousePoint, RecordedMetaPoints.Points[i + 1].MousePoint);

                //var maxSteps = Math.Max(Math.Max(Math.Abs(shoulderSteps), Math.Abs(elbowSteps)), Math.Abs(wristSteps));
                var shoulderPoints = KUtils.GetAnglesInBetweenTwoAngles(RecordedMetaPoints.Points[i].ShoulderAngle, RecordedMetaPoints.Points[i + 1].ShoulderAngle, maxSteps);
                var elbowPoints = KUtils.GetAnglesInBetweenTwoAngles(RecordedMetaPoints.Points[i].ElbowAngle, RecordedMetaPoints.Points[i + 1].ElbowAngle, maxSteps);
                var wristPoints = KUtils.GetAnglesInBetweenTwoAngles(RecordedMetaPoints.Points[i].WristAngle, RecordedMetaPoints.Points[i + 1].WristAngle, maxSteps);
                for (int j = 0; j < maxSteps; j++)
                {
                    playbackPoints.Add(new MetaPoint(RecordedMetaPoints.Points[i].MousePoint, j == 0 ? 1 : 2, RecordedMetaPoints.Points[i].JointsLocks, shoulderPoints[j], elbowPoints[j], wristPoints[j], 0, 0, RecordedMetaPoints.Points[i].EffectorGripPoint));

                    //Debug.WriteLine($"Shoulder: {shoulderPoints[j]} elbow: {elbowPoints[j]} wrist: {wristPoints[j]}");
                }

                var lastPoint = playbackPoints[playbackPoints.Count - 1];
            }
            else// isFollowingEffectorLine == true or IsMovingOnJointsDelta == false
            {
                // Set points based on the recorded points kinematics calculations
                double distance = KUtils.GetDistanceBetweenTwoPoints(RecordedMetaPoints.Points[i].MousePoint, RecordedMetaPoints.Points[i + 1].MousePoint);
                var pts = KUtils.GetPointsInBetweenTwoPoints(RecordedMetaPoints.Points[i].MousePoint, RecordedMetaPoints.Points[i + 1].MousePoint, (int)distance);
                for (int j = 0; j < pts.Count; j++)
                {
                    playbackPoints.Add(CreateMetaPointFromKinematics(Precision, pts[j]));

                    //Debug.WriteLine($"Shoulder: {shoulderPoints[j]} elbow: {elbowPoints[j]} wrist: {wristPoints[j]}");
                }
            }

            //Debug.WriteLine($"Last shoulder: {lastPoint.ShoulderAngle} elbow: {lastPoint.ElbowAngle} wrist: {lastPoint.WristAngle}");                                                                                                                                                                                                                                                                                                                                             
        }

        _tokenSource = new CancellationTokenSource();
        CancellationToken ct = _tokenSource.Token;
        using (var armPositionTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(5)))
        {
            try
            {
                // Play the points
                // TODO: Add Cancelation Token
                while (await armPositionTimer.WaitForNextTickAsync(ct))
                {
                    if (_playbackIndex >= playbackPoints.Count || IsPlaying == false)
                    {
                        _tokenSource.Cancel();
                        break;
                    }

                    MousePoint = playbackPoints[_playbackIndex].MousePoint;
                    IsWristLocked = playbackPoints[_playbackIndex].JointsLocks.HasFlag(JointsLocks.Wrist);
                    IsElbowLocked = playbackPoints[_playbackIndex].JointsLocks.HasFlag(JointsLocks.Elbow);
                    IsShoulderLocked = playbackPoints[_playbackIndex].JointsLocks.HasFlag(JointsLocks.Shoulder);
                    IsEffectorLocked = playbackPoints[_playbackIndex].JointsLocks.HasFlag(JointsLocks.EffectorGrip);

                    // When playing draw the active point
                    _activePoint = playbackPoints[_playbackIndex];
                    Debug.WriteLine($"[{playbackPoints[_playbackIndex].Speed}] -- Shoulder: {playbackPoints[_playbackIndex].ShoulderAngle} elbow: {playbackPoints[_playbackIndex].ElbowAngle} wrist: {playbackPoints[_playbackIndex].WristAngle}");

                    _playbackIndex++;

                    Refresh?.Invoke(this, new RefreshDrawingEventArgs(_activePoint));
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("Playback Canceled");
            }
            finally
            {
                IsPlaying = false;
                _playbackIndex = 0;
            }
        }
    }

    /// <summary>
    /// Start editing the recorded points
    /// </summary>
    [RelayCommand(CanExecute = nameof(EditCanExecute))]
    private void Edit()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Can the command be executed
    /// </summary>
    /// <returns>true if you can edit a point</returns>
    private bool EditCanExecute()
    {
        return false;
    }

    /// <summary>
    /// Rotate the shoulder joint by the angle increment
    /// </summary>
    /// <param name="angleDirection">Command parameter: postitive or negative number +1 or -1 based on the direction </param>
    [RelayCommand(CanExecute = nameof(CanExecuteShoulderRotate))]
    private void ShoulderRotate(string? angleDirection)
    {
        if (IsShoulderLocked)
        {
            return;
        }

        double direction = 1;
        if (double.TryParse(angleDirection, out direction))
        {
            direction = Math.Sign(direction);
        }

        RotateBy(_angleIncrement * direction, _angleIncrement * direction, _angleIncrement * direction);
    }

    /// <summary>
    /// Can the shoulder rotate command be executed
    /// </summary>
    /// <returns>false if the shoulder is locked</returns>
    private bool CanExecuteShoulderRotate()
    {
        return !IsShoulderLocked;
    }

    /// <summary>
    /// Close or open the effector to grip on an object (plate, tube, bottle...)
    /// </summary>
    [RelayCommand]
    private void EffectorGrip()
    {
        // Toggles the grip state
        if (IsEffectorGripped)
        {
            // TODO: replace by async call to the robot
            _messageBox.Show("Un-Gripping...", "Grip", MessageBoxServiceButton.Ok);
            IsEffectorGripped = false;
        }
        else
        {
            // TODO: replace by async call to the robot
            _messageBox.Show("Gripping...", "Grip", MessageBoxServiceButton.Ok);
            IsEffectorGripped = true;
        }
    }

    /// <summary>
    ///  Rotate the elbow joint by the angle increment
    /// </summary>
    /// <param name="angleDirection">Command parameter: postitive or negative number +1 or -1 based on the direction </param>
    [RelayCommand(CanExecute = nameof(CanExecuteElbowRotate))]
    private void ElbowRotate(string? angleDirection)
    {
        if (IsElbowLocked)
        {
            return;
        }

        double direction = 1;
        if (double.TryParse(angleDirection, out direction))
        {
            direction = Math.Sign(direction);
        }

        RotateBy(0, _angleIncrement * direction, _angleIncrement * direction);
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }

    /// <summary>
    /// Check if the elbow is locked
    /// </summary>
    /// <returns> true if we can rotate the elbow</returns>
    private bool CanExecuteElbowRotate()
    {
        return !IsElbowLocked;
    }

    /// <summary>
    /// Rotate the wrist joint by the angle increment
    /// </summary>
    /// <param name="angleDirection">Command parameter: postitive or negative number +1 or -1 based on the direction </param>
    [RelayCommand(CanExecute = nameof(CanExecuteWristRotate))]
    private void WristRotate(string? angleDirection)
    {
        if (IsWristLocked)
        {
            return;
        }

        double direction = 1;
        if (double.TryParse(angleDirection, out direction))
        {
            direction = Math.Sign(direction);
        }

        RotateBy(0, 0, _angleIncrement * direction);
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteGoDown))]
    private void GoDown(object param)
    {
        var offset = (double) param;
        var pos = MastPositionRange.GetClosestValueInRange(ArmHeightPosition - offset);
        if(pos == ArmHeightPosition)
        {
            return;
        }

        ArmHeightPosition = pos;
    }

    private bool CanExecuteGoDown()
    {
        return !MastPositionRange.IsImmobile && ArmHeightPosition != MastPositionRange.Min;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteGoUp))]
    private void GoUp(object param) 
    {
        var offset = (double) param;
        var pos = MastPositionRange.GetClosestValueInRange(ArmHeightPosition + offset);
        if(pos == ArmHeightPosition)
        {
            return;
        }

        ArmHeightPosition = pos;
    }

    private bool CanExecuteGoUp()
    {
        return !MastPositionRange.IsImmobile && ArmHeightPosition!= MastPositionRange.Max;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteGoForward))]
    private void GoForward(object param)
    {
        if(RailPositionRange == null)
        {
            return;
        }

        double offset = (double) param;
        var pos = RailPositionRange.GetClosestValueInRange(ArmRailPosition + offset);
        if(pos == ArmRailPosition)
        {
            return;
        }
        //// change the x value of the mouse point to simulate the arm moving
        //MousePoint = new Point(MousePoint.X - offset, MousePoint.Y);
        ArmRailPosition = pos;
    }

    private bool CanExecuteGoForward()
    {
        return RailPositionRange != null && !RailPositionRange.IsImmobile && ArmRailPosition != RailPositionRange.Max;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteGoBackward))]
    private void GoBackward(object param)
    {
        if(RailPositionRange == null)
        {
            return;
        }

        double offset = (double) param; 
        var pos = RailPositionRange.GetClosestValueInRange(ArmRailPosition - offset);
        if(pos == ArmRailPosition)
        {
            return;
        }

        ArmRailPosition = pos;
    }

    private bool CanExecuteGoBackward()
    {
        return RailPositionRange!=null && !RailPositionRange.IsImmobile && ArmRailPosition != RailPositionRange.Min;
    }

    /// <summary>
    /// Check if the wrist is locked before rotating it
    /// </summary>
    /// <returns>true if the wrist is not locked </returns>
    private bool CanExecuteWristRotate()
    {
        return !IsWristLocked;
    }

    #endregion

    #region Utility Functions  

    /// <summary>
    ///  Rotate the joints by an increment unless the joint is locked
    /// </summary>
    /// <param name="shoulderAngle">Shoulder Joint rotation increment in radians</param>
    /// <param name="elbowAngle">Elbow rotation increment in radians</param>
    /// <param name="wristAngle">Wrist rotation increment in radians</param>
    private void RotateBy(double shoulderAngle, double elbowAngle, double wristAngle)
    {
        if (_effectorSegment == null || _forearmSegment == null || _upperArmSegment == null)
        {
            return;
        }

        //if(_isElbowLocked){ elbowAngle=0;}
        //if(_isShoulderLocked){ shoulderAngle=0;}
        //if(_isWristLocked){ wristAngle=0;}
        //_upperArmSegment.RotateBy(shoulderAngle,_upperArmSegment.Angle);
        //_forearmSegment.RotateBy(elbowAngle,_forearmSegment.Angle-_upperArmSegment.Angle);
        //_effectorSegment.RotateBy(wristAngle,_effectorSegment.Angle-_forearmSegment.Angle-_upperArmSegment.Angle);
        _upperArmSegment.Rotate(shoulderAngle, shoulderAngle);
        _forearmSegment.Rotate(elbowAngle, shoulderAngle != 0 ? 0 : elbowAngle);
        _effectorSegment.Rotate(wristAngle, elbowAngle != 0 ? 0 : wristAngle);

        //_upperArmSegment.PointA = _base;

        // Attach Forearm to UpperArm
        _forearmSegment.PointA = _upperArmSegment.PointB;

        // Attach Wrist to Forearm
        _effectorSegment.PointA = _forearmSegment.PointB;

        // Send event to the view to Update
        Refresh?.Invoke(this, RefreshDrawingEventArgs.Empty);
    }

    /// <summary>
    /// Check the distance between shoulder origin and effector end
    /// so that the wrist can go around the effector end.
    /// - The effector end (or grip Point)is fixed.
    /// (used for capping and uncapping)
    /// </summary>
    /// <param name="shoulderOriginPoint">Origin</param>
    /// <param name="effectorGripPoint">Effector End</param>
    /// <returns>true if you can</returns>
    public bool CanWristGoAroundEffectorEnd(Point shoulderOriginPoint, Point effectorGripPoint)
    {
        double L1 = _upperArmSegment.Length;
        double L2 = _forearmSegment.Length;
        double L3 = _effectorSegment.Length;

        // calculate distance between shoulderOriginPoint and effectorGripPoint using Pythagorean theorem
        double distance = Math.Sqrt(Math.Pow(effectorGripPoint.X - shoulderOriginPoint.X, 2) + Math.Pow(effectorGripPoint.Y - shoulderOriginPoint.Y, 2));

        // calculate limits based on the lengths of the segments
        double lowerLimit = Math.Max(Math.Abs(L1 - L2), Math.Abs(L2 - L3));
        double upperLimit = Math.Min(L1 + L2, L2 + L3);

        // return true if the distance is within the limits
        return distance > lowerLimit && distance < upperLimit;
    }

    public void RecordPoint(MetaPoint metaPoint)
    {
        RecordedMetaPoints.Add(metaPoint);
    }
    #endregion

    /// <summary>
    /// Show the teach pendant window
    /// </summary>
    [RelayCommand]
    public void ShowTeachPendant()
    {
        _toolWindowService.ShowPendantWindow(this);
    }

    /// <summary>
    /// Run the inverse Kinematics to adjust the robot arm to the mouse position
    /// </summary>
    /// <param name="deltaTolerance">Defines how close we should be to the target point in mm 
    /// if close enough, the iteration loop breaks </param>
    /// <param name="canvas">The canvas to draw on</param>
    public void RunInverseKinematics(double deltaTolerance, SKCanvas canvas)
    {
        SeedRetractFromFullyExtendedPose();

        // ---- Inverse kinematics Loop ----
        for (int i = 0; i < AdjustIterations; i++) //  iterations to make it closer to the goal
        {
            if (!IsWristLocked)
            {
                // Gripper follows the mouse
                if (IsEffectorLocked)
                {
                    // get point on the circumference of the circle of the effector reach
                    var ptInCircle = KUtils.ClosestPointOnCircumference(MousePoint, EffectorSegment.PointB, EffectorSegment.Length);
                    EffectorSegment.PointA = ptInCircle;
                }
                else
                {
                    EffectorSegment.Follow(new Point(MousePoint.X, MousePoint.Y));
                    EffectorSegment.Update();
                }
            }

            if (!IsElbowLocked)
            {
                if (IsWristLocked)
                {
                    var curAngle = ForearmSegment.Angle;
                    ForearmSegment.Follow(new Point(MousePoint.X, MousePoint.Y)); // Shift by the effector
                    ForearmSegment.Update();
                    var deltaAngle = ForearmSegment.Angle - curAngle;

                    // var resultAngle = KUtils.GetClosestAngleBetweenTwoAngles( _effectorSegment.Angle + deltaAngle, _effectorSegment.MinAngle,_effectorSegment.MaxAngle);
                    //_effectorSegment.Angle = resultAngle;
                    EffectorSegment.Angle += deltaAngle;
                    EffectorSegment.Update();
                }
                else
                {
                    ForearmSegment.Follow(EffectorSegment);
                    ForearmSegment.Update();
                }
            }

            if (!IsShoulderLocked)
            {
                if (IsElbowLocked)
                {
                    if (IsWristLocked)
                    {
                        UpperArmSegment.Follow(new Point(MousePoint.X, MousePoint.Y));
                        UpperArmSegment.Update();
                    }
                    else
                    {
                        var curAngle = UpperArmSegment.Angle;
                        UpperArmSegment.Follow(EffectorSegment);
                        UpperArmSegment.Update();
                        var deltaAngle = UpperArmSegment.Angle - curAngle;

                        ForearmSegment.Angle += deltaAngle;
                        ForearmSegment.Update();
                    }
                }
                else
                {
                    UpperArmSegment.Follow(ForearmSegment);
                    UpperArmSegment.Update();
                }
            }

            // Stick the arm to the base and put the arm back together
            // Singing the skeleton song
            UpperArmSegment.PointA = RobotArmOriginPosition;
            ForearmSegment.PointA = UpperArmSegment.PointB;
            EffectorSegment.PointA = ForearmSegment.PointB;

            UpperArmSegment.RelativeAngle = KUtils.RadianToDegree(UpperArmSegment.Angle);
            ForearmSegment.RelativeAngle = KUtils.RadianToDegree(ForearmSegment.Angle -UpperArmSegment.Angle);
            if (!IsEffectorLocked)
            {
                EffectorSegment.RelativeAngle = KUtils.RadianToDegree(EffectorSegment.Angle - ForearmSegment.Angle);
            }

            if (i == 0) // Draw the first iteration in light shading to show the difference after adjustments
            {
                //var distance =  GetDistanceBetweenTwoPoints(_mousePoint,_effectorSegment.PointB) ;
                if (IsShowingDetails && canvas != null)
                {
                    UpperArmSegment.Draw(canvas,  SkiaColors.UpperArmPaint1, SkiaColors.JointPaint1, IsShoulderLocked);
                    ForearmSegment.Draw(canvas, SkiaColors.ForearmPaint1, SkiaColors.JointPaint1, IsElbowLocked);
                    EffectorSegment.Draw(canvas, SkiaColors.EffectorPaint1, SkiaColors.JointPaint1, IsWristLocked, IsEffectorLocked);
                }
            }

            // Break the inverse kinematic loop if we are within the distance tolerance
            if (KUtils.GetDistanceBetweenTwoPoints(MousePoint, EffectorSegment.PointB) < deltaTolerance)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Breaks the fully extended collinear singularity when the target asks the
    /// arm to retract. Without a small bend, every inverse-kinematics iteration
    /// remains on the same line and the rigid links cannot move inward.
    /// </summary>
    private void SeedRetractFromFullyExtendedPose()
    {
        const double angleTolerance = 0.000001;
        const double distanceTolerance = 0.001;
        const double seedAngleDegrees = 1;

        bool isFullyStraight =
            Math.Abs(UpperArmSegment.Angle - ForearmSegment.Angle) < angleTolerance &&
            Math.Abs(ForearmSegment.Angle - EffectorSegment.Angle) < angleTolerance;

        double targetDistance = KUtils.GetDistanceBetweenTwoPoints(RobotArmOriginPosition, MousePoint);
        double currentReach = KUtils.GetDistanceBetweenTwoPoints(RobotArmOriginPosition, EffectorSegment.PointB);
        if (!isFullyStraight || targetDistance >= currentReach - distanceTolerance)
        {
            return;
        }

        double seedAngle = KUtils.DegreeToRadian(seedAngleDegrees);
        ForearmSegment.Angle = UpperArmSegment.Angle + seedAngle;
        EffectorSegment.Angle = ForearmSegment.Angle;
        ForearmSegment.PointA = UpperArmSegment.PointB;
        EffectorSegment.PointA = ForearmSegment.PointB;
    }

    /// <summary>
    /// Run the inverse Kinematics to adjust the robot arm as close as possible to a point 
    /// This function expects none of the robot joints to be locked
    /// </summary>
    /// <param name="deltaTolerance">Defines how close we should be to the target point in mm 
    /// if close enough, the iteration loop breaks </param>
    /// <param name="pointToFollow">The point to follow</param>
    /// <returns>MetaPoint</returns>
    public MetaPoint CreateMetaPointFromKinematics(double deltaTolerance, Point pointToFollow )
    {
        var effectorSegment = new Segment(EffectorSegment);
        var forearmSegment = new Segment(ForearmSegment);
        var upperArmSegment = new Segment(UpperArmSegment);

        // ---- Inverse kinematics Loop ----
        for (int i = 0; i < AdjustIterations; i++) //  iterations to make it closer to the goal
        {
            // Gripper follows the mouse
            effectorSegment.Follow(new Point(pointToFollow.X, pointToFollow.Y));
            effectorSegment.Update();

            forearmSegment.Follow(effectorSegment);
            forearmSegment.Update();

            upperArmSegment.Follow(forearmSegment);
            upperArmSegment.Update();

            // Stick the arm to the base and put the arm back together
            // .....Singing The Skeleton Dance song....
            upperArmSegment.PointA = RobotArmOriginPosition;
            forearmSegment.PointA = upperArmSegment.PointB;
            effectorSegment.PointA = forearmSegment.PointB;

            upperArmSegment.RelativeAngle = KUtils.RadianToDegree(upperArmSegment.Angle);
            forearmSegment.RelativeAngle = KUtils.RadianToDegree(forearmSegment.Angle - upperArmSegment.Angle);

            effectorSegment.RelativeAngle = KUtils.RadianToDegree(effectorSegment.Angle - forearmSegment.Angle);

            // Break the inverse kinematic loop if we are within the distance tolerance
            if (KUtils.GetDistanceBetweenTwoPoints(pointToFollow, effectorSegment.PointB) < deltaTolerance)
            {
                break;
            }
        }

        return new MetaPoint(pointToFollow, 1d, JointsLocks.None, KUtils.NormalizeAngle(upperArmSegment.RelativeAngle), KUtils.NormalizeAngle(forearmSegment.RelativeAngle), KUtils.NormalizeAngle(effectorSegment.RelativeAngle), ArmHeightPosition, ArmRailPosition, effectorSegment.PointB);
    }
}
