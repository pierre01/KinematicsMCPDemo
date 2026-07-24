using System;

namespace KinematicsDemo.Models;

/// <summary>
/// Configuration Settings for the robot
/// </summary>
[Serializable]
public class RobotConfiguration
{
    /// <summary>
    /// Gets or sets the rail length: 2 meter, 1.5 meter, 1 meter, or fixed position (0)  (Mobile not supported yet)
    /// </summary>
    public double RailLength { get; set; }

    /// <summary>
    /// Gets or sets the mast height: 75 centimeter or 40 centimeter (40 cm is default)
    /// </summary>
    public double MastHeight { get; set; } = 40;

    /// <summary>
    /// Gets or sets 7 to 13 cm (gripper length)
    /// </summary>    
    public double EffectorLength { get; set; } = 10;

    /// <summary>
    /// Gets or sets 77 to 133 mm (gripper minimum Width)
    /// </summary>    
    public double EffectorGripMin { get; set; } = 7.7;

    /// <summary>
    /// Gets or sets 77 to 133 mm (gripper maximum Width)
    /// </summary>    
    public double EffectorGripMax { get; set; } = 13.3;
}
