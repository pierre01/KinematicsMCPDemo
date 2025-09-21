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
         
        
       _robotArmViewModel = new RobotArmViewModel(0, heightRange, upperArmSegment, forearmSegment, effectorSegment,
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
}
