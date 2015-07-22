using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nancy;
using Nancy.Hosting.Self;

namespace FileUploader.http
{
    class Program
    {
        public static ConcurrentDictionary<Guid, HttpFile> Dict = new ConcurrentDictionary<Guid, HttpFile>();
        static void StartNancy()
        {
            var _boot = new WebBootstrapper();
            var conf = new HostConfiguration { RewriteLocalhost = true, UrlReservations = new UrlReservations() { CreateAutomatically = true } };
            var _host = new NancyHost(_boot, conf, new Uri("http://localhost:7752/"));
            _host.Start();
        }
        static void Main(string[] args)
        {
            new Thread(() => StartNancy()).Start();
            try
            {
                while (true)
                {
                    Thread.Sleep(10000000);
                }
                Console.WriteLine(Directory.GetCurrentDirectory());

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
            }
            Console.WriteLine("Completed");

        }
    }
}
