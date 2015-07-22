using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nancy;
using Nancy.IO;

namespace FileUploader.http
{
    public class Index : NancyModule
    {
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public Index()
        {
            Post["meta.aa"] = par =>
            {
                try
                {
                    var content = SteramToString(Request.Body).Split('&');
                    Console.WriteLine("Content : " + content);
                    var guid = Guid.Parse(content[0]);
                    HttpFile file = Program.Dict[guid];
                    if (file == null)
                        return Response.AsText("-1");
                    var data = ReadFully(file.Value);
                    Console.WriteLine($"Name : {file.Name} Lenght : {data.Length}");
                    //file.Value.Read(data, 0, data.Length);
                    var path = $"./user/{content[1]}/{guid.ToString()}_{file.Name}";
                    if (!Directory.Exists($"./user/{content[1]}"))
                        Directory.CreateDirectory($"./user/{content[1]}");
                    Console.WriteLine("Writing to " + path);
                    File.WriteAllBytes(path, data);
                    Console.WriteLine("Writed " + path);
                    Console.WriteLine(path.Substring(2));
                    Program.Dict.TryRemove(guid, out file);
                    return Response.AsText(path.Substring(2));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return Response.AsText("-1");
                }

            };
            Post["file.aa"] = par =>
            {
                try
                {
                    if (Request.Body.Length > 500000000)
                        return Response.AsText("-1");
                    var ff = DeepClone(Request.Files.FirstOrDefault());
                    var guid = Guid.NewGuid();
                    Program.Dict.AddOrUpdate(guid, ff, (guid1, httpFile) => ff);
                    Console.WriteLine($"Retrived file with {guid} and name {ff.Name}");
                    return Response.AsText(guid.ToString());
                }
                catch (Exception ex)
                {
                    return Response.AsText(ex.ToString());
                }
            };
        }
        public static T DeepClone<T>(T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
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
