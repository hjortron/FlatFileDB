using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Timers;
using System.Windows;
using Microsoft.AspNet.SignalR.Client;

namespace DBClient
{
    static class Client
    {
        private static HubConnection Connection { get; set; }
        private static IHubProxy HubProxy { get; set; }
        private static Timer _timer;

        public static async void ConnectAsync()
        {
            Connection = new HubConnection("http://localhost:8080");
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");
            try
            {
                await Connection.Start();
            }
            catch (HttpRequestException)
            {
                WriteMessage("Unable to connect to server: Start server before connecting clients.");
                return;
            }

            WriteMessage("Connected to server\r");
        }

        public static void StartSending()
        {
            _timer = new Timer(60);
            _timer.Elapsed += OnTimedEvent;
            WriteMessage("Start sending records");
            _timer.Start();
        }

        public static void StopSending()
        {
            WriteMessage("Stop sending records");
            _timer.Stop();
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            SendRecord();
        }

        private static void SendRecord()
        {
            var random = new Random();
            const int dataLength = 128;
            var array = new byte[dataLength * random.Next(1, 3)];
            random.NextBytes(array);
            HubProxy.Invoke("WriteRecord", random.Next(1, 1000), random.Next(1, 10), array);
        }

        public static void GetRecords()
        {
            var random = new Random();
            var randomQuery = string.Format("sourceid = {0} AND sourcetype = {1}", random.Next(1, 1000), random.Next(1, 10));
            WriteMessage("Get " + randomQuery);
            HubProxy.Invoke("GetRecords", randomQuery);
            HubProxy.On<List<string>>("Response", message => message.ForEach(item => WriteMessage(string.Format("{0}\r", item))));
        }

        private static void WriteMessage(string message)
        {
            var dispatcher = Application.Current.Dispatcher;
            dispatcher.Invoke(() =>((MainWindow)Application.Current.MainWindow).WriteToConsole(message));
        }

        private static void Connection_Closed()
        {
            WriteMessage("Disconnected!");
        }
    }
}
