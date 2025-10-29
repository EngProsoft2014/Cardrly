using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Helpers
{
    public static class FfmpegInitializer
    {
        private static bool _initialized;

        public static async Task EnsureInitializedAsync()
        {
            if (_initialized)
                return;

            var ffmpegPath = Path.Combine(FileSystem.AppDataDirectory, "ffmpeg");
            var ffprobePath = Path.Combine(FileSystem.AppDataDirectory, "ffprobe");

            await CopyIfNotExistsAsync("ffmpeg", ffmpegPath);
            await CopyIfNotExistsAsync("ffprobe", ffprobePath);

            GlobalFFOptions.Configure(new FFOptions
            {
                BinaryFolder = FileSystem.AppDataDirectory,
                TemporaryFilesFolder = Path.GetTempPath()
            });

            _initialized = true;
        }

        private static async Task CopyIfNotExistsAsync(string resourceName, string outputPath)
        {
            if (File.Exists(outputPath))
                return;

            using var resource = await FileSystem.OpenAppPackageFileAsync($"Resources/Raw/{resourceName}");
            using var output = File.Create(outputPath);
            await resource.CopyToAsync(output);
        }
    }
}
