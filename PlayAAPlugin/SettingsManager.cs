using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PlayAAPlugin
{
    static class SettingsManager
    {
        private static string fileName = "play-aa.xml";

        public static void Save(PlayAASettings settings)
        {
            using (FileStream stream = File.Open(fileName, FileMode.Create))
            {
                XmlSerializer s = new XmlSerializer(typeof(PlayAASettings));
                s.Serialize(stream, settings);
            }
        }

        public static PlayAASettings Load()
        {
            try
            {
                using (FileStream stream = File.Open(fileName, FileMode.Open))
                {
                    XmlSerializer s = new XmlSerializer(typeof(PlayAASettings));
                    return (PlayAASettings)s.Deserialize(stream);
                }
            }
            catch (Exception)
            {
               Save(new PlayAASettings());
               return Load();
            }

        }
    }
}
