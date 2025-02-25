
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

        public event Action<string, string> OnMessageReceived;

        public SignalRService(ServicesService service)
        {
            _service = service;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Helpers.Utility.ServerUrl + "authHub",options =>
                {
                    options.AccessTokenProvider = async () => await _service.UserToken();
                    options.Transports = HttpTransportType.WebSockets; // You can choose WebSockets or other transports
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.Closed += async (error) =>
            {
                Console.WriteLine("SignalR Disconnected. Retrying in 2 seconds...");
                await Task.Delay(2000);
                await StartAsync();
            };

            _hubConnection.On<string, string>("ForceLogOut", (userId, email) =>
            {
                OnMessageReceived?.Invoke(userId, email);
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
