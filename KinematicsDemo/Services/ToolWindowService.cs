using System.Windows;
using KinematicsDemo.ViewModels;
using KinematicsDemo.Views;

namespace KinematicsDemo.Services;

/// <summary>
/// Provides functionality to display and manage tool windows related to robot arm operations within the application.
/// </summary>
/// <remarks>This service is typically used to show specialized windows, such as the teach pendant interface, for
/// interacting with robot arm view models. It is intended to be used by components that require user interaction with
/// robot arm controls in a dedicated window.</remarks>
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
