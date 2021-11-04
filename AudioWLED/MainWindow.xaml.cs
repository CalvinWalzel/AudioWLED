using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Windows.Media;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace AudioWLED
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowState storedWindowState = WindowState.Normal;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenu;

        private readonly MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        
        private AudioObserver audioObserver;

        public MainWindow()
        {
            InitializeComponent();

            CreateContextMenu();
            CreateNotifyIcon();

            LoadAudioInterfaceItems();

            txtAddress.Text = Properties.Settings.Default.Address;
            chckAutoStart.IsChecked = Properties.Settings.Default.AutoStart;

            if (Properties.Settings.Default.AutoStart)
                handleAudioObserverStart();
        }

        private void LoadAudioInterfaceItems()
        {
            cBoxAudioInterfaces.DisplayMemberPath = "Key";
            cBoxAudioInterfaces.SelectedValuePath = "Value";

            Dictionary<string, string> audioInterfaceOptions = new Dictionary<string, string>();

            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                MMDevice device = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)[i];
                audioInterfaceOptions.Add(device.ToString(), device.ID);
            }

            cBoxAudioInterfaces.ItemsSource = audioInterfaceOptions;

            cBoxAudioInterfaces.SelectedValue = Properties.Settings.Default.AudioInterface;

            if (String.IsNullOrWhiteSpace((String)cBoxAudioInterfaces.SelectedValue))
            {
                cBoxAudioInterfaces.SelectedValue = Properties.Settings.Default.AudioInterface = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Communications).ID;
            }
        }

        private void CreateContextMenu()
        {
            contextMenu = new System.Windows.Forms.ContextMenuStrip();

            contextMenu.Items.Add("&Show", null, ContextMenuShow_Click);
            contextMenu.Items.Add("E&xit", null, ContextMenuExit_Click);
        }

        private void CreateNotifyIcon()
        {
            var iconHandle = Properties.Resources.Logo.GetHicon();

            notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Text = Title,
                Icon = System.Drawing.Icon.FromHandle(iconHandle),
                ContextMenuStrip = contextMenu
            };
            notifyIcon.Click += NotifyIcon_Click;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            handleAudioObserverStart();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            handleAudioObserverStop();
        }

        private async void txtAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            int startLength = txtAddress.Text.Length;

            IPAddress ip;

            await Task.Delay(300);
            if (startLength == txtAddress.Text.Length)
            {
                bool isValidIPAdress = Regex.Matches(txtAddress.Text, "\\.").Count == 3 & IPAddress.TryParse(txtAddress.Text, out ip);

                if (isValidIPAdress)
                    txtAddress.Background = Brushes.LightGreen;
                else
                    txtAddress.Background = Brushes.LightPink;

                Properties.Settings.Default.Address = txtAddress.Text;
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();

            handleAudioObserverStop();

            notifyIcon.Dispose();
            notifyIcon = null;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            else
            {
                storedWindowState = WindowState;
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (notifyIcon == null) return;

            notifyIcon.Visible = !IsVisible;
        }
        private void NotifyIcon_Click(object sender, System.EventArgs e)
        {
            contextMenu.Show(System.Windows.Forms.Control.MousePosition);
        }

        private void ContextMenuShow_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = storedWindowState;
        }

        private void ContextMenuExit_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void cBoxAudioInterfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.AudioInterface = ((KeyValuePair<string, string>)cBoxAudioInterfaces.SelectedItem).Value;
        }

        private void chckAutoStart_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoStart = true;
        }

        private void chckAutoStart_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoStart = false;
        }

        private void handleAudioObserverStart()
        {
            MMDevice audioDevice = enumerator.GetDevice(Properties.Settings.Default.AudioInterface);
            audioObserver = new AudioObserver(new WasapiLoopbackCapture(audioDevice), new HttpClient());

            audioObserver.Start();
            txtAddress.IsEnabled = false;
            cBoxAudioInterfaces.IsEnabled = false;
            chckAutoStart.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        private void handleAudioObserverStop()
        {
            audioObserver.Stop();
            txtAddress.IsEnabled = true;
            cBoxAudioInterfaces.IsEnabled = true;
            chckAutoStart.IsEnabled = true;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }
    }
}
