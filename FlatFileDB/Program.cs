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
        static DBService dbService = new DBService();

        static void Main(string[] args)
        {
            var random = new Random();
            //var dataLength = 128;
            //byte[] array = new byte[dataLength];

            //for (var i = 0; i < 100000; i++)
            //{
            //    random.NextBytes(array);
            //    dbService.AddRecord(random.Next(1, 1000), random.Next(1, 10), array);
            //}
            dbService.Read("sourceid = " + random.Next(1, 1000) + " AND sourcetype = "+random.Next(1, 10));
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);            
            Console.ReadKey();
        }

        static void OnProcessExit(object sender, EventArgs e)
        {          
            dbService.FlushData();
        }        
    }
}
