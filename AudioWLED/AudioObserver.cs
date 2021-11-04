using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Net.Http;

namespace AudioWLED
{
    class AudioObserver
    {
        private readonly WasapiLoopbackCapture capture;
        private readonly HttpClient client;
        private bool autoRestart;
        private string ip;
        private bool lastStateActive;

        public AudioObserver(WasapiLoopbackCapture capture, HttpClient client)
        {
            this.capture = capture;
            this.client = client;

            this.capture.DataAvailable += Capture_DataAvailable;
            this.capture.RecordingStopped += Capture_RecordingStopped;
        }

        public void Start()
        {
            lastStateActive = false;
            ip = Properties.Settings.Default.Address;

            autoRestart = true;
            capture.StartRecording();
        }

        public void Stop()
        {
            autoRestart = false;
            capture.StopRecording();
        }
        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;

            double sum = 0;
            for (int i = 0; i < buffer.Length; i += 2)
            {
                double sample = BitConverter.ToInt16(buffer, i) / 3276.0;
                sum += sample * sample;
            }
            double rms = Math.Sqrt(sum / (buffer.Length / 2));
            double decibel = 20 * Math.Log10(rms);

            bool stateActive = !Double.IsNegativeInfinity(decibel);

            if (stateActive != lastStateActive)
            {
                if (stateActive)
                {
                    client.GetStringAsync("http://" + ip + "/win&A=128");
                } else
                {
                    client.GetStringAsync("http://" + ip + "/win&A=0");
                }
            }

            lastStateActive = stateActive;
        }

        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (!autoRestart) return;

            capture.StartRecording();
        }
    }
}
