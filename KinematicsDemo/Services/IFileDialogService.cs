using KinematicsDemo.Models;
using System.Collections.Generic;

namespace KinematicsDemo.Services;

/// <summary>
/// Load and save recorded points array to a local file
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// Path to the file that was picked, returned by the dialog
    /// </summary>
    string FilePath { get; set; }

    /// <summary>
    /// File extensions filter for the dialog
    /// </summary>
    string Filter { get; set; }

    /// <summary>
    /// Title of the dialog
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Initial directory of the dialog
    /// </summary>
    string InitialDirectory { get; set; }

    /// <summary>
    /// Load recorded points array from a local file
    /// </summary>
    /// <returns>recorded Metapoints or empty</returns>
    RobotActionRecording LoadMetaPointsFromFile();

    /// <summary>
    /// Save recorded points array to a local file
    /// </summary>e
    /// <param name="recordedPoints"></param>
    /// <returns>true if saved sucessfiully</returns>
    bool SaveMetaPointsToFile(RobotActionRecording recording);
}

