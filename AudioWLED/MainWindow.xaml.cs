using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            audioObserver = new AudioObserver();
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
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            audioObserver.Stop();
        }
    }
}
