using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System.Linq;

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
            var response = Program.dbService.Read(query);
            if (response.Count == 0)
            {
                Clients.Caller.Response("No result");
            }
            else
            {
                response.ForEach(s => { Clients.All.Response(s); Console.WriteLine(s); });
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
