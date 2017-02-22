using System.IO;
using System.Xml.Serialization;

namespace FTPPlugin
{
    internal static class SettingsManager
    {
        private static readonly string fileName = "ftpsettings.xml";

        public static void Save(FTPSettings settings)
        {
            using (var stream = File.Open(fileName, FileMode.Create))
            {
                var s = new XmlSerializer(typeof(FTPSettings));

                s.Serialize(stream, settings);
            }
        }

        public static FTPSettings Load()
        {
            using (var stream = File.Open(fileName, FileMode.Open))
            {
                var s = new XmlSerializer(typeof(FTPSettings));

                return (FTPSettings)s.Deserialize(stream);
            }
        }
    }
}
