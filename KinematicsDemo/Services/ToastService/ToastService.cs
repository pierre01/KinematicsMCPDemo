using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace KinematicsDemo.Services.ToastService;

/// <summary>
/// Service that displays toast messages in different part of the screen or the application window
/// </summary>
public class ToastService : IToastService
{
    public static ToastService Instance { get; } = new ToastService();

    private ToastService()
    {
    }

    ToastWindow? _screenBottomRightWindow;
    ToastWindow? _appTopCenterWindow;
    ToastWindow? _appBottomRightWindow;
    ToastWindow? _screenBottomLeftWindow;

    Rect _windowRect;

    /// <summary>
    /// Show a toast message at the specified location
    /// For each location an invisible window will appear to host the toast messages
    /// </summary>
    /// <param name="message"></param>
    /// <param name="location">ToastLocation.ScreenBottomRight is the most used location across operating systems</param>
    /// <param name="badgeType">Color look of the alert : Information, Help,  Success, Warning, Error. are the most used</param>
    /// <param name="timeout">Time to wait on the screen until it times-out or is manually closed (in seconds unless is <=0 then it stays until the app closes</param> 
    /// <param name="isClosable">Does the toast includes a close button</param>
    public void ShowToast(string message, ToastLocation location, BadgeTypeEnum badgeType, int timeout, bool isClosable = true)
    {
        if (location == ToastLocation.ScreenBottomLeft)
        {
            if (_screenBottomLeftWindow == null)
            {
                _screenBottomLeftWindow = new ToastWindow();
                _screenBottomLeftWindow.Owner = App.Current.MainWindow;

                // Get the main Display info
                var rc = SystemParameters.WorkArea;
                _screenBottomLeftWindow.Top = rc.Bottom - _screenBottomLeftWindow.Height;
                _screenBottomLeftWindow.Left = rc.Left;
                _screenBottomLeftWindow.Show();
            }

            _screenBottomLeftWindow.AddToast(message, badgeType, timeout, isClosable);
        }
        else

        if (location == ToastLocation.ApplicationBottomRight)
        {
            var rc = new Rect(App.Current.MainWindow.Left, App.Current.MainWindow.Top, App.Current.MainWindow.Width, App.Current.MainWindow.Height);

            if (_appBottomRightWindow == null)
            {
                _appBottomRightWindow = new ToastWindow();
                _appBottomRightWindow.Owner = App.Current.MainWindow;

                // Get the main Display info
                _appBottomRightWindow.Top = rc.Bottom - _appBottomRightWindow.Height - 4; // TODO: Should be window.border height
                _appBottomRightWindow.Left = rc.Right - _appBottomRightWindow.Width - 40;
                _appBottomRightWindow.Show();
                _windowRect = rc;
            }
            else
            {
                if (rc != _windowRect)
                {
                    _appBottomRightWindow.Top = rc.Bottom - _appBottomRightWindow.Height - 4; //  TODO: Should be window.border height
                    _appBottomRightWindow.Left = rc.Right - _appBottomRightWindow.Width - 40; //  TODO: Should be window.border with x2
                    _windowRect = rc;
                }
            }

            _appBottomRightWindow.AddToast(message, badgeType, timeout, isClosable);
        }
        else
        if (location == ToastLocation.ApplicationTopCenter)
        {
            var rc = new Rect(App.Current.MainWindow.Left, App.Current.MainWindow.Top, App.Current.MainWindow.Width, App.Current.MainWindow.Height);

            if (_appTopCenterWindow == null)
            {
                _appTopCenterWindow = new ToastWindow(false);
                _appTopCenterWindow.Owner = App.Current.MainWindow;

                // Get the main Display info
                _appTopCenterWindow.Top = rc.Top + 20; // TODO: Should be window.border height
                _appTopCenterWindow.Left = rc.Left + (rc.Width / 2) - (_appTopCenterWindow.Width / 2);
                _appTopCenterWindow.Show();
                _windowRect = rc;
            }
            else
            {
                if (rc != _windowRect)
                {
                    _appTopCenterWindow.Top = rc.Top + 20; // TODO: Should be window.border height
                    _appTopCenterWindow.Left = rc.Left + (rc.Width / 2) - (_appTopCenterWindow.Width / 2);
                    _windowRect = rc;
                }
            }

            _appTopCenterWindow.AddToast(message, badgeType, timeout, isClosable);
        }
        else
        {
            if (_screenBottomRightWindow == null)
            {
                _screenBottomRightWindow = new ToastWindow();
                _screenBottomRightWindow.Owner = App.Current.MainWindow;

                // Get the main Display info
                var rc = SystemParameters.WorkArea;
                _screenBottomRightWindow.Top = rc.Bottom - _screenBottomRightWindow.Height;
                _screenBottomRightWindow.Left = rc.Right - _screenBottomRightWindow.Width - 40;
                _screenBottomRightWindow.Show();
            }

            _screenBottomRightWindow.AddToast(message, badgeType, timeout, isClosable);
        }
    }
}
