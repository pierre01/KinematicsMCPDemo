using System;
using System.Windows;
using System.Windows.Media.Animation;
using KinematicsDemo.ViewModels;

namespace KinematicsDemo.Views
{
    /// <summary>
    /// Interaction logic for TeachPendantWindow.xaml
    /// </summary>
    public partial class TeachPendantWindow : Window
    {
        private Storyboard _animateSignal;
        private TeachPendantViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeachPendantWindow"/> class.
        /// </summary>
        /// <param name="pendantViewModel">ViewMode for the teach Pendant</param>
        public TeachPendantWindow(TeachPendantViewModel pendantViewModel)
        {
            InitializeComponent();
            _viewModel = pendantViewModel;
            DataContext = _viewModel;
            _animateSignal = (Storyboard)FindResource("AnimateSignal");
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void ToggleServerButton_Checked(object sender, RoutedEventArgs e)
        {
            _animateSignal.Begin();
        }

        private void ToggleServerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            _animateSignal.Pause();
            _animateSignal.Seek(TimeSpan.Zero);
        }
    }
}
