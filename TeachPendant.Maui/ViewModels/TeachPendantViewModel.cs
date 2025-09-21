using Biosero.TeachPendant.Common;
using Biosero.TeachPendant.Common.Communicators;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Biosero.TeachPendant.Maui.ViewModels
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TeachPendantViewModel"/> class.
    /// </summary>
    public partial class TeachPendantViewModel : ObservableObject
    {
        private const int _bufferedMoveDelayMilliSeconds = 500;

        private const int _positionUpdateIntervalMilliSec = 1000;

        private readonly KinematicsDemoCommunicator _kinematicDemoCommunicator;

        private readonly MoveBuffer _moveBuffer = new();

        private readonly object _coordinateLock = new();

        private DateTime _lastButtonPress = DateTime.Now;

        private TimeSpan TimeSinceButtonPressed
            => DateTime.Now - _lastButtonPress;

        private int ButtonIterations
            => (int)TimeSinceButtonPressed.TotalMilliseconds / _bufferedMoveDelayMilliSeconds;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(XString))]
        private double _x;

        /// <summary>
        /// Gets the X position for the teach pendant in mm to display in the UI
        /// </summary>
        public string XString
            => $"{X:F2}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(YString))]
        private double _y;

        /// <summary>
        /// Gets the Y position for the teach pendant in mm to display in the UI
        /// </summary>
        public string YString
            => $"{Y:F2}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ZString))]
        private double _z;

        /// <summary>
        /// Gets the Z position for the teach pendant in mm to display in the UI
        /// </summary>
        public string ZString
            => $"{Z:F2}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RailPositionString))]
        private double _railPosition;

        /// <summary>
        /// Gets the rail position for the teach pendant in mm to display in the UI
        /// </summary>
        public string RailPositionString
            => $"{RailPosition:F2}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StepPrecisionString))]
        private double _stepPrecision = 5.0; // 5mm

        /// <summary>
        /// Gets step precision for the teach pendant in mm to display in the UI
        /// </summary>
        public string StepPrecisionString
            => $"{StepPrecision:F0} mm";

        /// <summary>
        /// Initializes a new instance of the <see cref="TeachPendantViewModel"/> class.
        /// </summary>
        public TeachPendantViewModel(string url) 
            : this (new KinematicsDemoCommunicator(url))
        {}

        public TeachPendantViewModel(KinematicsDemoCommunicator communicator)
        {
            _kinematicDemoCommunicator = communicator;
            _moveBuffer.InitializeNewBufferCoordinate();

            Task.Run(UpdatePositionsPeriodically);
        }

        internal async Task UpdatePositionsPeriodically()
        {
            while (true)
            {
                UpdatePositions();
                await Task.Delay(_positionUpdateIntervalMilliSec);
            }
        }

        private void UpdatePositions()
        {
            lock (_coordinateLock)
            {
                var coordinate = _kinematicDemoCommunicator.GetCoordinates();
                X = coordinate.X;
                Y = coordinate.Y;
                Z = coordinate.Z;
                RailPosition = coordinate.Rail;
            }
        }

        /// <summary>
        /// Get the step precision of the robot
        /// </summary>
        [RelayCommand]
        public void GetStepPrecision()
        {
            StepPrecision = _kinematicDemoCommunicator.GetStepPrecision();
        }

        /// <summary>
        /// Record the current point
        /// </summary>
        [RelayCommand]
        public void RecordPoint()
        {
            _kinematicDemoCommunicator.RecordPoint();
        }

        /// <summary>
        /// Move effector to the home position
        /// </summary>
        [RelayCommand]
        public void GoHome()
        {
            _kinematicDemoCommunicator.GoHome();
        }

        /// <summary>
        /// Ask the Robot to play the recorded points
        /// </summary>
        [RelayCommand]
        public void Play()
        {
            _kinematicDemoCommunicator.Play();
        }

        /// <summary>
        /// Stop the robot from playing the recorded points
        /// </summary>
        [RelayCommand]
        public void StopPlay()
        {
            _kinematicDemoCommunicator.StopPlay();
        }

        /// <summary>
        /// Move effector to the north
        /// </summary>
        [RelayCommand]
        public void GoNorth()
        {
            SetBufferedMoveDirection(MoveDirection.North);
        }

        /// <summary>
        /// Move effector to the south
        /// </summary>
        [RelayCommand]
        public void GoSouth()
        {
            SetBufferedMoveDirection(MoveDirection.South);
        }

        /// <summary>
        /// Move effector to the west
        /// </summary>
        [RelayCommand]
        public void GoWest()
        {
            SetBufferedMoveDirection(MoveDirection.West);
        }

        /// <summary>
        /// Move effector to the east
        /// </summary>
        [RelayCommand]
        public void GoEast()
        {
            SetBufferedMoveDirection(MoveDirection.East);
        }

        /// <summary>
        /// Move effector up on the Mast (z axis)
        /// </summary>
        [RelayCommand]
        public void GoUp()
        {
            SetBufferedMoveDirection(MoveDirection.Up);
        }

        /// <summary>
        /// Move effector down on the Mast (z axis)
        [RelayCommand]
        public void GoDown()
        {
            SetBufferedMoveDirection(MoveDirection.Down);
        }

        /// <summary>
        /// Move robot forward on the rail (x or y axis)
        /// </summary>
        [RelayCommand]
        public void GoForward()
        {
            SetBufferedMoveDirection(MoveDirection.Forward);
        }

        /// <summary>
        /// Move robot backward on the rail (x or y axis)
        /// </summary>
        [RelayCommand]
        public void GoBackward()
        {
            SetBufferedMoveDirection(MoveDirection.Backward);
        }

        [RelayCommand]
        public void ShowRecordedPoints()
        {

        }

        private void SetBufferedMoveDirection(MoveDirection direction)
        {
            _lastButtonPress = DateTime.Now;
            _moveBuffer.SetBufferedMoveDirection(direction);
        }

        internal void MoveToBufferedCoordinates()
        {
            _moveBuffer.MoveInBufferedDirection(StepPrecision * ButtonIterations);
            _kinematicDemoCommunicator.Move(_moveBuffer.GetCoordinate());
            _moveBuffer.InitializeNewBufferCoordinate();
        }
    }
}
