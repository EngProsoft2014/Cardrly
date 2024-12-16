using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace TripBliss.Services.Data
{
    public class SignalRService
    {
        private readonly HubConnection _hubConnection;
        private readonly IHubProxy _hubProxy;
        public event Action<string, string,string ,string> OnMessageReceived;

        public SignalRService()
        {

            _hubConnection = new HubConnection("https://fixproapi.engprosoft.net/");
            _hubProxy = _hubConnection.CreateHubProxy("ChatHub");

            _hubProxy.On<string, string,string,string>("ReceiveMessage", (user, message, userFrom, userTo) =>
                {
                    // Handle received message
                    OnMessageReceived?.Invoke(user, message, userFrom, userTo);
                });
        }

        public async Task StartAsync()
        {
            await _hubConnection.Start();
        }

        public async Task Disconnect()
        {
             _hubConnection.Stop();
        }

        public async Task SendMessage(string user, string message)
        {
            await _hubProxy.Invoke("SendMessage", user, message);
        }
    }
}
