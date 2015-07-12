using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using System.Net.Http;
using System.Timers;

namespace DBClient
{
    class Client
    {
        public static HubConnection Connection { get; set; }
        public static IHubProxy HubProxy { get; set; }
        private static System.Timers.Timer timer;

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
            timer = new System.Timers.Timer(60);
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            WriteMessage("Start sending records");
            timer.Start();
        }

        public static void StopSending()
        {
            WriteMessage("Stop sending records");
            timer.Stop();
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            SendRecord();
        }

        public static void SendRecord()
        {
            var random = new Random();
            var dataLength = 128;
            byte[] array = new byte[dataLength * random.Next(1, 3)];
            random.NextBytes(array);
            HubProxy.Invoke("WriteRecord", random.Next(1, 1000), random.Next(1, 10), array);
        }

        public static void GetRecords()
        {
            var random = new Random();
            var randomQuery = String.Format("sourceid = {0} AND sourcetype = {1}", random.Next(1, 1000), random.Next(1, 10));
            WriteMessage("Get " + randomQuery);
            HubProxy.Invoke("GetRecords", randomQuery);
            HubProxy.On<string>("Response", (message) => WriteMessage(String.Format("{0}\r", message)));
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
