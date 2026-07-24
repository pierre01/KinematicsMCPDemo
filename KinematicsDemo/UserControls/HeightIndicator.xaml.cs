using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using KinematicsDemo.ViewModels;

namespace KinematicsDemo.UserControls;

/// <summary>
/// Interaction logic for HeightIndicator.xaml
/// </summary>
public partial class HeightIndicator : UserControl
{
    /// <summary>
    /// Maximum Arm reach, defaults to 40 cm
    /// </summary>
    public static readonly DependencyProperty MaxArmHeightProperty =
        DependencyProperty.Register("MaxArmHeight", typeof(double), typeof(HeightIndicator), new PropertyMetadata(400D));

    /// <summary>
    ///  Current Arm Height
    /// </summary>
    public static readonly DependencyProperty ArmHeightProperty =
        DependencyProperty.Register("ArmHeight", typeof(double), typeof(HeightIndicator), new PropertyMetadata(0D));

    private const double PixelTopHeight = -146D;

    /// <summary>
    /// Gets or sets the current arm z Position
    /// </summary>
    [Category("Arm")]
    public double ArmHeight
    {
        get
        {
            return (double)GetValue(ArmHeightProperty);
        }

        set
        {
           SetValue(ArmHeightProperty, value);
        }
    }

    /// <summary>
    /// Called when any of the properties are changed
    /// </summary>
    /// <param name="e">Event arguments</param>
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if(e.Property.Name == nameof(DataContext) && DataContext is RobotArmViewModel robotArmViewModel)
        {
            MaxArmHeight = robotArmViewModel.ArmMaxHeightPosition;
            MaxHeightTextBlock.Text = $"{MaxArmHeight:F2}";
        }

        if (e.Property.Name == nameof(ArmHeight))
        {
            MoveArm();
        }

        if (e.Property.Name == nameof(MaxArmHeight))
        {
            MaxHeightTextBlock.Text = $"{MaxArmHeight:F2}";
            MoveArm();
        }
    }

    /// <summary>
    /// Move the Arm to the current height
    /// </summary>
    private void MoveArm()
    {
        ArmTranslate.Y = (ArmHeight / MaxArmHeight) * PixelTopHeight;
        ArmHeightTextBlock.Text = $"{ArmHeight:F2}";
    }

    /// <summary>
    /// Gets or sets the maximum arm height
    /// </summary>
    [Category("Arm")]
    public double MaxArmHeight
    {
        get { return (double)GetValue(MaxArmHeightProperty); }
        set { SetValue(MaxArmHeightProperty, value); }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeightIndicator"/> class.
    /// </summary>
    public HeightIndicator()
    {
        InitializeComponent();
    }
}