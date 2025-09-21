using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinematicsDemo.Models;

/// <summary>
/// Recording a series of movements
/// </summary>
[Serializable]
public partial class RobotActionRecording:ObservableObject
{

    public RobotActionRecording(string name, string description)
    {
        _name = name;
        _description = description;
    }

    public RobotActionRecording()
    {
        
    }

    [ObservableProperty]
    string? _name;

    [ObservableProperty]
    string? _description;

    [ObservableProperty]
    ObservableCollection<MetaPoint> _points = new ObservableCollection<MetaPoint>();

    public void Add(MetaPoint metaPoint)
    {
        Points.Add(metaPoint);
    }
}
