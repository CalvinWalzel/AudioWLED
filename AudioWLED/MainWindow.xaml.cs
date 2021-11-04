using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Net.Http;
using System.Windows;

namespace AudioWLED
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AudioObserver audioObserver;

        public MainWindow()
        {
            InitializeComponent();
            audioObserver = new AudioObserver(new WasapiLoopbackCapture(), new HttpClient());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAudioInterfaceItems();
        }

        private void LoadAudioInterfaceItems()
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();

            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                MMDevice device = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)[i];
                cBoxAudioInterfaces.Items.Add(device.ToString());
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            audioObserver.Start();
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            audioObserver.Stop();
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }
    }
}
