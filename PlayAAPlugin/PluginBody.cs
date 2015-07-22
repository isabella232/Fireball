using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PlayAAPlugin;

namespace Fireball.Plugin
{
    public class PluginBody : IPlugin
    {
        public string Name
        {
            get { return "play-aa.net"; }
        }

        public Single Version
        {
            get { return 1.0f; }
        }

        public bool HasSettings
        {
            get { return false; }
        }

        public void ShowSettings()
        {
            throw new NotImplementedException();
        }
        public string Get5CharacterRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path.Substring(0, 5);  // Return 8 character string
        }
        public string Upload(byte[] image, string filename, bool isFile)
        {
            var set = SettingsManager.Load();
            if (!isFile)
                filename = $"{Get5CharacterRandomString()}.png";
            else filename = Path.GetFileName(filename);
            var path = $".{Get5CharacterRandomString()}_{filename}";
            File.WriteAllBytes(path,image);
            string str;
            using (var web = new WebClient())
            {
                byte[] resp = web.UploadFile("http://usercontent.play-aa.net/file.aa", path);
                File.Delete(path);
                var resp2 = Encoding.UTF8.GetString(resp);
                str = web.UploadString("http://usercontent.play-aa.net/meta.aa", $"{resp2}&{set.UserName}");
            }
            if (str == "-1")
                return "Failed";
            return $"http://usercontent.play-aa.net/{str}";
        }

    }
}
