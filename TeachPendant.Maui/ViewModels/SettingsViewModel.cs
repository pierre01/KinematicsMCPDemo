using CommunityToolkit.Mvvm.ComponentModel;

namespace Biosero.TeachPendant.Maui.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _webServiceEndpoint = "http://localhost:7276";
    }
}