using KinematicsDemo.ViewModels;
using KinematicsDemo.Views;
using System.Windows;

namespace KinematicsDemo.Services
{

    public class ToolWindowService : IToolWindowService
    {
        private static TeachPendantWindow? _teachPendanWindow;


        /// <inheritdoc/>
        public void ShowPendantWindow(RobotArmViewModel robotViewModel)
        {
            if(_teachPendanWindow == null)
            {
                var teachPendantViewModel    = new TeachPendantViewModel(robotViewModel);
                _teachPendanWindow = new TeachPendantWindow(teachPendantViewModel);
                _teachPendanWindow.Closed += (s, e) => _teachPendanWindow = null;
                _teachPendanWindow.Owner = Application.Current.MainWindow;
            }
  
            _teachPendanWindow.Show();
            
        }
    }
}
