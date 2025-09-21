using Biosero.TeachPendant.Maui.ViewModels;
using CommunityToolkit.Maui.Media;

namespace Biosero.TeachPendant.Maui.Views;

public partial class SpeechRecognitionView : ContentPage
{
    private readonly SpeechRecognitionViewModel _viewModel;

    public SpeechRecognitionView() : this(SpeechToText.Default)
    {
    }

    public SpeechRecognitionView(ISpeechToText speechToText)
    {
        _viewModel = new SpeechRecognitionViewModel(speechToText);
        BindingContext = _viewModel;
        InitializeComponent(); 
    }

    private void UserInput_Changed(object sender, TextChangedEventArgs e)
    {
        _viewModel.SetNewUserInput(e.NewTextValue);
    }

    private async void OnStartListening(object sender, EventArgs e)
    {
        SpeakButton.IsEnabled = false;
        try
        {
            await _viewModel.Listen();
    }
        finally
    {
            UserInput_Editor.Text = _viewModel.UserInput;
            SpeakButton.IsEnabled = true;
        }
    }
}