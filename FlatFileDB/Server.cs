using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

namespace FlatFileDB
{
    internal static class Server
    {
        private static IDisposable SignalR { get; set; }

        public static void StartServer()
        {
            try
            {
                SignalR = WebApp.Start("http://localhost:8080");
            }
            catch (TargetInvocationException)
            {
                //Dispatcher.Invoke(() =>
                //Console.WriteLine("A server is already running"));
                //Dispatcher.Invoke(() => ((MainWindow)Application.Current.MainWindow).buttonStart.IsEnabled = true);
                //return;
            }
            //Dispatcher.Invoke(() => .buttonStop.IsEnabled = true));
            //Dispatcher.Invoke(() =>
            //((MainWindow)Application.Current.MainWindow).WriteToConsole("Server started at " + serverName));
        }
    }

    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }

    public class MyHub : Hub
    {
        public void WriteRecord(int sourceId, int sourceType, byte[] data)
        {
            Program.DbService.AddRecord(sourceId, sourceType, data);
        }

        public void GetRecords(string query)
        {
            var result = Program.DbService.Read(query);
            if (result.Count == 0)
            {
                Clients.Caller.Response("No result");
            }
            else
            {
                Clients.Caller.Response(result);
            }
        }

        public override Task OnConnected()
        {
            Console.WriteLine("Client connected: " + Context.ConnectionId);

            return base.OnConnected();
        }

        public void OnDisconnected()
        {
            Console.WriteLine("Client disconnected: " + Context.ConnectionId);
        }
    }
}