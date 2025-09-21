using KinematicsDemo.Models;
using KinematicsDemo.Services.MessageBoxService;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;

namespace KinematicsDemo.Services;

/// <summary>
/// Load and save recorded points array to a local file
/// </summary>
public class FileDialog : IFileDialogService
{
    public string FilePath { get; set; } = "";

    public string Filter { get; set; } = "";

    public string Title { get; set; } = "";

    public string InitialDirectory { get; set; } = "";

    IMessageBoxService _messageBoxService;

    public FileDialog(IMessageBoxService messageBoxService)
    {
        _messageBoxService = messageBoxService;
    }

    /// <summary>
    /// Load recorded points array from a local file
    /// </summary>
    /// <returns></returns>
    public RobotActionRecording LoadMetaPointsFromFile()
    {
        // Load _recordedPoints array from a local file
        OpenFileDialog openFileDialog = new OpenFileDialog()
        {
            Filter = Filter,
            Title = Title,
            InitialDirectory = InitialDirectory
        };
        try
        {
            if (openFileDialog.ShowDialog() == true)
            {
                string loadPath = openFileDialog.FileName;
                FilePath = loadPath;
                using (StreamReader sr = new StreamReader(loadPath))
                {
                    string json = sr.ReadToEnd();
                    if (string.IsNullOrEmpty(json)) return new RobotActionRecording();
                    var recording = JsonConvert.DeserializeObject<RobotActionRecording>(json);
                    return recording == null ? new RobotActionRecording() : recording;
                }
            }
        }
        catch (Exception ex)
        {
            _messageBoxService.Show("Exception Occurred While Loading the File :" + ex.Message);
        }

        return new RobotActionRecording();
    }

    /// <summary>
    /// Save recorded points array to a local file
    /// </summary>
    /// <param name="recordedAction">Recordings of points plus metadata</param>
    /// <returns>true if points saved</returns>
    public bool SaveMetaPointsToFile(RobotActionRecording recordedAction)
    {
        // Save _recordedPoints array to a local file
        // Open file selector
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = Filter,
            Title = Title,
            InitialDirectory = InitialDirectory
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            string savePath = saveFileDialog.FileName;

            // Save _recordedPoints array to a local file 
            string json = JsonConvert.SerializeObject(recordedAction);

            using (StreamWriter sw = new StreamWriter(savePath))
            {
                try
                {
                    sw.Write(json);
                    FilePath = savePath;
                    return true;
                }
                catch (Exception ex)
                {

                    _messageBoxService.Show("Exception Occured While Loading the File :" + ex.Message);
                }
            }

        }

        return false;
    }

}
