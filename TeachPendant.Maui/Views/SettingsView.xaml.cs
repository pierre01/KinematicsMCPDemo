using Biosero.TeachPendant.Maui.ViewModels;

namespace Biosero.TeachPendant.Maui.Views;

public partial class SettingsView : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsView()
	{
        _viewModel = new SettingsViewModel();
        BindingContext = _viewModel;
		InitializeComponent();
	}
}