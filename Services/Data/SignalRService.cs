
using Cardrly.Constants;
using Cardrly.Controls;
using Cardrly.Models;
using CommunityToolkit.Maui.Alerts;
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

        public async Task SendEmployeeLocation(DataMapsModel locationData)
        {
            try
            {
                await _hubConnection.InvokeAsync("SendEmployeeLocation", locationData);
                Console.WriteLine($"📡 Sent location for employee {locationData.EmployeeId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send employee location: {ex.Message}");
            }
        }

        // 👇 New method: start listening for geolocation updates
        public async Task StartLocationTrackingAsync(string employeeId)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                await Toast.Make("Location permission not granted", CommunityToolkit.Maui.Core.ToastDuration.Long, 15).Show();

            var request = new GeolocationListeningRequest(
                GeolocationAccuracy.High,
                TimeSpan.FromSeconds(3) // update interval
            );

            if (await Geolocation.StartListeningForegroundAsync(request))
            {
                Geolocation.LocationChanged += async (s, e) =>
                {
                    var loc = e.Location;
                    if (loc != null)
                    {
                        var data = new DataMapsModel
                        {
                            EmployeeId = employeeId,
                            Lat = loc.Latitude.ToString(),
                            Long = loc.Longitude.ToString(),
                            CreateDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                            Time = DateTime.UtcNow.ToString("HH:mm:ss")
                        };

                        await SendEmployeeLocation(data);
                    }
                };

                Geolocation.ListeningFailed += (s, e) =>
                {
                    Console.WriteLine($"⚠️ Location listening failed: {e.Error}");
                };
            }
        }


        //public async Task StartLocationTrackingAsync(string employeeId)
        //    {
        //        // Simulate 100 updates
        //        for (int i = 0; i < 100; i++)
        //        {
        //            var fakeLat = 30.0444 + (i * 0.0002); // Cairo base + small offset
        //            var fakeLong = 31.2357 + (i * 0.0003);

        //            var data = new DataMapsModel
        //            {
        //                EmployeeId = employeeId,
        //                Lat = fakeLat.ToString("F6"),
        //                Long = fakeLong.ToString("F6"),
        //                CreateDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
        //                Time = DateTime.UtcNow.ToString("HH:mm:ss")
        //            };

        //            await SendEmployeeLocation(data);

        //            Console.WriteLine($"📡 Fake location {i}: {data.Lat}, {data.Long}");

        //            await Task.Delay(3000); // wait 3 seconds before next update
        //        }
        //    }


        public void StopLocationTracking()
        {
            Geolocation.StopListeningForeground();
        }

    }

}
