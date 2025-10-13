using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models
{
    public partial class AudioItem : ObservableObject
    {
        public IAudioSource AudioSource { get; set; }

        [ObservableProperty] bool isPlaying;
        [ObservableProperty] string duration;
        [ObservableProperty] string recordTime;

        public IAudioPlayer? Player { get; set; }

        CancellationTokenSource? cts;

        //private static readonly Random _random = new();

        //public static ObservableCollection<int> SharedWaveform { get; } =
        //    new(CreateWaveform(50));

        //private static IEnumerable<int> CreateWaveform(int count)
        //{
        //    for (int i = 0; i < count; i++)
        //        yield return _random.Next(5, 30);
        //}

        //public ObservableCollection<int> Waveform => SharedWaveform;

        //public ObservableCollection<int> Waveform { get; } = new(Enumerable.Range(0, 50).Select(_ => new Random().Next(5, 30)));


        //public void StartWaveAnimation()
        //{
        //    cts?.Cancel();
        //    cts = new CancellationTokenSource();

        //    Task.Run(async () =>
        //    {
        //        var rnd = new Random();
        //        while (!cts.IsCancellationRequested)
        //        {
        //            MainThread.BeginInvokeOnMainThread(() =>
        //            {
        //                for (int i = 0; i < Waveform.Count; i++)
        //                    Waveform[i] = rnd.Next(5, 40);
        //            });
        //            await Task.Delay(100);
        //        }
        //    });
        //}

        //public void StopWaveAnimation()
        //{
        //    cts?.Cancel();
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        for (int i = 0; i < Waveform.Count; i++)
        //            Waveform[i] = 5;
        //    });
        //}


    }
}
