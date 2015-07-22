using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nancy;
using Nancy.IO;

namespace FileUploader.http
{
    public class Index : NancyModule
    {
        public static ConcurrentDictionary<Guid,HttpFile> Dict = new ConcurrentDictionary<Guid, HttpFile>(); 
        public Index()
        {
            Post["meta.aa"] = par =>
            {
                try
                {
                    var content = SteramToString(Request.Body).Split('&');
                    Console.WriteLine("Content : " + content);
                    var guid = Guid.Parse(content[0]);
                    HttpFile file;
                    Dict.TryRemove(guid, out file);
                    if (file == null)
                        return Response.AsText("-1");
                    var data = new byte[file.Value.Length];
                    file.Value.Read(data, 0, (int)file.Value.Length);
                    var path = $"./user/{content[1]}/{guid.ToString()}_{file.Name}";
                    if (!Directory.Exists($"./user/{content[1]}"))
                        Directory.CreateDirectory($"./user/{content[1]}");
                    File.WriteAllBytes(path, data);

                    return Response.AsText(path.Substring(2));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    return Response.AsText("-1");
                }

            };
            Post["file.aa"] = par =>
            {
                try
                {
                    if (Request.Body.Length > 500000000)
                        return Response.AsText("-1");
                    var ff = Request.Files.FirstOrDefault();
                    var guid = Guid.NewGuid();
                    Dict.AddOrUpdate(guid, ff, (guid1, httpFile) => httpFile);
                    Console.WriteLine($"Retrived file with {guid} and name {ff.Name}");
                    return Response.AsText(guid.ToString());
                }
                catch (Exception)
                {
                    return Response.AsText("-1");
                }
            };
        }
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        public static string SteramToString(RequestStream body)
        {
            using (var reader = new StreamReader(body))
            {
                return reader.ReadToEnd();
            }
        }
        public static byte[] SteramToBytes(RequestStream body)
        {
            var bb = new byte[body.Length];
            body.Read(bb, 0, (int)body.Length);
            return bb;
        }
    }
}
