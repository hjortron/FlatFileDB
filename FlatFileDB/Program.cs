using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatFileDB.Model;

namespace FlatFileDB
{
    class Program
    {         
        public static DBService dbService = new DBService();

        static void Main(string[] args)
        {                                    
            Console.WriteLine("Starting server...");
            Task.Run(() => Server.StartServer());           
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            var random = new Random();
            var dataLength = 128;
            for (var i = 0; i < 100000; i++)
            {
                byte[] array = new byte[dataLength * random.Next(1, 3)];
                random.NextBytes(array);
                dbService.AddRecord(random.Next(1, 1000), random.Next(1, 10), array);
            }         
            Console.ReadLine();
        }

        static void OnProcessExit(object sender, EventArgs e)
        {          
            dbService.FlushData();
        }        
    }
}
