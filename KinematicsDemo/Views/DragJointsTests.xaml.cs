using Microsoft.Xaml.Behaviors.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KinematicsDemo.Views
{
    /// <summary>
    /// Interaction logic for DragJointsTests.xaml
    /// </summary>
    public partial class DragJointsTests : Window
    {
        public DragJointsTests()
        {
            InitializeComponent();
        }

        private void StraightJoint_Dragging(object sender, MouseEventArgs e)
        {

        }

        private void StraightJoint_DragFinished(object sender, MouseEventArgs e)
        {
            var dragBehavior = (MouseDragElementBehavior)sender;
            //  e.GetPosition
            XDragTextBox.Text= $"x drag: {dragBehavior.X}";
            YDragTextBox.Text= $"Y drag: {dragBehavior.Y}";
        }

        private void VerticalJoint_Dragging(object sender, MouseEventArgs e)
        {
            var dragBehavior = (MouseDragElementBehavior)sender;            
            XDragTextBox.Text= $"XEllipse: {Canvas.GetLeft(RailDragEllipse)}";
            YDragTextBox.Text= $"YEllipse: {Canvas.GetTop(RailDragEllipse)}";
            var y = dragBehavior.Y - Canvas.GetTop(RailCanvas);
            Canvas.SetTop(ArmCanvas,y);
        }

        private void VerticalJoint_DragFinished(object sender, MouseEventArgs e)
        {
            var dragBehavior = (MouseDragElementBehavior)sender;

            //  e.GetPosition
            //XDragTextBox.Text= $"x drag: {dragBehavior.X}";
            //YDragTextBox.Text= $"Y drag: {dragBehavior.Y}";
            XDragTextBox.Text= $"XEllipse: {Canvas.GetLeft(RailDragEllipse)}";
            YDragTextBox.Text= $"YEllipse: {Canvas.GetTop(RailDragEllipse)}";
        }
    }
}
