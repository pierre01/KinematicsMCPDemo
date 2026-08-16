using KinematicsDemo.Models;
using KinematicsDemo.Services;
using KinematicsDemo.Services.MessageBoxService;
using KinematicsDemo.Services.ToastService;
using KinematicsDemo.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace KinematicsDemoTests.ViewModels;
[TestClass()]

public class RobotArmViewModelTests
{
    private readonly RobotArmViewModel _robotArmViewModel;
    private readonly Mock<IFileDialogService> _fileDialogServiceMock = new Mock<IFileDialogService>();
    private readonly Mock<IToastService> _toastServiceMock = new Mock<IToastService>();
    private readonly Mock<IMessageBoxService> _messageBoxServiceMock = new Mock<IMessageBoxService>();
    private readonly AllServices _allServices = new AllServices();

    public RobotArmViewModelTests()
    {
        //var serviceProvider = new ServiceCollection().AddCommunityToolkit().BuildServiceProvider();
        //Ioc.Default.ConfigureServices(serviceProvider);
        var upperArmSegment = new Segment(0, 0, 225, 0, -90, 90);
        var forearmSegment = new Segment(upperArmSegment, 210, 0, -167, 167);
        var effectorSegment = new Segment(forearmSegment, 144, 0, -970, 970);
        var heightRange = new KRange(-200, 200); // 40 cm  mast
        var railRange = new KRange(-500, 500); // 1 meter rail
         
        
       _robotArmViewModel = new RobotArmViewModel(0, heightRange, 0, railRange, upperArmSegment, forearmSegment, effectorSegment,
                _messageBoxServiceMock.Object, _allServices, _allServices,_allServices);
        _robotArmViewModel.Refresh += _robotArmViewModel_Refresh;

    }

    bool _refreshed = false;
    private void _robotArmViewModel_Refresh(object? sender, EventArgs e)
    {
        _refreshed = true;
        
    }

    [TestInitialize]
    public void TestItnitialization()
    {
        _refreshed = false;
        _allServices.ClearResponses();
    }

    [TestMethod]
    public void IsShoulderLockedChanged_RefreshesView()
    {
        // Arrange

        // Act
        _robotArmViewModel.IsShoulderLocked = true;

        // Assert
        Assert.IsTrue(_refreshed);
        
    }

    [TestMethod]
    public void IsElbowLockedChanged_RefreshesView()
    {
        // Arrange

        // Act
        _robotArmViewModel.IsElbowLocked = true;

        // Assert
        Assert.IsTrue(_refreshed);
    }

    [TestMethod]
    public void IsWristLockedChanged_RefreshesView()
    {
        // Arrange

        // Act
        _robotArmViewModel.IsWristLocked = true;

        // Assert
        Assert.IsTrue(_refreshed);
    }

    [TestMethod]
    public void IsEffectorLockedChanged_RefreshesView()
    {
        // Arrange

        // Act
        _robotArmViewModel.IsEffectorLocked = true;

        // Assert
        Assert.IsTrue(_refreshed);
    }

    [TestMethod]
    public void IsEffectorGrippedChanged_RefreshesView()
    {
        // Arrange

        // Act
        _robotArmViewModel.IsEffectorGripped = true;

        // Assert
        Assert.IsTrue(_refreshed);
    }

    [TestMethod]
    public void IsShowingDetailsChanged_RefreshesView()
    {
        // Arrange

        // Act
        _robotArmViewModel.IsShowingDetails = false;

        // Assert
        Assert.IsTrue(_refreshed);
    }

    [TestMethod]
    public void MousePointChanged_RefreshesView()
    {
        // Arrange

        // Act
        _robotArmViewModel.MousePoint = new System.Windows.Point(0, 0);

        // Assert
        Assert.IsFalse(_refreshed);
    }

    [TestMethod]
    public void Initialization_UsesEffectorPositionForFirstCommand()
    {
        Assert.AreEqual(_robotArmViewModel.EffectorSegment.PointB, _robotArmViewModel.MousePoint);
        Assert.AreEqual(_robotArmViewModel.EffectorSegment.PointB, _robotArmViewModel.LastSurfacePoint);
        Assert.IsTrue(_robotArmViewModel.IsMousePointInRobotCoordinates);
    }

    [TestMethod]
    public void GoForward_IncreasesRailPosition()
    {
        var initialPosition = _robotArmViewModel.ArmRailPosition;

        _robotArmViewModel.GoForwardCommand.Execute(25d);

        Assert.AreEqual(initialPosition + 25d, _robotArmViewModel.ArmRailPosition);
        Assert.IsTrue(_refreshed);
    }

    [TestMethod]
    public void GoBackward_DecreasesRailPosition()
    {
        var initialPosition = _robotArmViewModel.ArmRailPosition;

        _robotArmViewModel.GoBackwardCommand.Execute(25d);

        Assert.AreEqual(initialPosition - 25d, _robotArmViewModel.ArmRailPosition);
        Assert.IsTrue(_refreshed);
    }

    [TestMethod]
    public void GoUpAndDown_UsePositiveMagnitude()
    {
        var initialPosition = _robotArmViewModel.ArmHeightPosition;

        _robotArmViewModel.GoUpCommand.Execute(25d);
        Assert.AreEqual(initialPosition + 25d, _robotArmViewModel.ArmHeightPosition);
        Assert.IsTrue(_refreshed);

        _refreshed = false;
        _robotArmViewModel.GoDownCommand.Execute(25d);
        Assert.AreEqual(initialPosition, _robotArmViewModel.ArmHeightPosition);
        Assert.IsTrue(_refreshed);
    }

    [TestMethod]
    public void GoHome_ResetsAllAxesAndReportedEffectorPosition()
    {
        _robotArmViewModel.GoForwardCommand.Execute(25d);
        _robotArmViewModel.GoUpCommand.Execute(25d);
        _robotArmViewModel.UpperArmSegment.Angle = 0.5;
        _robotArmViewModel.ForearmSegment.Angle = -0.25;
        _robotArmViewModel.EffectorSegment.Angle = 0.75;
        _robotArmViewModel.UpperArmSegment.RelativeAngle = 30;
        _robotArmViewModel.ForearmSegment.RelativeAngle = -45;
        _robotArmViewModel.EffectorSegment.RelativeAngle = 60;
        _robotArmViewModel.MousePoint = new System.Windows.Point(100, 100);

        _robotArmViewModel.GoHomeCommand.Execute(null);

        Assert.AreEqual(0, _robotArmViewModel.ArmRailPosition);
        Assert.AreEqual(0, _robotArmViewModel.ArmHeightPosition);
        Assert.AreEqual(0, _robotArmViewModel.UpperArmSegment.Angle);
        Assert.AreEqual(0, _robotArmViewModel.ForearmSegment.Angle);
        Assert.AreEqual(0, _robotArmViewModel.EffectorSegment.Angle);
        Assert.AreEqual(0, _robotArmViewModel.UpperArmSegment.RelativeAngle);
        Assert.AreEqual(0, _robotArmViewModel.ForearmSegment.RelativeAngle);
        Assert.AreEqual(0, _robotArmViewModel.EffectorSegment.RelativeAngle);
        Assert.AreEqual(new System.Windows.Point(579, 0), _robotArmViewModel.EffectorSegment.PointB);
        Assert.AreEqual(_robotArmViewModel.EffectorSegment.PointB, _robotArmViewModel.MousePoint);
        Assert.AreEqual(_robotArmViewModel.EffectorSegment.PointB, _robotArmViewModel.LastSurfacePoint);
        Assert.IsTrue(_robotArmViewModel.IsMousePointInRobotCoordinates);
        Assert.IsTrue(_refreshed);
    }

    [TestMethod]
    public void RetractFromHome_ArticulatesArm()
    {
        _robotArmViewModel.GoHomeCommand.Execute(null);
        _robotArmViewModel.MousePoint = new System.Windows.Point(529, 0);

        _robotArmViewModel.RunInverseKinematics(_robotArmViewModel.Precision, null!);

        Assert.AreNotEqual(
            _robotArmViewModel.UpperArmSegment.Angle,
            _robotArmViewModel.ForearmSegment.Angle,
            0.0001);
        Assert.IsTrue(_robotArmViewModel.EffectorSegment.PointB.X < 579);
    }
}
