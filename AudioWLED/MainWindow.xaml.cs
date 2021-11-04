using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace AudioWLED
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowState storedWindowState = WindowState.Normal;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;

        private readonly AudioObserver audioObserver;

        public MainWindow()
        {
            InitializeComponent();

            CreateContextMenu();
            LoadNotifyIcon();

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

        private void CreateContextMenu()
        {
            contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add("&Show", null, ContextMenuShow_Click);
            contextMenu.Items.Add("E&xit", null, ContextMenuExit_Click);
        }

        private void LoadNotifyIcon()
        {
            var iconHandle = Properties.Resources.Logo.GetHicon();

            notifyIcon = new NotifyIcon
            {
                Text = Title,
                Icon = System.Drawing.Icon.FromHandle(iconHandle),
                ContextMenuStrip = contextMenu
            };
            notifyIcon.Click += NotifyIcon_Click;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Dispose();
            notifyIcon = null;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            } else
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
            System.Windows.Application.Current.Shutdown();
        }
    }
}
