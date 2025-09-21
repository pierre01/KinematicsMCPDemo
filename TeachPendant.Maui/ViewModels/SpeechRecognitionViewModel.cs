using Biosero.TeachPendant.Common.SemanticKernel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace Biosero.TeachPendant.Maui.ViewModels
{
    public partial class SpeechRecognitionViewModel : ObservableObject
    {
        private const int _responseUpdateIntervalMilliSeconds = 200;

        private readonly ISpeechToText _speechToText;

        private CancellationTokenSource _tokenSource;

        private readonly SemanticKernelRecognizer _semanticKernelRecognizer;

        [ObservableProperty]
        private string _userInput;

        [ObservableProperty]
        private string _pluginResponse;

        [ObservableProperty]
        private string _aiResponse;

        public SpeechRecognitionViewModel(ISpeechToText speechToText)
        {
            _speechToText = speechToText;
            _semanticKernelRecognizer = SemanticKernelRecognizer.Instance;

            Task.Run(UpdateResponsesPeriodically);
        }

        private async Task UpdateResponsesPeriodically()
        {
            while (true)
            {
                UpdateResponses();
                await Task.Delay(_responseUpdateIntervalMilliSeconds);
            }
        }

        private void UpdateResponses()
        {
            PluginResponse = _semanticKernelRecognizer.PluginResponse;
            AiResponse = _semanticKernelRecognizer.AiResponse;
        }

        public void SetNewUserInput(string value)
        {
            _semanticKernelRecognizer.SetNewUserInput(value);
        }

        public async Task Listen()
        {
            _tokenSource = new CancellationTokenSource();
            var cancellationToken = _tokenSource.Token;
            try
            {
            var isGranted = await _speechToText.RequestPermissions(cancellationToken);
            if (!isGranted)
            {
                    await Toast.Make("Permission not granted").Show(CancellationToken.None);
                return;
            }

                var recognitionResult = await _speechToText.ListenAsync(
                CultureInfo.GetCultureInfo("en"),
                new Progress<string>(partialText =>
                {
                    UserInput += $"{partialText} ";
                }), cancellationToken);

                if (recognitionResult.IsSuccessful)
                {
                    OnRecognitionTextCompleted(recognitionResult.Text);
                }
                else
                {
                    await Toast.Make(recognitionResult.Exception?.Message ?? "Unable to recognize speech").Show(CancellationToken.None);
                }
            }
            catch (TaskCanceledException)
            {
                _tokenSource.Cancel();
                await StopListening(cancellationToken);
            }
        }

        public async Task StopListening(CancellationToken cancellationToken)
        {
            await SpeechToText.StopListenAsync(cancellationToken);
            SpeechToText.Default.RecognitionResultUpdated -= OnRecognitionTextUpdated;
            SpeechToText.Default.RecognitionResultCompleted -= OnRecognitionTextCompleted;
        }

        public void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
        {
            UserInput += args.RecognitionResult;
        }

        public void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
        {
            OnRecognitionTextCompleted(args.RecognitionResult);
        }

        private void OnRecognitionTextCompleted(string recognizedText)
        {
            UserInput = recognizedText;
            SetNewUserInput(UserInput);
            _tokenSource.Cancel();
        }
    }
}
