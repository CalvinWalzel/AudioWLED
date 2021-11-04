using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Net.Http;
using System.Threading;

namespace AudioWLED
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        // add settings persistence
        // add state changed
        // minimize to system tray
        // https://www.codeproject.com/Questions/354327/Minimizing-Console-Window-to-System-Tray

        static void Main(string[] args)
        {
            var defaultIp = "10.0.0.74";
            Console.WriteLine("Enter an IP (default: {0}):", defaultIp);
            var ip = Console.ReadLine();

            if (String.IsNullOrWhiteSpace(ip)) ip = defaultIp;

            var enumerator = new MMDeviceEnumerator();
            // cycle trough all audio devices
            for (int i = 0; i < WaveOut.DeviceCount; i++)
                Console.WriteLine("[{0}] {1}", i, enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)[i]);
            var index = Convert.ToInt32(Console.ReadLine());

            var device = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)[index];

            enumerator.Dispose();

            var capture = new WasapiLoopbackCapture(device);

            capture.DataAvailable += (object sender, WaveInEventArgs a) =>
            {
                var _buffer = a.Buffer;

                double sum = 0;
                for (var i = 0; i < _buffer.Length; i = i + 2)
                {
                    double sample = BitConverter.ToInt16(_buffer, i) / 3276.0;
                    sum += (sample * sample);
                }
                double rms = Math.Sqrt(sum / (_buffer.Length / 2));
                var decibel = 20 * Math.Log10(rms);

                Console.WriteLine(decibel);

                if (Double.IsNegativeInfinity(decibel))
                {
                    Console.WriteLine("Silence");

                    client.GetStringAsync("http://" + ip + "/win&A=0");
                } else
                {
                    Console.WriteLine("Party!");

                    client.GetStringAsync("http://" + ip + "/win&A=128");
                }
            };

            capture.RecordingStopped += (object sender, StoppedEventArgs e) =>
            {
                capture.StartRecording();
            };

            capture.StartRecording();

            while (true)
            {
                Thread.Sleep(500);
            }
        }

    }
}
