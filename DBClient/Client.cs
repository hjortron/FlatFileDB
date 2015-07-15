using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Microsoft.AspNet.SignalR.Client;
using AppSettings = DBClient.Properties.Settings;

namespace DBClient
{
    static class Client
    {
        private static HubConnection Connection { get; set; }
        private static IHubProxy HubProxy { get; set; }
        private static Timer _timer;
        private static bool _isConnected;

        public static void ConnectAsync()
        {
            Connection = new HubConnection(AppSettings.Default.ServerUrl);
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");
            try
            {
                Connection.Start();
                _isConnected = true;
            }
            catch (HttpRequestException)
            {
                WriteMessage("Unable to connect to server: Start server before connecting clients.");
                _isConnected = false;
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
            var random = new Random();
            const int dataLength = 128;
            var array = new byte[dataLength * random.Next(1, 3)];
            random.NextBytes(array);
            SendRecord(random.Next(1, 1000), random.Next(1, 10));            
        }

        public static void SendRecord(int sourceId, int sourceType)
        {
            var random = new Random();
            const int dataLength = 128;
            var array = new byte[dataLength * random.Next(1, 3)];
            random.NextBytes(array);
            HubProxy.Invoke("WriteRecord", sourceId, sourceType, array);
        }

        public static void GetRandomRecords()
        {
            var random = new Random();
            GetRecords(FormatQueryString(random.Next(1, 1000), random.Next(1, 10)));           
        }

        public static string FormatQueryString(int? sourceId = null, int? sourceType = null)
        {
            var queryBuilder = new StringBuilder();
            if (sourceId != null)
            {
                queryBuilder.Append(string.Format("sourceid = {0}", sourceId));
            }
            if (sourceType != null)
            {
                if (queryBuilder.Length > 0)
                {
                    queryBuilder.Append(" AND ");
                }
                queryBuilder.Append(string.Format("sourcetype = {0}", sourceType));
            }
            return queryBuilder.ToString();
        }

        public async static void GetRecords(string query)
        {
            WriteMessage("Get " + query);
            await HubProxy.Invoke("GetRecords", query);
            HubProxy.On<List<string>>("Response", message => message.ForEach(item => WriteMessage(string.Format("{0}\r", item))));
        }

        private static void WriteMessage(string message)
        {
            var dispatcher = Application.Current.Dispatcher;
            dispatcher.Invoke(() =>((MainWindow)Application.Current.MainWindow).WriteToConsole(message));
        }

        private static void Connection_Closed()
        {
            _isConnected = false;
            WriteMessage("Disconnected!");
        }
    }
}
