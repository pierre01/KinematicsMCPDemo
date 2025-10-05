using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KinematicsDemo.Models;
using KinematicsDemo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KinematicsDemo.ViewModels
{
    /// <summary>
    /// ViewModel for the TeachPendantView
    /// Allows simple control of the robot arm 
    /// by moving the effector in 3d space (+  the rail )
    /// </summary>
    public partial class TeachPendantViewModel : ObservableObject
    {
        private readonly IWebServerService _webServerService;
        private RobotArmViewModel _robotViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeachPendantViewModel"/> class.
        /// </summary>
        /// <param name="robotViewModel">ViewModel </param>
        public TeachPendantViewModel(RobotArmViewModel robotViewModel)
        {
            _webServerService = App.Current.Services.GetRequiredService<IWebServerService>();

            _robotViewModel = robotViewModel;
            _x = _robotViewModel.MousePoint.X;
            _y = _robotViewModel.MousePoint.Y;            
            _z = _robotViewModel.ArmHeightPosition;
            _railPosition = _robotViewModel.ArmRailPosition;

        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(XString))]
        private double _x;

        public string XString
        {
            get
            {
               // return $"{X:F2}";
                 return $"{_robotViewModel.MousePoint.X:F2}";
            }
        }

        public ObservableCollection<MetaPoint> RecordedMetaPoints => _robotViewModel.RecordedMetaPoints.Points;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(YString))]
        private double _y;

        public string YString
        {
            get
            {
                //return $"{Y:F2}";
                 return $"{_robotViewModel.MousePoint.Y:F2}";
           }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ZString))]
        private double _z;

        public string ZString
        {
            get
            {
                return $"{Z:F2}";
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RailPositionString))]
        private double _railPosition;

        public string RailPositionString
        {
            get
            {
                return $"{RailPosition:F2}";
            }
        }



        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StepPrecisionString))]
        private double _stepPrecision = 5.0; // 5mm

        /// <summary>
        /// Gets step precision for the teach pendant in mm to display in the UI
        /// </summary>
        public string StepPrecisionString
        {
            get
            {
                return $"{StepPrecision:F0} mm";
            }
        }

        [RelayCommand]
        private async void SwitchRemoteMode(object param)
        {
            bool isRemote = (bool)param;
            if (isRemote)
            {
                RemoteModeTooltip = "Stop Mobile Server";
                await _webServerService.StartAsync(string.Empty);
            }
            else
            {
                RemoteModeTooltip = "Start Mobile Server";
                await _webServerService.StopAsync();
            }
        }

        [ObservableProperty]
        public partial string RemoteModeTooltip { get; set; } = "Stop Mobile Server";


        /// <summary>
        /// Record the current point
        /// </summary>
        [RelayCommand]
        public void RecordPoint()
        {
            _robotViewModel.AddPointCommand.Execute(null);
        }

        /// <summary>
        /// Move effector to the home position
        /// </summary>
        [RelayCommand]
        public void GoHome()
        {
            _robotViewModel.GoHomeCommand.Execute(null);
        }

        /// <summary>
        /// Ask the Robot to play the recorded points
        /// </summary>
        [RelayCommand]
        public void Play()
        {
            _robotViewModel.PlayCommand.Execute(null);
        }

        /// <summary>
        /// Stop the robot from playing the recorded points
        /// </summary>
        [RelayCommand]
        public void StopPlay()
        {
            _robotViewModel.StopPlayCommand.Execute(null);
        }

        /// <summary>
        /// Move effector to the north
        /// </summary>
        [RelayCommand]
        public void GoNorth()
        {
            var m = _robotViewModel.LastSurfacePoint;
            m.Y -= StepPrecision;
            Y = m.Y;
            UpdateAndRefresh(m);
        }

        /// <summary>
        /// Move effector to the south
        /// </summary>
        [RelayCommand]
        public void GoSouth()
        {
            var m = _robotViewModel.LastSurfacePoint;
            m.Y += StepPrecision;
            Y = m.Y;
            UpdateAndRefresh(m);
        }

        /// <summary>
        /// Move effector to the west
        /// </summary>
        [RelayCommand]
        public void GoWest()
        {
            var m = _robotViewModel.LastSurfacePoint;
            m.X -= StepPrecision;
            X = m.X;
            UpdateAndRefresh(m);
        }

        /// <summary>
        /// Move effector to the east
        /// </summary>
        [RelayCommand]
        public void GoEast()
        {
            var m = _robotViewModel.LastSurfacePoint;
            m.X += StepPrecision;
            X = m.X;
            UpdateAndRefresh(m);
        }

        /// <summary>
        /// Move effector up on the Mast (z axis)
        /// </summary>
        [RelayCommand]
        public void GoUp()
        {

                _robotViewModel.GoUpCommand.Execute(StepPrecision);
                Z = _robotViewModel.ArmHeightPosition;
        }

        /// <summary>
        /// Move effector down on the Mast (z axis)
        [RelayCommand]
        public void GoDown()
        {

                _robotViewModel.GoDownCommand.Execute(StepPrecision);
                Z = _robotViewModel.ArmHeightPosition;

        }

        /// <summary>
        /// Move robot forward on the rail (x or y axis)
        /// </summary>
        [RelayCommand]
        public void GoForward()
        {
            if (_robotViewModel.GoForwardCommand.CanExecute(StepPrecision))
            {
                _robotViewModel.GoForwardCommand.Execute(StepPrecision);
                RailPosition = _robotViewModel.ArmRailPosition;
            }
        }

        /// <summary>
        /// Move robot backward on the rail (x or y axis)
        /// </summary>
        [RelayCommand]
        public void GoBackward()
        {
            if (_robotViewModel.GoBackwardCommand.CanExecute(StepPrecision))
            {
                _robotViewModel.GoBackwardCommand.Execute(StepPrecision);
                RailPosition = _robotViewModel.ArmRailPosition;
            }
        }        
        
        private void UpdateAndRefresh(Point m)
        {
            _robotViewModel.MousePoint = m;
            _robotViewModel.LastSurfacePoint = m;
            _robotViewModel.RefreshDrawing();
        }

    }
}
