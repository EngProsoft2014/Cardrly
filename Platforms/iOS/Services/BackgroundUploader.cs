using Foundation;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Cardrly.Platforms.iOS
{
    public class BackgroundUploader : NSObject, INSUrlSessionDelegate, INSUrlSessionTaskDelegate
    {
        public async Task UploadFileAsync(string filePath, string apiUrl, string token)
        {
            var config = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration("com.fixprous.upload");
            var session = NSUrlSession.FromConfiguration(config, this, new NSOperationQueue());

            var request = new NSMutableUrlRequest(new NSUrl(apiUrl))
            {
                HttpMethod = "POST"
            };

            request["Authorization"] = $"Bearer {token}";

            var uploadTask = session.CreateUploadTask(request, NSUrl.FromFilename(filePath));
            uploadTask.Resume();

            Console.WriteLine("🚀 Started background upload (iOS).");
        }

        [Export("URLSession:task:didCompleteWithError:")]
        public void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
        {
            if (error == null)
                Console.WriteLine("✅ Upload completed successfully (iOS).");
            else
                Console.WriteLine($"❌ Upload failed: {error.LocalizedDescription}");
        }
    }
}
