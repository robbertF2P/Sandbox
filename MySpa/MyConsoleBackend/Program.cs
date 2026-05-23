using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using MyBackend;

namespace MyConsoleBackend
{
    class Program
    {
        static void Main(string[] args)
        {
            // create a group of worker objects
            var workers = new BlockingCollection<MyWorker>();
            for (int i = 0; i < 10; i++)
            {
                workers.Add(new MyWorker());
            }
            // create the bus
            var bus = RabbitHutch.CreateBus("host=localhost");
            // respond to requests
            bus.RespondAsync<RequestServerTime, ResponseServerTime>(request =>
                Task.Factory.StartNew(() =>
                {
                    var worker = workers.Take();
                    try
                    {
                        return worker.Execute(request);
                    }
                    finally
                    {
                        workers.Add(worker);
                    }
                }));
            Console.ReadLine();
            bus.Dispose();
        }
    }

    internal class MyWorker
    {
        public ResponseServerTime Execute(RequestServerTime request)
        {
            Console.WriteLine("executed");
            return new ResponseServerTime();
        }
    }
}
