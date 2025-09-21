using Biosero.TeachPendant.Maui.ViewModels;

namespace Biosero.TeachPendant.Maui.Views;

public partial class TeachPendantView : ContentPage
{
    private readonly TeachPendantViewModel _viewModel;

    public TeachPendantView()
    {
        _viewModel = new TeachPendantViewModel(WebServiceSettings.Url);
        BindingContext = _viewModel;
        InitializeComponent();
	}

    private void Move_Button_Released(object sender, EventArgs e)
    {
        _viewModel.MoveToBufferedCoordinates();
    }

    private void Record_Button_Pressed(object sender, EventArgs e)
    {
        _viewModel.RecordPoint();
    }

    private void GoNorth_Button_Pressed(object sender, EventArgs e)
    {
        _viewModel.GoNorth();
    }

    private void GoSouth_Button_Pressed(object sender, EventArgs e)
    {
        _viewModel.GoSouth();
    }

    private void GoWest_Button_Pressed(object sender, EventArgs e)
    {
        _viewModel.GoWest();
    }

    private void GoEast_Button_Pressed(object sender, EventArgs e)
    {
        _viewModel.GoEast();
    }

    private void GoUp_Button_Pressed(object sender, EventArgs e)
    {
        _viewModel.GoUp();
    }

    private void GoDown_Button_Pressed(object sender, EventArgs e)
    {
        _viewModel.GoDown();
    }

    private void GoForward_Button_Pressed(object sender, EventArgs e)
    {
        _viewModel.GoForward();
    }

    private void GoBackward_Button_Pressed(object sender, EventArgs e)
    {
        _viewModel.GoBackward();
    }
}

