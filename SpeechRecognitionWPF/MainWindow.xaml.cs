using Biosero.TeachPendant.Common.SemanticKernel;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SpeechRecognitionWPF
{   // Save To Vault
    // https://westus.api.cognitive.microsoft.com/
    // 3d6697aa650443c1b2b3a813b535c4aa
    //

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WaveIn _waveIn;
        private WaveFileWriter _writer;
        private bool _isRecording = false;
        private bool _isRecordingAI = false;
        private static TaskCompletionSource<int> _stopRecognition;

        public MainWindow()
        {
            InitializeComponent();
            // Create a timer with a 500ms interval.
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            KernerResponsesPanel.DataContext = SemanticKernelRecognizer.Instance;
        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!RecordButton.IsPressed && _isRecording)
            {
                _isRecording = false;
                MicrophoneImage.Fill = Brushes.Black;
                _waveIn.StopRecording();
                _writer.Close();
                _writer.Dispose();
                _waveIn.Dispose();
            }

            if (!RecordButtonAI.IsPressed && _isRecordingAI)
            {
                _isRecordingAI = false;
                MicrophoneImageAI.Fill = Brushes.Black;
                _stopRecognition?.TrySetResult(0);
            }
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRecording)
            {
                // test if the mouse button is up

                return;
            }
            _isRecording = true;
            MicrophoneImage.Fill = Brushes.Red;
            _waveIn = new WaveIn();
            _waveIn.DataAvailable += WaveInOnDataAvailable;
            _waveIn.RecordingStopped += WaveInOnRecordingStopped;
            _waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
            _writer = new WaveFileWriter("test.wav", _waveIn.WaveFormat);
            _waveIn.StartRecording();
        }

        private void WaveInOnRecordingStopped(object? sender, StoppedEventArgs e)
        {

        }

        private void WaveInOnDataAvailable(object? sender, WaveInEventArgs e)
        {
            // write to file
            _writer.Write(e.Buffer, 0, e.BytesRecorded);
        }

        private void PlaySampleClick(object sender, RoutedEventArgs e)
        {
            // Create a new instance of the WaveOut class.
            WaveOut waveOut = new WaveOut();
            // Create a new instance of the AudioFileReader class.
            AudioFileReader audioFileReader = new AudioFileReader("test.wav");
            // Set the audio output device of the WaveOut object to the AudioFileReader object.
            waveOut.Init(audioFileReader);
            // Start playback.
            waveOut.Play();

        }

        public async Task RecognitionWithPushAudioStreamAsync()
        {
            // Create an instance of WaveInEvent
            var waveIn = new WaveIn
            {
                // Set the recording format to float
                WaveFormat = new WaveFormat(16000, 16, 1)
            };

            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").            
            var config = SpeechConfig.FromSubscription("3d6697aa650443c1b2b3a813b535c4aa", "westus");
            
            if(FrenchCheckBox.IsChecked == true)
            {
                config.SpeechRecognitionLanguage = "fr-FR";
            }
            
            _stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Create a push stream
            using var pushStream = AudioInputStream.CreatePushStream();
            using var audioInput = AudioConfig.FromStreamInput(pushStream);
            // Creates a speech recognizer using audio stream input.
            using var recognizer = new SpeechRecognizer(config, audioInput);
            Console.WriteLine("Say something...");

            // Subscribes to events.
            recognizer.Recognizing += (s, e) =>
            {
                Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UnrecognizedTextBox.Text = e.Result.Text;
                });

            };

            recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RecognizedTextBox.Text = e.Result.Text;
                        UnrecognizedTextBox.Text = string.Empty;
                        SemanticKernelRecognizer.Instance.PluginResponse = string.Empty;
                        SemanticKernelRecognizer.Instance.SetNewUserInput(e.Result.Text);
                    });
                    _stopRecognition.TrySetResult(0);
                }
                else if (e.Result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
            };

            recognizer.Canceled += (s, e) =>
            {
                Console.WriteLine($"CANCELED: Reason={e.Reason}");

                if (e.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you update the subscription info?");
                }

                _stopRecognition.TrySetResult(0);
            };

            recognizer.SessionStarted += (s, e) =>
            {
                Console.WriteLine("\nSession started event.");
            };

            recognizer.SessionStopped += (s, e) =>
            {
                Console.WriteLine("\nSession stopped event.");
                Console.WriteLine("\nStop recognition.");
                _stopRecognition.TrySetResult(0);
            };

            waveIn.DataAvailable += (s, e) =>
            {
                if (e.BytesRecorded != 0)
                {
                    pushStream.Write(e.Buffer);
                }
            };

            // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            waveIn.StartRecording();

            // Waits for completion.
            // Use Task.WaitAny to keep the task rooted.
            Task.WaitAny(new[] { _stopRecognition.Task });

            // Stops recognition.
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            waveIn.StopRecording();
        }

        /// <summary>
        /// Repeat button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RecordButtonAI_Click(object sender, RoutedEventArgs e)
        {
            if (_isRecordingAI && _stopRecognition != null && !_stopRecognition.Task.IsCompleted)
            {
                return;
            }
            if (_stopRecognition != null && !_stopRecognition.Task.IsCompleted)
            {
                _stopRecognition.TrySetResult(0);
                return;
            }
            _isRecordingAI = true;
            MicrophoneImageAI.Fill = Brushes.Red;
            await RecognitionWithPushAudioStreamAsync();

        }
    }
}