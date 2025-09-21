using KinematicsDemo.ViewModels;

namespace KinematicsDemo.Services
{
    /// <summary>
    /// Service to display Modeless windows
    /// </summary>
    public interface IToolWindowService
    {
        /// <summary>
        /// Show the teach pendant window
        /// </summary>
        /// <param name="robotViewModel">parent viewModel</param>
        void ShowPendantWindow(RobotArmViewModel robotViewModel);
    }
}
