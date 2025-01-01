using System.Diagnostics;

namespace Cardrly.Helpers
{
    public static class GlobalExceptionHandler
    {
        public static void RegisterGlobalExceptionHandlers()
        {
            // Handle non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                HandleException(e.ExceptionObject as Exception, "UnhandledException");
            };

            // Handle unobserved task exceptions
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                e.SetObserved(); // Prevent app crash
                HandleException(e.Exception, "UnobservedTaskException");
            };

            // Platform-specific UI thread exceptions
            HandleUIThreadExceptions();
        }

        private static void HandleUIThreadExceptions()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Simulate an exception handler hook (used in other parts of the UI)
                    Application.Current.MainPage?.Dispatcher.Dispatch(() =>
                    {
                        // Simulate a handled exception for testing
                        throw new Exception("Simulated UI Thread Exception");
                    });
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "UIThreadException");
            }
        }

        private static void HandleException(Exception? exception, string source)
        {
            // Log the exception
            Debug.WriteLine($"[{source}] {exception}");

            // Save to local storage for debugging (optional)
            SaveExceptionToLog(exception, source);

            // Reset app state if needed
            ResetAppState();

            // Optionally notify the user
            NotifyUser();
        }

        private static void SaveExceptionToLog(Exception? exception, string source)
        {
            try
            {
                var logFilePath = Path.Combine(FileSystem.AppDataDirectory, "app_errors.log");
                File.AppendAllText(logFilePath, $"{DateTime.Now}: [{source}] {exception}\n");
            }
            catch
            {
                // Suppress any errors while logging
            }
        }

        private static void ResetAppState()
        {
            // Clear preferences or reset state if necessary
            Preferences.Clear();
        }

        private static void NotifyUser()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage?.DisplayAlert(
                    "Unexpected Error",
                    "An unexpected error occurred. Please restart the app or contact support.",
                    "OK"
                );
            });
        }
    }
}
