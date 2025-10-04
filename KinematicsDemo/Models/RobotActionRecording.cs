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
        Name = name;
        Description = description;
    }

    public RobotActionRecording()
    {
        
    }

    [ObservableProperty]
    public partial string? Name{ get;set;}

    [ObservableProperty]
    public partial string? Description { get;set; }

    [ObservableProperty]
    public partial ObservableCollection<MetaPoint> Points { get;set;} = new ObservableCollection<MetaPoint>();

    public void Add(MetaPoint metaPoint)
    {
        Points.Add(metaPoint);
    }
}
