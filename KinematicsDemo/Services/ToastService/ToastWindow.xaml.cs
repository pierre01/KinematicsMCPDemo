using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace KinematicsDemo.Services.ToastService;

/// <summary>
/// Container for toast alerts, placed on the bottom right corner of the screen or on the top right corner of the screen...
/// </summary>
public partial class ToastWindow : Window
{

    ObservableCollection<ToastAlert> _alerts = new ObservableCollection<ToastAlert>();
    bool _isListReversed = true;

    public ToastWindow(bool reverseListVisual=true)
    {
        InitializeComponent();
        if(reverseListVisual== false)
        {
            _isListReversed=reverseListVisual;
            // todo: flip the ToastList 
            ListScale.ScaleY = 1;
        }
    }


    private void WindowDrag(object sender, MouseButtonEventArgs e)
    {
        try
        {
            this.DragMove();
        }
        catch (Exception)
        {

        }
    }

    public void AddToast(string message, BadgeTypeEnum badgeType, int timeout, bool isClosable)
    {
        ToastAlert alert = new ToastAlert( message,  badgeType,  timeout,  isClosable,_isListReversed);
        alert.Closed += Alert_Closed;
        ToastList.Items.Insert(0, alert); // allows the fluidLayout behavior to work
    }

    private void Alert_Closed(object sender, EventArgs e)
    {
        var alert = sender as ToastAlert;
        ToastList.Items.Remove(sender); // allows the fluidLayout behavior to work
        alert.Closed -= Alert_Closed;
    }

}
// Not Used in this project yet
public enum AnimationType
{
    None,
    Fade,
    Slide
}

public enum BadgeTypeEnum
{
    Primary,
    Information,
    Help,
    Success,
    Warning,
    Error,
    Gray,
    Green,
    Yellow,
    Red,
    Blue
}

