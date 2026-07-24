// <copyright file="App.xaml.cs" company="BioRobot">
// Copyright (c) BioRobot. All rights reserved.
// </copyright>

using System;
using System.Windows;
using KinematicsDemo.Models;
using KinematicsDemo.Services;
using KinematicsDemo.Services.MessageBoxService;
using KinematicsDemo.Services.ToastService;
using KinematicsDemo.ViewModels;
using KinematicsDemo.Views;
using Microsoft.Extensions.DependencyInjection;

namespace KinematicsDemo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class and configures application services.
    /// </summary>
    /// <remarks>This constructor sets up the application's service dependencies and performs component
    /// initialization. It should be called once when the application starts.</remarks>
    public App()
    {
        Services = ConfigureServices();

        this.InitializeComponent();
    }

    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IToastService>(ToastService.Instance);
        services.AddTransient<IFileDialogService, FileDialog>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();
        services.AddSingleton<IToolWindowService, ToolWindowService>();
        services.AddSingleton<IMCPServer, McpService>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Handles application startup logic, including processing command-line arguments and initializing the main window
    /// and core services.
    /// </summary>
    /// <remarks>If a configuration file is specified as a command-line argument, the application loads robot
    /// parameters from that file. Otherwise, default robot configuration values are used. This method also initializes
    /// essential services and starts the MCP web server before displaying the main window.</remarks>
    /// <param name="e">An object that contains the event data for the startup event, including command-line arguments.</param>
    /// <exception cref="ArgumentNullException">Thrown if a required service cannot be resolved during startup.</exception>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Get the command-line arguments as an array of strings
        string[] args = Environment.GetCommandLineArgs();

        // TODO: Process the command-line arguments as needed
        // args[0] is the full path of the executable
        // args[1] is the first command-line argument
        // TODO: if the second argument is not null, then load the file containing the robot configuration
        // The file should contain Mast Height, Rail Length, and Effector length
        if (args.Length > 1)
        {
            // Read the configuration file - Create View Model
            //MainWindow = new MainWindow(robotArmViewModel);
        }
        else
        {
            // Mast freedom 400 mm standard, 750 mm or 1160 mm options available (0 is at bottom)
            // Rail Option Goes 1, Meter, 1.5 Meter or 2 Meter (0 is in Center
            var upperArmSegment = new Segment(0, 0, 225, 0, -90, 90);
            var forearmSegment = new Segment(upperArmSegment, 210, 0, -167, 167);
            var effectorSegment = new Segment(forearmSegment, 144, 0, -970, 970);
            var heightRange = new KRange(0, 400); // 40 cm  mast
            var railRange = new KRange(-500, 500); // 1 meter rail - if no rail , then 0 (Origin is at center of Rail)
            IMessageBoxService messageBoxService = Services.GetService<IMessageBoxService>() ?? throw new ArgumentNullException(nameof(messageBoxService));
            IFileDialogService fileDialogService = Services.GetService<IFileDialogService>() ?? throw new ArgumentNullException(nameof(fileDialogService));
            IToastService toastService = Services.GetService<IToastService>() ?? throw new ArgumentNullException(nameof(toastService));
            IToolWindowService toolWindowService = Services.GetService<IToolWindowService>() ?? throw new ArgumentNullException(nameof(toolWindowService));
            RobotArmViewModel robotArmViewModel = new RobotArmViewModel(
                0, heightRange,0,railRange, upperArmSegment, forearmSegment, effectorSegment, messageBoxService, fileDialogService, toastService, toolWindowService);
            MainWindow = new RobotWindow(robotArmViewModel);
            RobotMcpTool.Robot = robotArmViewModel;

            // start the MCP web server
            IMCPServer webServerService = Services.GetService<IMCPServer>() ?? throw new ArgumentNullException(nameof(webServerService));
            webServerService.StartAsync(string.Empty).ConfigureAwait(false);
        }

        // Display the main window
        MainWindow.Show();
    }

}