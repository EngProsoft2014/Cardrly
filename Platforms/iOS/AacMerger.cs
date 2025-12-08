#if IOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AVFoundation;
using CoreMedia;
using Foundation;

namespace Cardrly.Platforms.iOS
{
    public static class AacMerger
    {
        public static async Task<bool> MergeAsync(string outputPath, List<string> files)
        {
            if (files == null || files.Count == 0)
                return false;

            try
            {
                var composition = new AVMutableComposition();

                // Create a mutable audio track
                var audioTrack = composition.AddMutableTrack(
                    new NSString("soun"), // Audio type
                    0                     // Track ID
                );

                if (audioTrack == null)
                    return false;

                CMTime currentTime = CMTime.Zero;

                foreach (var file in files)
                {
                    var url = NSUrl.FromFilename(file);
                    var asset = AVUrlAsset.FromUrl(url);

                    if (asset == null)
                        continue;

                    var track = asset.Tracks?.FirstOrDefault();
                    if (track == null)
                        continue;

                    // Manually create time range
                    var timeRange = new CMTimeRange
                    {
                        Start = CMTime.Zero,
                        Duration = asset.Duration
                    };

                    NSError insertError;
                    audioTrack.InsertTimeRange(timeRange, track, currentTime, out insertError);

                    if (insertError != null)
                    {
                        Console.WriteLine($"InsertTimeRange Error: {insertError.LocalizedDescription}");
                        return false;
                    }

                    currentTime = CMTime.Add(currentTime, asset.Duration);
                }

                // Export the merged M4A
                var exporter = new AVAssetExportSession(composition, AVAssetExportSession.PresetAppleM4A)
                {
                    OutputUrl = NSUrl.FromFilename(outputPath),
                    OutputFileType = new NSString("com.apple.m4a-audio")
                };

                await exporter.ExportTaskAsync();

                return exporter.Status == AVAssetExportSessionStatus.Completed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AAC Merge Exception: {ex.Message}");
                return false;
            }
        }
    }
}
#endif
