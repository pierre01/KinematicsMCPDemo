using RobotMcpClient.ViewModels;

namespace RobotMcpClient
{
    public partial class MainPage : ContentPage
    {
        private MainPageViewModel _viewModel;


        public MainPage(MainPageViewModel viewModel)
        {
            _viewModel = viewModel;
            BindingContext = _viewModel;
            InitializeComponent();
        }
    }
}
