using Biosero.TeachPendant.Maui.ViewModels;

namespace Biosero.TeachPendant.Maui.Views;

public partial class RecordedPointsView : ContentPage
{
    private readonly RecordedPointsViewModel _viewModel;

    public RecordedPointsView()
	{
		_viewModel = new RecordedPointsViewModel(WebServiceSettings.Url);
        BindingContext = _viewModel;
		InitializeComponent();
	}
}