
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

            var ffmpegPath = FfmpegPaths.FfmpegPath;
            var ffprobePath = FfmpegPaths.FfprobePath;

            await CopyIfNotExistsAsync("ffmpeg", ffmpegPath);
            await CopyIfNotExistsAsync("ffprobe", ffprobePath);

            //GlobalFFOptions.Configure(new FFOptions
            //{
            //    BinaryFolder = Path.GetDirectoryName(ffmpegPath)!,
            //    TemporaryFilesFolder = FfmpegPaths.TempDirectory,
            //    LogLevel = FFMpegLogLevel.Debug 
            //});

            _initialized = true;
        }

        //        private static async Task CopyIfNotExistsAsync(string resourceName, string outputPath)
        //        {
        //            if (File.Exists(outputPath))
        //                return;

        //            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        //#if ANDROID
        //            //using var asset = await FileSystem.OpenAppPackageFileAsync(resourceName); // reads from Assets
        //            using var stream = Android.App.Application.Context.Resources!.OpenRawResource(
        //                resourceName == "ffmpeg" ? Resource.Raw.ffmpeg : Resource.Raw.ffprobe);

        //            using var output = File.Create(outputPath);
        //            await stream.CopyToAsync(output);
        //#elif IOS
        //            var bundlePath = Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, resourceName); // reads from Resources
        //            using var stream = File.OpenRead(bundlePath);
        //            using var output = File.Create(outputPath);
        //            await stream.CopyToAsync(output);
        //#endif
        //        }

        private static async Task CopyIfNotExistsAsync(string resourceName, string outputPath)
        {
#if ANDROID
            try
            {
                var context = Android.App.Application.Context;

                using var asset = context.Assets.Open(resourceName);
                var ffmpegPath = Path.Combine(context.CacheDir.AbsolutePath, resourceName);
                Directory.CreateDirectory(Path.GetDirectoryName(ffmpegPath)!);
                using var output = File.Create(ffmpegPath);
                await asset.CopyToAsync(output);

                // Make executable (required on Android 10+)
                Java.Lang.Runtime.GetRuntime().Exec($"chmod 755 {outputPath}");

                await Task.Delay(50);

                Android.Util.Log.Info(resourceName.ToUpper(), $"Copied & chmod OK: {outputPath}");
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(resourceName.ToUpper(), $"Copy error: {ex}");
            }

#elif IOS
            var bundlePath = Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, resourceName);
            using var stream = File.OpenRead(bundlePath);
            using var output = File.Create(outputPath);
            await stream.CopyToAsync(output);
#endif
        }

    }

}
