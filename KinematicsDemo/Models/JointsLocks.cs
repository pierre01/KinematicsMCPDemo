using System;

namespace KinematicsDemo.Models;

/// <summary>
/// The state of the joints locking.
/// </summary>
[Flags]
public enum JointsLocks
{
    None = 0,
    Shoulder = 1,
    Elbow = 2,
    Wrist = 4,
    EffectorGrip = 8,
}