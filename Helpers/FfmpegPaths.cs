using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Helpers
{
    public static class FfmpegPaths
    {
        public static string FfmpegPath
        {
            get
            {
#if ANDROID
                return Path.Combine(Android.App.Application.Context.CacheDir!.AbsolutePath, "ffmpeg");
#elif IOS
            return Path.Combine(FileSystem.CacheDirectory, "ffmpeg");
#else
            return Path.Combine(FileSystem.AppDataDirectory, "ffmpeg");
#endif
            }
        }

        public static string FfprobePath
        {
            get
            {
#if ANDROID
                return Path.Combine(Android.App.Application.Context.CacheDir!.AbsolutePath, "ffprobe");
#elif IOS
            return Path.Combine(FileSystem.CacheDirectory, "ffprobe");
#else
            return Path.Combine(FileSystem.AppDataDirectory, "ffprobe");
#endif
            }
        }

        public static string TempDirectory
        {
            get
            {
#if ANDROID
                return Android.App.Application.Context.CacheDir!.AbsolutePath;
#elif IOS
            return FileSystem.CacheDirectory;
#else
            return Path.GetTempPath();
#endif
            }
        }
    }

}
