using KinematicsDemo.Models;
using KinematicsDemo.Services;
using KinematicsDemo.Services.MessageBoxService;
using KinematicsDemo.Services.ToastService;
using KinematicsDemo.ViewModels;
using System.Windows;

namespace KinematicsDemoTests.ViewModels;

/// <summary>
/// Implements mock of all the services needed by the view model
/// </summary>
public class AllServices : IFileDialogService, IMessageBoxService, IToastService, IToolWindowService
{
    string IFileDialogService.FilePath { get; set; } = "";
    string IFileDialogService.Filter { get; set; } = "";
    string IFileDialogService.Title { get; set; } = "";
    string IFileDialogService.InitialDirectory { get; set; } = "";

    public Queue<string> FunctionsCalls { get; } = new Queue<string>();

    public AllServices()
    {

    }

    RobotActionRecording IFileDialogService.LoadMetaPointsFromFile()
    {
        FunctionsCalls.Enqueue(nameof(IFileDialogService.LoadMetaPointsFromFile));
        return new RobotActionRecording();
    }

    bool IFileDialogService.SaveMetaPointsToFile(RobotActionRecording recording)
    {
        FunctionsCalls.Enqueue(nameof(IFileDialogService.SaveMetaPointsToFile));
        return true;
    }

    MessageBoxServiceResult IMessageBoxService.Show(string messageBoxText)
    {
        FunctionsCalls.Enqueue(nameof(IMessageBoxService.Show));
        return MessageBoxServiceResult.OK;
    }

    MessageBoxServiceResult IMessageBoxService.Show(string messageBoxText, string caption)
    {
        FunctionsCalls.Enqueue(nameof(IMessageBoxService.Show));
        return MessageBoxServiceResult.OK;
    }

    MessageBoxServiceResult IMessageBoxService.Show(string messageBoxText, string caption, MessageBoxServiceButton button)
    {
        FunctionsCalls.Enqueue(nameof(IMessageBoxService.Show));
        return MessageBoxServiceResult.OK;
    }

    MessageBoxServiceResult IMessageBoxService.Show(string messageBoxText, string caption, MessageBoxServiceButton button, MessageBoxServiceIcon icon)
    {
        FunctionsCalls.Enqueue(nameof(IMessageBoxService.Show));
        return MessageBoxServiceResult.OK;
    }

    MessageBoxServiceResult IMessageBoxService.Show(string messageBoxText, string caption, MessageBoxServiceButton button, MessageBoxServiceIcon icon, MessageBoxServiceResult defaultResult)
    {
        FunctionsCalls.Enqueue(nameof(IMessageBoxService.Show));
        return MessageBoxServiceResult.OK;
    }

    MessageBoxServiceResult IMessageBoxService.Show(Window owner, string messageBoxText)
    {
        FunctionsCalls.Enqueue(nameof(IMessageBoxService.Show));
        return MessageBoxServiceResult.OK;
    }

    MessageBoxServiceResult IMessageBoxService.Show(Window owner, string messageBoxText, string caption)
    {
        FunctionsCalls.Enqueue(nameof(IMessageBoxService.Show));
        return MessageBoxServiceResult.OK;
    }

    MessageBoxServiceResult IMessageBoxService.Show(Window owner, string messageBoxText, string caption, MessageBoxServiceButton button)
    {
        FunctionsCalls.Enqueue(nameof(IMessageBoxService.Show));
        return MessageBoxServiceResult.OK;
    }

    MessageBoxServiceResult IMessageBoxService.Show(Window owner, string messageBoxText, string caption, MessageBoxServiceButton button, MessageBoxServiceIcon icon)
    {
        FunctionsCalls.Enqueue(nameof(IMessageBoxService.Show));
        return MessageBoxServiceResult.OK;
    }

    MessageBoxServiceResult IMessageBoxService.Show(Window owner, string messageBoxText, string caption, MessageBoxServiceButton button, MessageBoxServiceIcon icon, MessageBoxServiceResult defaultResult)
    {
        FunctionsCalls.Enqueue(nameof(IMessageBoxService.Show));
        return MessageBoxServiceResult.OK;
    }

    void IToastService.ShowToast(string message, ToastLocation location, BadgeTypeEnum badgeType, int timeout, bool isClosable)
    {
        FunctionsCalls.Enqueue(nameof(IMessageBoxService.Show));
    }

    public void ClearResponses()
    {
        FunctionsCalls.Clear();
    }

    public void ShowPendantWindow(RobotArmViewModel robotViewModel)
    {
        throw new NotImplementedException();
    }
}
