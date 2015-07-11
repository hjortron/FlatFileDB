using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

namespace FlatFileDB
{
    class Server
    {
        public static IDisposable SignalR { get; set; }

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
 
    class Startup
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
            Program.dbService.AddRecord(sourceId, sourceType, data);
        }

        public void GetRecords(string query)
        {
            var records = Program.dbService.Read(query);
            Clients.Caller.WriteRecords(records);
        }
        public override Task OnConnected()
        {
            //Use Application.Current.Dispatcher to access UI thread from outside the MainWindow class
            Console.WriteLine("Client connected: " + Context.ConnectionId);

            return base.OnConnected();
        }
        public void OnDisconnected()
        {
            //Use Application.Current.Dispatcher to access UI thread from outside the MainWindow class
            Console.WriteLine("Client disconnected: " + Context.ConnectionId);            
        }
    }
}
