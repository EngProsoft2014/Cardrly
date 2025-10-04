
using Cardrly.Constants;
using Cardrly.Models;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace Cardrly.Services.Data
{
    public class SignalRService
    {

        private readonly HubConnection _hubConnection;
        readonly Services.Data.ServicesService _service;
        private bool _isReconnecting = false;

        public event Action<string> OnMessageReceivedLogout;
        public event Action<string, string, string, string, string, string, string> OnMessageReceivedUpdateVersion;
        public event Action<DataMapsModel> OnMessageReceivedLocation;

        public SignalRService(ServicesService service)
        {
            _service = service;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Helpers.Utility.ServerUrl + "authHub",options =>
                {
                    options.AccessTokenProvider = async () => await _service.UserToken();
                    options.Transports = HttpTransportType.WebSockets; // You can choose WebSockets or other transports

                    // Add custom headers
                    options.Headers["DeviceType"] = "Mobile";
                    options.Headers["OS"] = DeviceInfo.Platform.ToString();
                    options.Headers["CurrentVersion"] = AppInfo.VersionString;
                    options.Headers["BuildVersion"] = AppInfo.BuildString;
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.Closed += async (error) =>
            {
                Console.WriteLine("SignalR Disconnected. Retrying in 2 seconds...");
                await Task.Delay(2000);
                await StartAsync();
            };

            _hubConnection.On<string>("ForceLogOut", (GuidKey) =>
            {
                string Id = Preferences.Default.Get(ApiConstants.GuidKey, "");

                if (!string.IsNullOrEmpty(Id) && GuidKey == Id)
                    OnMessageReceivedLogout?.Invoke(GuidKey);
            });

            _hubConnection.On<string,string, string, string, string, string, string>("UpdateVersion", (GuidKey, Name, VersionNumber, VersionBuild, DescriptionEN, DescriptionAR, ReleaseDate) =>
            {
                string Id = Preferences.Default.Get(ApiConstants.GuidKey, "");

                if(!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(GuidKey) && !string.IsNullOrEmpty(Id) && GuidKey == Id)
                {
                    int VersionNumberParse = int.Parse(VersionNumber.Trim().Replace(".", ""));
                    int VersionBuildParse = int.Parse(VersionBuild.Trim().Replace(".", ""));

                    int currentVersionParse = int.Parse(AppInfo.VersionString.Replace(".", ""));
                    int currentBuildParse = int.Parse(AppInfo.BuildString.Replace(".", ""));

                    if ((Name.ToLower() == "android" && DeviceInfo.Platform == DevicePlatform.Android) && (currentBuildParse < VersionBuildParse))
                    {
                        OnMessageReceivedUpdateVersion?.Invoke(GuidKey, Name, VersionNumber, VersionBuild, DescriptionEN, DescriptionAR, ReleaseDate);
                    }
                    else if((Name.ToLower() == "ios" && DeviceInfo.Platform == DevicePlatform.iOS) && (currentVersionParse < VersionNumberParse))
                    {
                        OnMessageReceivedUpdateVersion?.Invoke(GuidKey, Name, VersionNumber, VersionBuild, DescriptionEN, DescriptionAR, ReleaseDate);
                    }
                }
            });

            _hubConnection.On<DataMapsModel>("ReceiveLocation", (locationData) =>
            {
                OnMessageReceivedLocation?.Invoke(locationData);
            });
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public async Task StartAsync()
        {
            if (_hubConnection.State == HubConnectionState.Connected || _isReconnecting)
            {
                Console.WriteLine("SignalR is already connected or reconnecting.");
                return;
            }

            _isReconnecting = true;

            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("✅ SignalR Connected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SignalR Connection Failed: {ex.Message}");
            }
            finally
            {
                _isReconnecting = false;
            }
        }

        public async Task InvokeNotifyDisconnectyAsync(string guidKey)
        {
            try
            {
                await _hubConnection.InvokeAsync("NotifyDisconnect", guidKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SignalR Connection Failed: {ex.Message}");
            }
            finally
            {
                _isReconnecting = false;
            }
        }

        public async Task NotifyUpdatedVersionMobile(string guidKey)
        {
            try
            {
                await _hubConnection.InvokeAsync("NotifyUpdatedVersionMobile", guidKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SignalR Connection Failed: {ex.Message}");
            }
            finally
            {
                _isReconnecting = false;
            }
        }

        public async Task Disconnect()
        {
            try
            {
                if (_hubConnection != null)
                {
                    await _hubConnection.StopAsync();
                    await _hubConnection.DisposeAsync();
                    Console.WriteLine("🔴 SignalR Disconnected.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ SignalR Disconnection Failed: {ex.Message}");
            }
        }

        public async Task SendMessage(string user, string message)
        {
            try
            {
                await _hubConnection.InvokeAsync("SendMessage", user, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage failed: {ex.Message}");
            }
        }
    }

}
