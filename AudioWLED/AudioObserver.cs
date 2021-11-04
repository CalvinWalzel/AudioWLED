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

        public AudioObserver(WasapiLoopbackCapture capture, HttpClient client)
        {
            this.capture = capture;
            this.client = client;

            this.capture.DataAvailable += Capture_DataAvailable;
            this.capture.RecordingStopped += Capture_RecordingStopped;
        }

        public void Start()
        {
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

            // TODO: Replace with setting here
            string ip = "127.0.0.1";

            if (Double.IsNegativeInfinity(decibel))
            {
                client.GetStringAsync("http://" + ip + "/win&A=0");
            }
            else
            {
                client.GetStringAsync("http://" + ip + "/win&A=128");
            }
        }

        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (!autoRestart) return;

            capture.StartRecording();
        }
    }
}
