using Biosero.TeachPendant.Common;
using Biosero.TeachPendant.Common.Communicators;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Biosero.TeachPendant.Maui.ViewModels
{
    public partial class RecordedPointsViewModel : ObservableObject
    {
        private const int _recordPointsUpdateIntervalMilliSec = 1000;

        private readonly KinematicsDemoCommunicator _kinematicDemoCommunicator;

        [ObservableProperty]
        private IEnumerable<RobotCoordinate> _recordedPoints = new List<RobotCoordinate>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TeachPendantViewModel"/> class.
        /// </summary>
        public RecordedPointsViewModel(string url)
            : this(new KinematicsDemoCommunicator(url))
        { }

        public RecordedPointsViewModel(KinematicsDemoCommunicator communicator)
        {
            _kinematicDemoCommunicator = communicator;

            Task.Run(UpdateRecordedPointsPeriodically);
        }

        internal async Task UpdateRecordedPointsPeriodically()
        {
            while (true)
            {
                UpdateRecordedPoints();
                await Task.Delay(_recordPointsUpdateIntervalMilliSec);
            }
        }

        private void UpdateRecordedPoints()
        {
            //_recordedPoints = _kinematicDemoCommunicator.GetRecordedPoints();
        }
    }
}
