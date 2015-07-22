using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nancy.Hosting.Self;

namespace FileUploader.http
{
    class Program
    {

        static void Main(string[] args)
        {
            var _boot = new WebBootstrapper();
            var conf = new HostConfiguration { RewriteLocalhost = true, UrlReservations = new UrlReservations() { CreateAutomatically = true } };
            var _host = new NancyHost(_boot, conf, new Uri("http://localhost:7752/"));
            _host.Start();
            Console.ReadKey();
        }
    }
}
