
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

        public event Action<string, string> OnMessageReceived;

        public SignalRService(ServicesService service)
        {
            _service = service;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Helpers.Utility.ServerUrl + "authHub", options =>
                {
                    options.AccessTokenProvider = async () => await GetAccessTokenAsync();
                    options.Transports = HttpTransportType.LongPolling; // You can choose WebSockets or other transports
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string, string>("ForceLogOut", (userId, email) =>
            {
                OnMessageReceived?.Invoke(userId, email);
            });
        }

        private async Task<string> GetAccessTokenAsync()
        {
            // Fetch token from secure storage or authentication provider
            return await _service.UserToken();
        }

        public async Task StartAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("SignalR connected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR connection failed: {ex.Message}");
            }
        }

        public async Task Disconnect()
        {
            try
            {
                await _hubConnection.StopAsync();
                Console.WriteLine("SignalR disconnected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR disconnection failed: {ex.Message}");
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
