using System;
using System.Threading.Tasks;
using AppSettings = FlatFileDB.Properties.Settings;

namespace FlatFileDB
{
    static class Program
    {         
        public static readonly DataBaseManager DataBaseManager = new DataBaseManager();

        static void Main(string[] args)
        {                                    
            Console.WriteLine("Starting server...");
            Task.Run(() => Server.StartServer());           
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            //InsertRandomRecords(100000); 
   
            Console.ReadLine();
        }

        static void OnProcessExit(object sender, EventArgs e)
        {          
            DataBaseManager.SaveData();
        }

        static void InsertRandomRecords(int count)
        {
            var random = new Random();
            var dataLength = AppSettings.Default.DataMinLength;
            for (var i = 0; i < count; i++)
            {
                var array = new byte[dataLength * random.Next(1, 3)];
                random.NextBytes(array);
                DataBaseManager.AddRecord(random.Next(1, 1000), random.Next(1, 10), array);
            }   
        }
    }
}
