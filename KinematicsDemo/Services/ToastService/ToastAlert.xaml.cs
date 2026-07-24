using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KinematicsDemo.Services.ToastService;

/// <summary>
/// Interaction logic for ToastAlert.xaml
/// </summary>
public partial class ToastAlert : UserControl
{
    public event EventHandler? Closed;

    private Brush _badgeBorderBrush = Brushes.Transparent;
    private Brush _badgeFillBrush = Brushes.Transparent;
    private Brush _badgeTextBrush = Brushes.Transparent;
    private string _text = string.Empty;
    private bool _showCloseButton;
    private int _timeout;
    private Timer? _timeoutTimer;

    public Brush BadgeBorderBrush
    {
        get => _badgeBorderBrush; set { _badgeBorderBrush = value; ToastBorder.BorderBrush = value; }
    }

    public Brush BadgeFillBrush
    {
        get => _badgeFillBrush; set { _badgeFillBrush = value; ToastBorder.Background = value; }
    }

    public Brush BadgeTextBrush
    {
        get => _badgeTextBrush; set { _badgeTextBrush = value; ToastText.Foreground = value; }
    }

    public string Text
    {
        get => _text; set { _text = value; ToastText.Text = _text; }
    }

    public int ToastTimeout
    {
        get => _timeout; set { _timeout = value; }
    }

    public bool ShowCloseButton
    {
        get => _showCloseButton; set { _showCloseButton = value; CloseButton.Visibility = value == false ? Visibility.Collapsed : Visibility.Visible; SeparatorRect.Visibility = value == false ? Visibility.Collapsed : Visibility.Visible; }
    }

    public ToastAlert(string message, BadgeTypeEnum badgeType, int timeout, bool isClosable, bool isListVisualReversed=true)
    {
        InitializeComponent();

        switch (badgeType)
        {
            case BadgeTypeEnum.Primary:
                BadgeBorderBrush = new SolidColorBrush((Color)FindResource("Primary50"));
                BadgeFillBrush = new SolidColorBrush((Color)FindResource("Primary400"));
                BadgeTextBrush = new SolidColorBrush((Color)FindResource("Primary600"));
                break;
            case BadgeTypeEnum.Information:
                BadgeBorderBrush = new SolidColorBrush((Color)FindResource("StaticGray900"));
                BadgeFillBrush = new SolidColorBrush((Color)FindResource("StaticGray700"));
                BadgeTextBrush = new SolidColorBrush((Color)FindResource("StaticGray50"));
                break;
            case BadgeTypeEnum.Help:
                BadgeBorderBrush = new SolidColorBrush((Color)FindResource("StaticGray900"));
                BadgeFillBrush = new SolidColorBrush((Color)FindResource("StaticGray700"));
                BadgeTextBrush = new SolidColorBrush((Color)FindResource("StaticGray50"));
                break;
            case BadgeTypeEnum.Success:
                BadgeBorderBrush = new SolidColorBrush((Color)FindResource("StaticGreen800"));
                BadgeFillBrush = new SolidColorBrush((Color)FindResource("StaticGreen600"));
                BadgeTextBrush = new SolidColorBrush((Color)FindResource("StaticGreen50"));
                break;
            case BadgeTypeEnum.Warning:
                BadgeBorderBrush = new SolidColorBrush((Color)FindResource("StaticYellow500"));
                BadgeFillBrush = new SolidColorBrush((Color)FindResource("StaticYellow400"));
                BadgeTextBrush = new SolidColorBrush(Colors.Black);
                break;
            case BadgeTypeEnum.Error:
                BadgeBorderBrush = new SolidColorBrush((Color)FindResource("StaticRed800"));
                BadgeFillBrush = new SolidColorBrush((Color)FindResource("StaticRed600"));
                BadgeTextBrush = new SolidColorBrush((Color)FindResource("StaticRed50"));
                break;
            case BadgeTypeEnum.Gray:
                BadgeBorderBrush = new SolidColorBrush((Color)FindResource("StaticGray50"));
                BadgeFillBrush = new SolidColorBrush((Color)FindResource("StaticGray300"));
                BadgeTextBrush = new SolidColorBrush((Color)FindResource("StaticGray600"));
                break;
            case BadgeTypeEnum.Green:
                BadgeBorderBrush = new SolidColorBrush((Color)FindResource("StaticGreen50"));
                BadgeFillBrush = new SolidColorBrush((Color)FindResource("StaticGreen300"));
                BadgeTextBrush = new SolidColorBrush((Color)FindResource("StaticGreen600"));
                break;
            case BadgeTypeEnum.Yellow:
                BadgeBorderBrush = new SolidColorBrush((Color)FindResource("StaticYellow50"));
                BadgeFillBrush = new SolidColorBrush((Color)FindResource("StaticYellow300"));
                BadgeTextBrush = new SolidColorBrush((Color)FindResource("StaticYellow600"));
                break;
            case BadgeTypeEnum.Red:
                BadgeBorderBrush = new SolidColorBrush((Color)FindResource("StaticRed50"));
                BadgeFillBrush = new SolidColorBrush((Color)FindResource("StaticRed300"));
                BadgeTextBrush = new SolidColorBrush((Color)FindResource("StaticRed600"));
                break;
            case BadgeTypeEnum.Blue:
                BadgeBorderBrush = new SolidColorBrush((Color)FindResource("StaticBlue50"));
                BadgeFillBrush = new SolidColorBrush((Color)FindResource("StaticBlue300"));
                BadgeTextBrush = new SolidColorBrush((Color)FindResource("StaticBlue600"));
                break;
        }

        Text = message;
        ShowCloseButton = isClosable;
        ToastTimeout = timeout;
        Loaded += OnToastLoaded;
        if(isListVisualReversed== false)
        { 
            ToastScale.ScaleY = 1;
        }
    }

    public ToastAlert()
    {
        InitializeComponent();

        Loaded += OnToastLoaded;
    }

    private void OnToastLoaded(object sender, RoutedEventArgs e)
    {
        _timeoutTimer = new Timer(TimeoutCallback, null, ToastTimeout * 1000, Timeout.Infinite);
    }

    private void TimeoutCallback(object? state)
    {
        Dispatcher.Invoke(() =>
        {
            Closed?.Invoke(this, EventArgs.Empty);
        });
    }

    private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Closed?.Invoke(this, e);
    }
}
