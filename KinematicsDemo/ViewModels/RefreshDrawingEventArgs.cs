using KinematicsDemo.Models;
using System;

namespace KinematicsDemo.ViewModels
{
    public class RefreshDrawingEventArgs:EventArgs
    {
        private MetaPoint? _point;
        
        private static RefreshDrawingEventArgs _empty = new RefreshDrawingEventArgs();
        
        public static new RefreshDrawingEventArgs Empty => _empty;

        public RefreshDrawingEventArgs()
        {
            
        }
        
        public RefreshDrawingEventArgs(MetaPoint point)
        {
            Point = point;
        }

        public MetaPoint? Point { get => _point; private set => _point = value; }
    }
}