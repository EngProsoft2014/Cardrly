using Cardrly.Models.MeetingAiActionRecord;
using Controls.UserDialogs.Maui;
using Foundation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;

namespace Cardrly.Platforms.iOS
{
    public class BackgroundUploader : NSObject, INSUrlSessionDelegate, INSUrlSessionTaskDelegate
    {
        // Dictionary to handle multiple concurrent uploads
        private readonly Dictionary<string, TaskCompletionSource<bool>> _tasks = new();

        // Event to notify MAUI app when upload completes
        public static event Action<string, bool>? UploadCompleted;

        // Singleton pattern
        private static readonly Lazy<BackgroundUploader> _instance = new(() => new BackgroundUploader());
        public static BackgroundUploader Instance => _instance.Value;

        public BackgroundUploader() { }

        /// <summary>
        /// Upload audio file in background using NSURLSession.
        /// </summary>
        public async Task<bool> UploadFileAsync(AudioUploadRequest req, string apiUrl, string token)
        {
            if (!File.Exists(req.AudioPath))
                throw new FileNotFoundException("Audio file does not exist.", req.AudioPath);

            // Use a fixed session identifier per upload to allow resume if app terminates
            var sessionId = $"com.cardrly.upload.{req.AudioUploadId}";
            var config = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(sessionId);
            config.AllowsCellularAccess = true;
            config.WaitsForConnectivity = true;

            var session = NSUrlSession.FromConfiguration(config, this, new NSOperationQueue());

            var request = new NSMutableUrlRequest(new NSUrl(apiUrl))
            {
                HttpMethod = "POST"
            };

            // Metadata headers
            request["Authorization"] = $"Bearer {token}";
            request["X-Audio-Upload-Id"] = req.AudioUploadId ?? "";
            request["X-Audio-Time"] = req.AudioTime ?? "";
            request["X-Audio-Script"] = req.AudioScript ?? "";
            request["X-Audio-Extension"] = req.Extension ?? "";

            if (req.LstMeetingMessage != null)
            {
                string jsonMsgs = JsonConvert.SerializeObject(req.LstMeetingMessage);
                request["X-Audio-Messages"] = jsonMsgs;
            }

            // Track TaskCompletionSource
            var tcs = new TaskCompletionSource<bool>();
            _tasks[req.AudioUploadId] = tcs;

            // Create upload task
            var uploadTask = session.CreateUploadTask(request, NSUrl.FromFilename(req.AudioPath));
            uploadTask.TaskDescription = req.AudioUploadId; // Important for tracking in DidCompleteWithError
            uploadTask.Resume();

            Console.WriteLine($"🚀 Started background upload: {req.AudioUploadId}");

            return await tcs.Task;
        }

        [Export("URLSession:task:didCompleteWithError:")]
        public void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
        {
            string uploadId = task.TaskDescription;

            if (_tasks.TryGetValue(uploadId, out var tcs))
            {
                if (error == null)
                {
                    tcs.TrySetResult(true);
                    UploadCompleted?.Invoke(uploadId, true);

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UserDialogs.Instance.Loading("Uploading... 100%", null, true, MaskType.Clear, null);
                    });
                }
                else
                {
                    tcs.TrySetResult(false);
                    UploadCompleted?.Invoke(uploadId, false);

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UserDialogs.Instance.Loading("❌ Upload failed !!", null, true, MaskType.Clear, null);
                    });
                }

                _tasks.Remove(uploadId);
            }
        }

        [Export("URLSession:task:didSendBodyData:totalBytesSent:totalBytesExpectedToSend:")]
        public void DidSendBodyData(NSUrlSession session, NSUrlSessionTask task, long bytesSent, long totalBytesSent, long totalBytesExpectedToSend)
        {
            double progress = (double)totalBytesSent / totalBytesExpectedToSend;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.Loading(
                    $"📤 Upload progress... ({task.TaskDescription}): {progress:P1}",
                    maskType: MaskType.Clear);
            });
        }


        // Called when all background session events are delivered (iOS resumes app to finish uploads)
        [Export("URLSessionDidFinishEventsForBackgroundURLSession:")]
        public void DidFinishEventsForBackgroundUrlSession(NSUrlSession session)
        {
            Console.WriteLine("🔔 iOS delivered background upload events.");
        }


    }
}
